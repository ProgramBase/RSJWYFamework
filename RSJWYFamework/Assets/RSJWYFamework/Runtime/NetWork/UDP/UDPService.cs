using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.NetWork.Base;
using RSJWYFamework.Runtime.NetWork.Public;
using UnityEngine;

namespace RSJWYFamework.Runtime.NetWork.UDP
{
    internal class UDPService
    {
        internal ISocketUDPController SocketUDPController;
        
        /// <summary>
        /// 监听端口
        /// </summary>
        int _port = 5000;
        /// <summary>
        /// 监听IP
        /// </summary>
        IPAddress _ip ;
        /// <summary>
        /// UDP Socket
        /// </summary>
        System.Net.Sockets.Socket _udpClient;
        /// <summary>
        /// 存储接收到的数据
        /// </summary>
        byte[] _ReceiveData = new byte[8192];
        /// <summary>
        /// 消息发送队列
        /// </summary>
        ConcurrentQueue<UDPMsg> _SendMsgQueue = new ();
        /// <summary>
        /// 是否初始化过
        /// </summary>
        bool _isInit;
        /// <summary>
        /// 消息发送线程
        /// </summary>
        Thread _sendMsgThread;
        /// <summary>
        ///  消息队列发送锁
        /// </summary>
        object _msgSendThreadLock = new object();
        /// <summary>
        /// 通知多线程自己跳出
        /// </summary>
        private static CancellationTokenSource _cts;

        /// <summary>
        /// 写
        /// </summary>
        private SocketAsyncEventArgs _read;
        /// <summary>
        /// 读
        /// </summary>
        private SocketAsyncEventArgs _write;
        

        /// <summary>
        /// 传入指定IP
        /// </summary>
        /// <param name="_ip"></param>
        /// <param name="_port"></param>
        public void Init(string _ip, int _port)
        {
            this._ip = IPAddress.Parse(_ip);
            this._port = _port;
            Init();
        }
        /// <summary>
        /// 使用设置好的IPAddress
        /// </summary>
        /// <param name="_ipAddress"></param>
        /// <param name="_port"></param>
        public void Init(IPAddress _ipAddress, int _port)
        {
            _ip = _ipAddress;
            this._port = _port;
            Init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="_port"></param>
        public void Init()
        {
            if (_isInit)
            {
                return;
            }
            try
            {
                _udpClient = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                //支持广播消息
                _udpClient.EnableBroadcast = true;
                //配置监听
                IPEndPoint ipendpoint = new IPEndPoint(_ip, _port);
                _udpClient.Bind(ipendpoint);
                //开启异步监听
                _sendMsgThread = new Thread(SendMsgThread);
                _sendMsgThread.IsBackground = true;//后台运行
                _sendMsgThread.Start();
                _isInit = true;

                RSJWYLogger.Log($"UDP启动监听 {_udpClient.LocalEndPoint.ToString()} ");
                Start();
            }
            catch (Exception e)
            {
                RSJWYLogger.Error($"UDP启动监听 {_udpClient.LocalEndPoint.ToString()} 失败！！错误信息：\n {e.ToString()}");
            }
        }
        /// <summary>
        /// 开启接收
        /// </summary>
        private void Start()
        {
            _read = new SocketAsyncEventArgs();
            _read.SetBuffer(new byte[10240], 0, 10240);
            _read.Completed += IO_Completed; 
            
            _write = new SocketAsyncEventArgs();
            _write.Completed += IO_Completed;

            if (!_udpClient.ReceiveFromAsync(_read))
                Task.Run(() => ProcessReceived(_read));
        }
        /// <summary>
        /// 回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // SocketAsyncEventArgs回调处理
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.ReceiveFrom:
                    Task.Run(() => ProcessReceived(e));
                    break;
                case SocketAsyncOperation.SendTo:
                    Task.Run(() => ProcessSend(e));
                    break;
                default:
                    RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer, $"UDP IO_Completed 在套接字上完成的最后一个操作不是接收或发送，{e.LastOperation}");
                    break;
            }
        }
        /// <summary>
        /// UDP消息接收
        /// </summary>
        /// <param name="e"></param>
        private void ProcessReceived(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    byte[] data = new byte[e.BytesTransferred];
                    Buffer.BlockCopy(_ReceiveData, 0, data, 0, e.BytesTransferred);
                    var msg = new UDPMsg
                    {
                        Bytes =data,
                        remoteEndPoint = e.RemoteEndPoint as IPEndPoint
                    };
                    SocketUDPController.ReceiveMsgCallBack(msg);//交给执行回调
                }
                else
                {
                    RSJWYLogger.Error(RSJWYFameworkEnum.NetworkUDP, $"UDP ProcessReceived error: {e.SocketError}");
                }
            }
            catch (Exception exception)
            {
                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkUDP, $"UDP ProcessReceived exception: {exception}");
            }
            finally
            {
                //无论是否异常，重设缓冲区，接收下一组数据
                e.SetBuffer(0,e.Buffer.Length);
                if (!_udpClient.ReceiveFromAsync(e))
                    Task.Run(() => ProcessReceived(e));
            }
        }
        
        
        /// <summary>
        /// 发送UDP消息
        /// 调用者必须确保消息的
        /// </summary>
        /// <param name="ipAddress">目标IP地址</param>
        /// <param name="port">目标端口</param>
        /// <param name="message">要发送的消息内容</param>
        public void SendUdpMessage(string ipAddress, int port, byte[] message)
        {
            if (_udpClient == null)
            {
                RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkUDP, "UDP服务未初始化或已关闭，无法发送消息。");
                return;
            }
            //创建消息容器
            var sendMsg = new UDPMsg
            {
                Bytes = message,
                remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port)
            };

            // 发送数据
            _SendMsgQueue.Enqueue(sendMsg);
        }
        /// <summary>
        /// 消息发送完成回调
        /// </summary>
        /// <param name="e"></param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError== SocketError.Success && e.BytesTransferred > 0)
                {
                    _SendMsgQueue.TryDequeue(out var _msg);
                    if (_msg.Bytes.Length!= e.BytesTransferred)
                    {
                        RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkUDP, $"ProcessSend UDP消息发送错误！！！已发送长度和消息本体长度不同");
                    }
                    //本条消息发送完成，激活线程
                    lock (_msgSendThreadLock)
                    {
                        Monitor.Pulse(_msgSendThreadLock);
                    }
                }
                else
                {
                    RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkUDP, $" ProcessSend UDP消息发送错误！！！SocketError：{e.SocketError}");
                }
            }
            catch (Exception ex)
            {

                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkUDP,$" ProcessSend UDP消息发送发生异常！：{ex}");
            }
        }
        /// <summary>
        /// 消息发送监控线程
        /// </summary>
        void SendMsgThread()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    if (_SendMsgQueue.Count <= 0)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    //取出数据
                    UDPMsg _data;
                    //取出不移除
                    _SendMsgQueue.TryPeek(out _data);
                    //绑定消息
                    _write.SetBuffer(_data.Bytes, 0, _data.Bytes.Length);
                    _write.RemoteEndPoint = _data.remoteEndPoint;
                    lock (_msgSendThreadLock)
                    {
                        if (!_udpClient.SendToAsync(_write))
                            Task.Run(() => ProcessSend(_write));
                        Monitor.Wait(_msgSendThreadLock);
                    }
                }
                catch (Exception e)
                {
                    RSJWYLogger.Error(RSJWYFameworkEnum.NetworkUDP,$"SendMsgThread 线程处理异常！！：{e}");
                    // 重启线程
                    Thread.Sleep(1000); // 等待一段时间再重启，避免立即重启可能导致的问题
                    if (_cts.IsCancellationRequested)
                    {
                        RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkUDP, $"请求取消任务");
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 关闭监听
        /// </summary>
        public void Close()
        {
            if (_udpClient != null)
            {
                _udpClient.Close();
            }
            _isInit = false;
        }

    }
}
