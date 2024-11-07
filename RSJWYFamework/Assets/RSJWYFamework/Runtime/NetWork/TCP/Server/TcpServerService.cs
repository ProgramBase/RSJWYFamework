﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.NetWork.Base;
using RSJWYFamework.Runtime.Network.Public;
using RSJWYFamework.Runtime.NetWork.Public;
using RSJWYFamework.Runtime.Pool;

namespace RSJWYFamework.Runtime.NetWork.TCP.Server
{
    /// <summary>
    /// 网络链接事件枚举
    /// </summary>
    public enum NetServerStatus
    {
        None ,
        /// <summary>
        /// 正在打开监听
        /// </summary>
        OpenListening,
        /// <summary>
        /// 正在监听
        /// </summary>
        Listen,
        /// <summary>
        /// 正在关闭
        /// </summary>
        CloseListening,
        /// <summary>
        /// 关闭
        /// </summary>
        Close,
        /// <summary>
        /// 发生错误，无法监听，参数设置问题
        /// </summary>
        Fail
    }
    public class TcpServerService
    {  
        
        
        #region 字段

        private NetServerStatus _status;

        public NetServerStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    TcpServerController?.ServerServiceStatus(_status);
                }
            }
        }
        
        /// <summary>
        /// 绑定的服务端控制器
        /// </summary>
        internal ISocketTCPServerController TcpServerController;
        /// <summary>
        /// 消息体加密接口
        /// </summary>
        internal ISocketMsgBodyEncrypt  m_MsgBodyEncrypt;
        /// <summary>
        /// 消息体编码接口
        /// </summary>
        internal ISocketMsgEncode m_SocketMsgEncode;
        /// <summary>
        /// 监听端口
        /// </summary>
        static int port = 5236;

        /// <summary>
        /// 监听IP
        /// </summary>
        static IPAddress ip = IPAddress.Any;

        /// <summary>
        /// 心跳包间隔时间
        /// </summary>
        internal static long pingInterval = 60;

        /// <summary>
        /// 服务器监听Socket
        /// </summary>
        static System.Net.Sockets.Socket ListenSocket;


        /// <summary>
        /// 客户端容器字典
        /// </summary>
        internal ConcurrentDictionary<System.Net.Sockets.Socket, ClientSocketToken> ClientDic = new ConcurrentDictionary<System.Net.Sockets.Socket, ClientSocketToken>();

        /// <summary>
        /// 服务器接收的消息队列
        /// </summary>
        static ConcurrentQueue<ClientMsgContainer> serverMsgQueue = new();


        /// <summary>
        /// 消息处理线程
        /// </summary>
        static Thread msgThread;
        /// <summary>
        /// 心跳包监测线程
        /// </summary>
        static Thread pingThread;

        /// <summary>
        /// 通知多线程自己跳出
        /// </summary>
        private static CancellationTokenSource cts;

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        bool isInit = false;
        
        /// <summary>
        /// 已连接客户端数量
        /// </summary>
        int m_clientCount;

        /// <summary>
        /// SocketAsyncEventArgs池-读写
        /// </summary>
        private SocketAsyncEventPool RWSocketAsynEA;
        
        
        
        #endregion

        #region 初始化

        /// <summary>
        /// 使用指定IP
        /// </summary>
        /// <param name="_ip"></param>
        /// <param name="_port"></param>
        internal void Init(string _ip, int _port)
        {
            ip = IPAddress.Parse(_ip);
            port = _port;
            SetInit();
        }
        /// <summary>
        /// 使用设置好的IPAddress
        /// </summary>
        /// <param name="_ipAddress"></param>
        /// <param name="_port"></param>
        internal void Init(IPAddress _ipAddress, int _port)
        {
            ip = _ipAddress;
            port = _port;
            SetInit();
        }

        internal void Init()
        {
            if (!isInit)
            {
                RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer,$":没有初始化过，无法初始化指令");
                return;
            }
            SetInit();
        }

        /// <summary>
        /// 初始化监听
        /// </summary>
        void SetInit()
        {
            RSJWYLogger.Log(RSJWYFameworkEnum.NetworkTcpServer,$"服务端启动参数，IP：{ip.ToString()}，port：{port}");
            Status = NetServerStatus.OpenListening;
            //初始化池
            RWSocketAsynEA = new(
                (_obj) =>
                {
                    _obj.Completed += IO_Completed;
                },
                (_obj) =>
                {
                    _obj.Dispose();
                },
                (_obj) =>
                {
                    var _buffer = new byte[10485760]; 
                    _obj.SetBuffer(_buffer,0,_buffer.Length);
                },
                (_obj) =>
                {
                    _obj.UserToken = null;
                    _obj.SetBuffer(null,0,0);
                },
                100,100);
            //配置连接信息
            try
            {
                //监听IP
                IPAddress m_ip = ip;
                //设置监听端口
                IPEndPoint ipendpoint = new IPEndPoint(m_ip, port);
                //创建Socket并设置模式
                ListenSocket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //绑定监听Socket监听
                ListenSocket.Bind(ipendpoint);
                //链接数量限制
                ListenSocket.Listen(100);
                //启动线程
                cts = new ();
                //消息处理线程-后台处理
                msgThread = new Thread(() => MsgThread(cts.Token));
                msgThread.IsBackground = true;//后台运行
                msgThread.Start();
                StartAccept(null);
            }
            catch (Exception e)
            {
                Status = NetServerStatus.Fail;
                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$" 服务端启动监听 IP:{ip}，Port:{port}失败！！错误信息：\n {e}");
            }

        }

        #endregion

        #region 功能处理

        /// <summary>
        /// 开启接受连接请求
        /// </summary>
        /// <param name="acceptEventArg"></param>
        public void StartAccept(SocketAsyncEventArgs acceptEventArg)  
        {  
            if (acceptEventArg == null)  
            {  
                //没有，创建一个新的
                acceptEventArg = new SocketAsyncEventArgs();  
                acceptEventArg.Completed += (sender, e) =>
                {
                    Task.Run(() => ProcessAccept(e));
                };  
            }  
            else  
            {  
                // socket 必须清除，因为 Context 对象正在被重用
                acceptEventArg.AcceptSocket = null;  
            }  
            try
            {
                //绑定时必须检查，绑定时已完成不会触发回调
                //https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socket.acceptasync?view=netframework-4.8.1#system-net-sockets-socket-acceptasync(system-net-sockets-socketasynceventargs)
                if (!ListenSocket.AcceptAsync(acceptEventArg))
                    Task.Run(() => ProcessAccept(acceptEventArg));
                Status = NetServerStatus.Listen;
                //输出日志
                RSJWYLogger.Log(RSJWYFameworkEnum.NetworkTcpServer,$"服务端开启监听：{ListenSocket.LocalEndPoint.ToString()}");
                
            }
            catch (Exception e)
            {
                Status = NetServerStatus.Fail;
                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$" AcceptAsync监听时发生异常\n {e}");
            }
        } 
        /// <summary>
        /// 接受连接请求
        /// </summary>
        private void ProcessAccept(SocketAsyncEventArgs socketAsyncEArgs)  
        {
            
            // 接受下一个连接
            if (socketAsyncEArgs.SocketError == SocketError.OperationAborted)
            {
                RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer,$"socket操作中止，不再接收新的连接请求；{socketAsyncEArgs.SocketError}");
                return;
            }
            try
            {
                Interlocked.Increment(ref m_clientCount); //计数 //获取一个空闲的  

                //创建并存储客户端信息
                var clientToken = new ClientSocketToken
                {
                    lastPingTime = Utility.Utility.GetTimeStamp(),
                    socket = socketAsyncEArgs.AcceptSocket,
                    ReadBuff = new(),
                    readSocketAsyncEA = RWSocketAsynEA.Get(),
                    writeSocketAsyncEA = RWSocketAsynEA.GetNullBuffer(),
                    ConnectTime = DateTime.Now,
                    Remote = socketAsyncEArgs.AcceptSocket.RemoteEndPoint,
                    IPAddress = ((IPEndPoint)(socketAsyncEArgs.AcceptSocket.RemoteEndPoint)).Address,
                    sendQueue = new(),
                    cts = new(),
                    msgSendThreadLock = new(),
                    ServerService=this
                };
                //绑定线程
                //消息发送线程
                clientToken.msgSendThread = new Thread(
                    () => MsgSendListenThread(clientToken));
                clientToken.msgSendThread.IsBackground = true;
                clientToken.msgSendThread.Start();
                //心跳监控线程
                clientToken.PingPongThread = new Thread(
                    () => clientToken.PongThread());
                clientToken.PingPongThread.IsBackground = true;
                clientToken.PingPongThread.Start();

                //添加和绑定
                ClientDic.TryAdd(clientToken.socket, clientToken);
                clientToken.readSocketAsyncEA.UserToken = clientToken;
                clientToken.writeSocketAsyncEA.UserToken = clientToken;
                //广播消息
                TcpServerController.ClientConnectedCallBack(clientToken);
                RSJWYLogger.Log(RSJWYFameworkEnum.NetworkTcpServer,
                    $"一个客户端连接上来：{clientToken.Remote},当前设备数：{m_clientCount}");

                //接受消息传入请求
                if (!clientToken.socket.ReceiveAsync(clientToken.readSocketAsyncEA))
                    Task.Run(() => ProcessReceive(clientToken.readSocketAsyncEA)); // 异步执行接收处理，避免递归调用
            }
            catch (Exception e)
            {
                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer, $" ReceiveAsync接受连接时发生异常\n {e}");
            }
  
            StartAccept(socketAsyncEArgs);  
        }
        /// <summary>
        /// SocketAsyncEventArgs的操作回调
        /// </summary>
        void IO_Completed(object sender, SocketAsyncEventArgs e)  
        {  
            // SocketAsyncEventArgs回调处理
            switch (e.LastOperation)  
            {  
                case SocketAsyncOperation.Receive:  
                    Task.Run(() => ProcessReceive(e));  
                    break;  
                case SocketAsyncOperation.Send:  
                    Task.Run(() =>ProcessSend(e));  
                    break;  
                default:  
                    RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer,$"TCP IO_Completed 在套接字上完成的最后一个操作不是接收或发送，{e.LastOperation}");  
                    break;
            }  
        }  
        
        /// <summary>
        /// 处理数据传入
        /// </summary>
        private void ProcessReceive(SocketAsyncEventArgs socketAsyncEA)
        {
            // 检查连接状态
            var token = (ClientSocketToken)socketAsyncEA.UserToken;
            if (!ClientDic.ContainsKey(token.socket))
            {
                //可能后期会移除
                RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer,$"无法找到用于存储的客户端容器，无法执行接收,可能已经断开链接，当前数量为：{ClientDic.Count}");
                return;//找不到存储的容器
            }
            try
            {
                var readBuff = token.ReadBuff;//获取客户端容器的字节容器
                //必须有可读数据，否则将关闭连接
                if (socketAsyncEA.BytesTransferred > 0 && socketAsyncEA.SocketError == SocketError.Success)
                {
                    //首先判断客户端token缓冲区剩余空间是否支持数据拷贝
                    if (readBuff.Remain<=socketAsyncEA.BytesTransferred)
                        readBuff.ReSize(readBuff.WriteIndex+socketAsyncEA.BytesTransferred);//确保从写入索引开始，能写入数据
                    //拷贝到容器缓冲区
                    lock (token.ReadBuff)
                    {
                        //从缓冲区获取并设置数据
                        readBuff.SetBytes(socketAsyncEA.Buffer,socketAsyncEA.Offset,socketAsyncEA.BytesTransferred);
                        socketAsyncEA.SetBuffer(0, socketAsyncEA.Buffer.Length);
                        //处理本次接收的数据
                        while (readBuff.Readable>4)
                        {
                            //获取消息长度
                            int msgLength = BitConverter.ToInt32(readBuff.GetlengthBytes(4).ToArray());
                            //判断是不是分包数据
                            if (readBuff.Readable < msgLength + 4)
                            {
                                //如果消息长度小于读出来的消息长度
                                //此为分包，不包含完整数据
                                //因为产生了分包，可能容量不足，根据目标大小进行扩容到接收完整
                                //扩容后，retun，继续接收
                                readBuff.MoveBytes(); //已经完成一轮解析，移动数据
                                readBuff.ReSize(msgLength + 8); //扩容，扩容的同时，保证长度信息也能被存入
                                return;
                            }
                            //移动，规避长度位，从整体消息开始位接收数据
                            readBuff.ReadIndex += 4; //前四位存储字节流数组长度信息
                            //在消息接收异步线程内同步处理消息，保证当前客户消息顺序性
                            //var _msgBase= MessageTool.DecodeMsg(readBuff.Bytes, readBuff.ReadIndex, msgLength);
                            var decodeMsg= MessageTool.DecodeMsg(readBuff.GetlengthBytes(msgLength),m_MsgBodyEncrypt,m_SocketMsgEncode);
                            //创建消息容器，检查解析的是不是心跳包
                            if (decodeMsg.IsPingPong)
                            {
                                token.lastPingTime = Utility.Utility.GetTimeStamp();
                                RSJWYLogger.Log($"<color=blue>服务器返回心跳包</color>");
                                ServerToClientMsgContainer pingpongMsg = new()
                                {
                                    TargetToken = token,
                                    SendBytes = MessageTool.SendPingPong()
                                };
                                token.sendQueue.Enqueue(pingpongMsg);
                            }
                            else
                            {
                                var msgContainer = new ClientMsgContainer()
                                {
                                    TargetToken= token,
                                    msg= decodeMsg.msgBase,
                                }; 
                                serverMsgQueue.Enqueue(msgContainer);
                            }
                            //处理完后移动数据位
                            readBuff.ReadIndex += msgLength;
                            //检查是否需要扩容
                            readBuff.CheckAndMoveBytes();
                            //结束本次处理循环，如果粘包，下一个循环将会处理
                        }
                    }
                    //继续接收. 为什么要这么写,请看Socket.ReceiveAsync方法的说明  
                    if (!token.socket.ReceiveAsync(socketAsyncEA))
                        Task.Run(() => ProcessReceive(socketAsyncEA));
                }
                else
                {
                    RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer,$"读取客户端：{token.TokenID}-{token.Remote}发来的消息出错！！错误信息： {socketAsyncEA.SocketError}，将关闭本链接" );
                    CloseClientSocket(token);
                }
            }
            catch (Exception ex)
            {
                RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer,$"读取客户端：{token.TokenID}-{token.Remote}发来的消息出错！！错误信息： {ex.ToString()}，将关闭本链接" );
                CloseClientSocket(token);
            }
        }
        
        /// <summary>
        /// 发送信息到客户端
        /// </summary>
        /// <param name="msgBase">消息内容</param>
        /// <param name="targetToken">目标客户端标志</param>
        /// <returns>仅确保数据转换完毕，可以发送，但不确保数据发送成功</returns>
        internal void SendMessage(object msgBase,ClientSocketToken targetToken)
        {
            if (targetToken?.socket is not { Connected: true })
            {
                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$"Socket链接未设置或者未建立链接");
                return;//链接不存在或者未建立链接
            }
            //写入数据
            try
            {
                if (!targetToken.socket.Poll(100, SelectMode.SelectWrite))
                {
                    RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$"Socket状态不可写");
                    return;//链接不存在或者未建立链接
                }
                //编码
                var sendBytes = MessageTool.EncodeMsg(msgBase,m_MsgBodyEncrypt,m_SocketMsgEncode);
                //创建容器
                ServerToClientMsgContainer msg = new()
                {
                    TargetToken = targetToken,
                    SendBytes = sendBytes
                };
                targetToken.sendQueue.Enqueue(msg);
                //写入到队列，向客户端发送消息，根据客户端和绑定的数据发送
            }
            catch (SocketException ex)
            {
                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$"向客户端发送消息失败 SendMessage Error:{ex.ToString()}");
                //CloseSocket(ClientDic[_client]);?断开方式有待商讨
            }
        }
        
        
        /// <summary>
        /// 数据发送后回调
        /// </summary>
        /// <param name="socketAsyncEA"></param>
        private void ProcessSend(SocketAsyncEventArgs socketAsyncEA)
        {
            var clientToken = (ClientSocketToken)socketAsyncEA.UserToken;
            if (!ClientDic.ContainsKey(clientToken.socket))
            {
                //可能后期会移除
                RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer,$"无法找到用于存储的客户端容器，无法执行接收,可能已经断开链接，当前数量为：{ClientDic.Count}");
                return;//找不到存储的容器
            }
            try
            {
                if (socketAsyncEA.SocketError == SocketError.Success&&socketAsyncEA.BytesTransferred > 0)
                {
                    //获取本次数据操作长度，对比应该发送长度
                    int senlength = socketAsyncEA.BytesTransferred;
                    //取出消息类但不移除，用作数据比较，根据消息MSG队列，获取相应的客户端内的消息数组队列
                    clientToken.sendQueue.TryPeek(out var _msgbase);
                    var _ba = _msgbase.SendBytes;
                    _ba.ReadIndex += senlength;//已发送索引
                    if (_ba.Readable == 0)//代表发送完整
                    {
                        clientToken.sendQueue.TryDequeue(out var _);//取出但不使用，只为了从队列中移除
                        _ba = null;//发送完成
                    }
                    //发送不完整，再次发送
                    if (_ba != null)
                    {
                        //重新获取可用数据切片作为发送数据
                        socketAsyncEA.SetBuffer(_ba.GetRemainingSlices());
                        if (!_msgbase.TargetToken.socket.SendAsync(socketAsyncEA))
                            Task.Run(() => ProcessSend(socketAsyncEA));
                    }
                    else
                    {
                        //本条数据发送完成，激活线程，继续处理下一条
                        lock (clientToken.msgSendThreadLock)
                        {
                            //释放锁，继续执行信息发送
                            Monitor.Pulse(clientToken.msgSendThreadLock);
                        }

                    }
                }
                else
                {
                    RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpClient,$"向服务器发送消息失败 ProcessSend Error:不满足回调进入条件；{socketAsyncEA.SocketError }");
                    CloseClientSocket(clientToken);
                }

            }
            catch (Exception exception)
            { 
                CloseClientSocket(clientToken);
                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$"向客户端发送消息失败 SendCallBack Error:{exception}" );
            }
        }  
        
        
        
        /// <summary>
        /// 关闭客户端
        /// </summary>
        /// <param name="targetSocket"></param>
        public void CloseClientSocket(ClientSocketToken ClientToken)
        {
            ClientDic.TryRemove(ClientToken.socket, out var _);
                
            Interlocked.Decrement(ref m_clientCount);
            TcpServerController.CloseClientReCallBack(ClientToken);
            ClientToken.Close();
            // 释放SocketAsyncEventArgs,并放回池子中
            if (ClientToken.readSocketAsyncEA!=null)
                RWSocketAsynEA.Release(ClientToken.readSocketAsyncEA);
            if (ClientToken.writeSocketAsyncEA != null)
                RWSocketAsynEA.Release(ClientToken.writeSocketAsyncEA);
            RSJWYLogger.Log(RSJWYFameworkEnum.NetworkTcpServer,$"一个客户端断开连接，当前连接总数：{m_clientCount}");
        }
        /// <summary>
        /// 关闭服务器，关闭所有已经链接上来的socket以及关闭多余线程
        /// </summary>
        public void Quit()
        {
            if (!isInit)
            {
                return;
            }
            Status = NetServerStatus.CloseListening;
            cts.Cancel();
            //关闭所有已经链接上来的socket
            List<ClientSocketToken> _tmp = ClientDic
                .Select(x=>x.Value)
                .ToList();
            // lock ((clientList as ICollection).SyncRoot)
            // {
            //     for (int i = 0; i < clientList.Count; i++)
            //     {
            //         _tmp.Add(clientList[i]);
            //     }
            // }
            for (int i = 0; i < _tmp.Count; i++)
            {
                CloseClientSocket(_tmp[i]);
            }
            ListenSocket.Close();
            RSJWYLogger.Log(RSJWYFameworkEnum.NetworkTcpServer,$"已关闭所有链接上来的客户端");
            Status = NetServerStatus.Close;
        }


        #endregion
        
        #region 线程
        /// <summary>
        /// 消息队列发送监控线程
        /// 分配到每一个的客户端容器内
        /// </summary>
        void MsgSendListenThread(ClientSocketToken clientSocketToken)
        {
            while (!clientSocketToken.cts.Token.IsCancellationRequested)
            {
                try
                {
                    if (clientSocketToken.sendQueue.Count <= 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    //取出消息队列内的消息，但不移除队列，以获取目标客户端
                    clientSocketToken.sendQueue.TryPeek(out var _msgbase);
                    //_msgbase.targetToken.writeSocketAsyncEA.SetBuffer(_msgbase.SendOldBytes.Bytes, _msgbase.SendOldBytes.ReadIndex,_msgbase.SendOldBytes.length);
                    var data = _msgbase.SendBytes.GetRemainingSlices();
                    _msgbase.TargetToken.writeSocketAsyncEA.SetBuffer(data);
                    //当前线程执行休眠，等待消息发送完成后继续
                    lock (clientSocketToken.msgSendThreadLock)
                    {
                        if (!_msgbase.TargetToken.socket.SendAsync( _msgbase.TargetToken.writeSocketAsyncEA))
                            Task.Run(()=>ProcessSend(_msgbase.TargetToken.writeSocketAsyncEA));
                        //等待SendCallBack完成回调释放本锁再继续执行，超时10秒
                        Monitor.Wait(clientSocketToken.msgSendThreadLock);
                    }
                }
                catch (Exception ex)
                {
                    RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer, $"消息发送时发生错误：{ex.Message}");
                    if (clientSocketToken.cts.Token.IsCancellationRequested)
                    {
                        RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer, $"请求取消任务");
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 消息处理线程，分发消息
        /// </summary>
        void MsgThread(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (serverMsgQueue.Count <= 0)
                    {
                        Thread.Sleep(10);
                        continue;//当前无消息，跳过进行下一次排查处理
                    }
                    //有待处理的消息
                    ClientMsgContainer _msg = null;
                    //取出并移除取出来的数据
                    if (!serverMsgQueue.TryDequeue(out _msg))
                    {
                        RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$"取出并处理消息队列失败！！");
                        continue;
                    }
                    //处理取出来的数据
                    if (_msg != null)
                    {
                        //是其他协议，交给unity处理
                        if (TcpServerController == null)
                            throw new RSJWYException(RSJWYFameworkEnum.NetworkTool, $"没有绑定控制器，无法发送客户端来的消息");
                        TcpServerController.FromClientReceiveMsgCallBack(_msg.TargetToken,_msg.msg);
                    }
                }
                catch (Exception ex)
                {
                    RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer, $"消息分发时发生错误：{ex.Message}");
                    if (token.IsCancellationRequested)
                    {
                        RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer, $"请求取消任务");
                        break;
                    }
                }
               
            }
        }

        #endregion

    }
}