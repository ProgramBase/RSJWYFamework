using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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
        int port = 5000;
        /// <summary>
        /// 监听IP
        /// </summary>
        IPAddress ip = IPAddress.Any;
        /// <summary>
        /// UDP Socket
        /// </summary>
        System.Net.Sockets.Socket udpClient;
        /// <summary>
        /// 存储消息来源客户端信息
        /// </summary>
        EndPoint clientEndPoint;
        /// <summary>
        /// 存储接收到的数据
        /// </summary>
        byte[] ReceiveData = new byte[8192];
        /// <summary>
        /// 接收到的消息数组队列
        /// </summary>
        ConcurrentQueue<byte[]> reciveByteMsgQueue = new ConcurrentQueue<byte[]>();

        /// <summary>
        /// 处理完毕的待执行的消息队列
        /// </summary>
        ///ConcurrentQueue<string> MsgQueue = new();
        /// <summary>
        /// 消息发送队列
        /// </summary>
        ConcurrentQueue<UDPSend> SendMsgQueue = new ();
        /// <summary>
        /// 通知多线程自己跳出
        /// </summary>
        bool isThreadOver;
        /// <summary>
        /// 是否初始化过
        /// </summary>
        bool isInit;
        /// <summary>
        /// 消息处理线程
        /// </summary>
        Thread MsgThread;
        /// <summary>
        /// 消息发送线程
        /// </summary>
        Thread sendMsgThread;
        /// <summary>
        ///  消息队列发送锁
        /// </summary>
        object msgSendThreadLock = new object();
        

        /// <summary>
        /// 传入指定IP
        /// </summary>
        /// <param name="_ip"></param>
        /// <param name="_port"></param>
        public void Init(string _ip, int _port)
        {
            ip = IPAddress.Parse(_ip);
            port = _port;
            Init();
        }
        /// <summary>
        /// 使用设置好的IPAddress
        /// </summary>
        /// <param name="_ipAddress"></param>
        /// <param name="_port"></param>
        public void Init(IPAddress _ipAddress, int _port)
        {
            ip = _ipAddress;
            port = _port;
            Init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="_port"></param>
        public void Init()
        {
            if (isInit)
            {
                return;
            }

            try
            {
                isThreadOver = false;
                udpClient = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                //配置监听
                IPEndPoint ipendpoint = new IPEndPoint(ip, port);
                udpClient.Bind(ipendpoint);
                clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                //开启异步监听
                udpClient.BeginReceiveFrom(ReceiveData, 0, ReceiveData.Length,
                    SocketFlags.None, ref clientEndPoint, ReceiveMessageToQueue, clientEndPoint);

                //MsgThread = new Thread(Msghandle);
                //MsgThread.IsBackground = true;//后台运行
                //MsgThread.Start();
                sendMsgThread = new Thread(SendMsgHandle);
                sendMsgThread.IsBackground = true;//后台运行
                sendMsgThread.Start();

                isInit = true;
                Debug.Log($"UDP启动监听{udpClient.LocalEndPoint.ToString()}成功");
            }
            catch (Exception e)
            {
                Debug.LogError($"UDP启动监听 {udpClient.LocalEndPoint.ToString()} 失败！！错误信息：\n {e.ToString()}");
                isThreadOver = true;
            }
        }

        /// <summary>
        /// 接收消息并存放 
        /// </summary>
        /// <param name="iar"></param>
        void ReceiveMessageToQueue(IAsyncResult iar)
        {
            if (isThreadOver)
            {
                return;
            }
            try
            {
                int receiveDataLength = udpClient.EndReceiveFrom(iar, ref clientEndPoint);
                //如果有数据
                if (receiveDataLength > 0)
                {
                    byte[] data = new byte[receiveDataLength];
                    Buffer.BlockCopy(ReceiveData, 0, data, 0, receiveDataLength);
                    reciveByteMsgQueue.Enqueue(data);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"UDP启动监听 {udpClient.LocalEndPoint.ToString()} 失败！！错误信息：\n {e.ToString()}");
            }
            finally
            {
                //继续监听
                udpClient.BeginReceiveFrom(ReceiveData, 0, ReceiveData.Length,
                    SocketFlags.None, ref clientEndPoint, ReceiveMessageToQueue, clientEndPoint);
            }
        }
        /// <summary>
        /// 关闭监听
        /// </summary>
        public void Close()
        {
            isThreadOver = true;
            if (udpClient != null)
            {
                udpClient.Close();
            }
            isInit = false;
        }
        /// <summary>
        /// 数据处理
        /// </summary>
        internal void Msghandle()
        {
            while (true)
            {
                if (isThreadOver)
                {
                    return;
                }
                if (!reciveByteMsgQueue.IsEmpty)
                {
                    //取出数据
                    byte[] _data;
                    reciveByteMsgQueue.TryDequeue(out _data);
                    //处理数据，分别尝试不同协议
                    //string _strASCII = Encoding.ASCII.GetString(_data);
                    SocketUDPController.ReceiveMsgCallBack(_data);
                }
            }
        }

        public void UnityUpdate()
        {
            /*//线程是否出错
            if (sendMsgThread != null)
            {
                if (sendMsgThread.IsAlive == false && isInit == true)
                {
                    //重新创建本线程
                    sendMsgThread = new Thread(Msghandle);
                    sendMsgThread.IsBackground = true;//后台运行
                    sendMsgThread.Start();
                }
            }*/
            
            //取出消息
            if (!reciveByteMsgQueue.IsEmpty)
            {
                byte[] msgBase = null;
                //取出并移除数据
                if (reciveByteMsgQueue.TryDequeue(out msgBase))
                {
                    SocketUDPController.ReceiveMsgCallBack(msgBase);//交给执行回调
                }
                else
                {
                    RSJWYLogger.LogError(RSJWYFameworkEnum.NetworkUDP,$"非正常错误！取出UDP消息队列失败！！");
                }
            }
        }
        void SendMsgHandle()
        {
            if (isThreadOver)
            {
                return;
            }
            try
            {
                if (!SendMsgQueue.IsEmpty)
                {
                    //取出数据
                    UDPSend _data;
                    SendMsgQueue.TryDequeue(out _data);
                    //处理数据，分别尝试不同协议
                    //string _strASCII = Encoding.ASCII.GetString(_data);
                    //SocketUDPController.ReceiveMsgCallBack(_data);
                    lock (msgSendThreadLock)
                    {
                        if (udpClient != null && udpClient.Connected)
                        {
                            udpClient.BeginSendTo(_data.Bytes, 0, _data.length, SocketFlags.None, _data.remoteEndPoint, new AsyncCallback(SendCallback), udpClient);
                        }
                        bool istimeout = Monitor.Wait(msgSendThreadLock, 10000);
                        if (!istimeout)
                        {
                            RSJWYLogger.LogWarning(RSJWYFameworkEnum.NetworkTcpClient,$"客户端消息：消息发送时间超时（超过10s），请检查网络质量，关闭本客户端的链接");
                            Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // 重启线程
                Thread.Sleep(1000); // 等待一段时间再重启，避免立即重启可能导致的问题
                sendMsgThread.Interrupt(); // 打断当前线程
                sendMsgThread = new Thread(SendMsgHandle);
                sendMsgThread.Start();
            }
        }
        private void SendCallback(IAsyncResult ar)
        {
            // 从异步操作中获取Socket
            UDPSend msg;
            System.Net.Sockets.Socket socket = (System.Net.Sockets.Socket)ar.AsyncState;
            if (socket == null || !socket.Connected)
            {
                return;
            }
            // 完成异步发送操作
            int bytesSent = socket.EndSendTo(ar);
            
            SendMsgQueue.TryPeek(out msg);
            msg.ReadIndex += bytesSent;
            if (msg.length == 0)//代表发送完整
            {
                SendMsgQueue.TryDequeue(out var _);//取出但不使用，只为了从队列中移除
                msg = null;//发送完成，置空
            }
            if (msg != null)
            {
                socket.BeginSend(msg.Bytes, msg.ReadIndex, msg.length, 0, SendCallback, socket);
            }
            else
            {
                //本条消息发送完成，激活线程
                lock (msgSendThreadLock)
                {
                    Monitor.Pulse(msgSendThreadLock);
                }
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
            if (isThreadOver || udpClient == null)
            {
               RSJWYLogger.LogWarning(RSJWYFameworkEnum.NetworkUDP,"UDP服务未初始化或已关闭，无法发送消息。");
               return;
            }
            // 创建目标端点
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            var sendMsg = new UDPSend(message);
            
            // 发送数据
            SendMsgQueue.Enqueue(sendMsg);
        }
        
    }
}
