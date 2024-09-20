using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Net.Public;
using RSJWYFamework.Runtime.NetWork.Base;
using RSJWYFamework.Runtime.NetWork.Public;
using RSJWYFamework.Runtime.Pool;

namespace RSJWYFamework.Runtime.NetWork.TCP.Server
{
    public class TCPServer
    {
        
        #region 字段

        internal ISocketTCPServerController SocketTcpServerController;
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
        internal static long pingInterval = 3;

        /// <summary>
        /// 服务器监听Socket
        /// </summary>
        static System.Net.Sockets.Socket ListenSocket;


        /// <summary>
        /// 客户端容器字典
        /// </summary>
        internal static ConcurrentDictionary<System.Net.Sockets.Socket, ClientSocketToken> ClientDic = new ConcurrentDictionary<System.Net.Sockets.Socket, ClientSocketToken>();

        /// <summary>
        /// 服务器接收的消息队列
        /// </summary>
        static ConcurrentQueue<MsgBase> serverMsgQueue = new();

        /// <summary>
        /// 需要交给unity处理的消息队列
        /// </summary>
        static ConcurrentQueue<MsgBase> UnityMsgQueue = new();

        /// <summary>
        /// 消息发送队列
        /// </summary>
        static ConcurrentQueue<ServerToClientMsg> msgSendQueue = new();

        /// <summary>
        /// 消息处理线程
        /// </summary>
        static Thread msgThread;
        /// <summary>
        /// 心跳包监测线程
        /// </summary>
        static Thread pingThread;
        /// <summary>
        /// 消息队列发送监控线程
        /// </summary>
        static Thread msgSendThread;
        /// <summary>
        ///  消息队列发送锁
        /// </summary>
        object msgSendThreadLock = new object();

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
        private ObjectPool<SocketAsyncEventArgs> RWSocketAsynvEA;
        

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
            //初始化池
            RWSocketAsynvEA = new(
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
                    var _buffer = new byte[1048576]; 
                    _obj.SetBuffer(_buffer,0,_buffer.Length);
                },
                (_obj) =>
                {
                    var _buffer = new byte[1048576]; 
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
                //心跳包监测处理线程-后台处理
                pingThread = new Thread(() =>PingThread(cts.Token));
                pingThread.IsBackground = true;
                pingThread.Start();
                //消息发送线程
                msgSendThread = new Thread(() =>MsgSendListenThread(cts.Token));
                msgSendThread.IsBackground = true;
                msgSendThread.Start();
                isInit = true;
                StartAccept(null);
                //输出日志
                RSJWYLogger.Log(RSJWYFameworkEnum.NetworkTcpServer,$"服务端开启监听：{ListenSocket.LocalEndPoint.ToString()}");
            }
            catch (Exception e)
            {
                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$" 服务端启动监听 IP:{ip}，Port{port}失败！！错误信息：\n {e}");
            }

        }

        #endregion

        #region 功能处理

        /// <summary>
        /// 开启监听，连接请求
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
                    ProcessAccept(e);
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
                {
                    ProcessAccept(acceptEventArg);
                }
            }
            catch (Exception e)
            {
                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$" 绑定AcceptAsync连接监听时发生异常\n {e}");
            }
        } 
        /// <summary>
        /// 接受连接请求
        /// </summary>
        private void ProcessAccept(SocketAsyncEventArgs socketAsyncEArgs)  
        {  
            try  
            {  
                Interlocked.Increment(ref m_clientCount);//计数
                SocketAsyncEventArgs readEventArgs = RWSocketAsynvEA.Get();  //获取一个空闲的  

                //创建并存储客户端信息
                var clientToken = new ClientSocketToken
                {
                    socket = socketAsyncEArgs.AcceptSocket,
                    ConnectTime = DateTime.Now,
                    Remote = socketAsyncEArgs.AcceptSocket.RemoteEndPoint,
                    IPAddress = ((IPEndPoint)(socketAsyncEArgs.AcceptSocket.RemoteEndPoint)).Address
                };
                //添加和绑定
                ClientDic.TryAdd(clientToken.socket, clientToken);
                readEventArgs.UserToken = clientToken;
                //接受消息传入请求
                if (!socketAsyncEArgs.AcceptSocket.ReceiveAsync(readEventArgs)) 
                    ProcessReceive(readEventArgs);  
            }  
            catch (Exception e)  
            {  
                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$" 绑定ReceiveAsync监听时发生异常\n {e}");
            }  
  
            // 接受下一个连接
            if (socketAsyncEArgs.SocketError == SocketError.OperationAborted)
            {
                RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer,$"socket操作中止，不再接收新的连接请求");
                return;
            }
            StartAccept(socketAsyncEArgs);  
        }  
        /// <summary>
        /// 关闭客户端
        /// </summary>
        /// <param name="SocketAsyncEA"></param>
        private void CloseClientSocket(SocketAsyncEventArgs SocketAsyncEA)  
        {  
            var token = SocketAsyncEA.UserToken as ClientSocketToken; 
            SocketTcpServerController.ClientReConnectedCallBack(token);
            ClientDic.TryRemove(token.socket, out var _a);
            
            Interlocked.Decrement(ref m_clientCount);  
            // 关闭与客户端关联的套接字 
            try  
            {  
                token.socket.Shutdown(SocketShutdown.Send);  
            }
            catch (Exception e)
            {
                RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer,$"向客户端执行socket.shutdown出现异常！{e}");
            }
            //关闭链接
            token.socket.Close();
            // 释放SocketAsyncEventArgs,并放回池子中
            SocketAsyncEA.UserToken = null;  
            RWSocketAsynvEA.Release(SocketAsyncEA);
            RSJWYLogger.Log(RSJWYFameworkEnum.NetworkTcpServer,$"一个客户端断开连接，当前连接总数：{m_clientCount}");
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
                    ProcessReceive(e);  
                    break;  
                case SocketAsyncOperation.Send:  
                    ProcessSend(e);  
                    break;  
                default:  
                    throw new RSJWYException(RSJWYFameworkEnum.NetworkTcpServer,"在套接字上完成的最后一个操作不是接收或发送，{e.LastOperation}");  
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
                        readBuff.ReSize(readBuff.length+socketAsyncEA.BytesTransferred);
                    //拷贝到容器缓冲区
                    lock (token.ReadBuff)
                    {
                        //拷贝数据
                        Array.Copy(socketAsyncEA.Buffer, socketAsyncEA.Offset,
                            readBuff.Bytes, readBuff.WriteIndex,
                            socketAsyncEA.BytesTransferred);
                        readBuff.WriteIndex += socketAsyncEA.BytesTransferred;//移动已经写入索引
                    }
                    //提交给反序列化解析数据
                    MessageTool.DecodeMsg(token.ReadBuff, serverMsgQueue, token.socket,
                        () =>
                        {
                            CloseClientSocket(socketAsyncEA);
                        });
                    readBuff.CheckAndMoveBytes();
                    //继续接收. 为什么要这么写,请看Socket.ReceiveAsync方法的说明  
                    if (!token.socket.ReceiveAsync(socketAsyncEA))
                        this.ProcessReceive(socketAsyncEA);
                }
                else
                {
                    CloseClientSocket(e);
                }
            }
            catch (Exception xe)
            {
                RuncomLib.Log.LogUtils.Info(xe.Message + "\r\n" + xe.StackTrace);
            }
        }
// This method is invoked when an asynchronous send operation completes.    
        // The method issues another receive on the socket to read any additional   
        // data sent from the client  
        //  
        // <param name="e"></param>  
        private void ProcessSend(SocketAsyncEventArgs e)  
        {  
            if (e.SocketError == SocketError.Success)  
            {  
                // done echoing data back to the client  
                AsyncUserToken token = (AsyncUserToken)e.UserToken;  
                // read the next block of data send from the client  
                bool willRaiseEvent = token.Socket.ReceiveAsync(e);  
                if (!willRaiseEvent)  
                {  
                    ProcessReceive(e);  
                }  
            }  
            else  
            {  
                CloseClientSocket(e);  
            }  
        }  
        #endregion
        
         #region 线程
        /// <summary>
        /// 消息队列发送监控线程
        /// </summary>
        void MsgSendListenThread(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (msgSendQueue.Count <= 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }
                    ServerToClientMsg _msgbase;
                    System.Net.Sockets.Socket _client;
                    ByteArray _sendByte;
                    //取出消息队列内的消息，但不移除队列，以获取目标客户端
                    msgSendQueue.TryPeek(out _msgbase);
                    //设置目标客户端
                    _client = _msgbase.msgTargetSocket;
                    //获取发送消息数组
                    _sendByte = _msgbase.sendBytes;
                    //发送消息
                    _client.BeginSend(_sendByte.Bytes, 0, _sendByte.length, 0, SendCallBack, _client);
                    //当前线程执行休眠，等待消息发送完成后继续
                    lock (msgSendThreadLock)
                    {
                        //等待SendCallBack完成回调释放本锁再继续执行，超时10秒
                        bool istimeout = Monitor.Wait(msgSendThreadLock, 20000);
                        if (!istimeout)
                        {
                            RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer,$"消息发送时间超时（超过10s），请检查网络质量，关闭本客户端的链接");
                            CloseClient(ClientDic[_client]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer, $"消息发送时发生错误：{ex.Message}");
                }
            }
        }

        /// <summary>
        /// 心跳包监测线程
        /// </summary>
        void PingThread(CancellationToken token)
        {
            List<ClientSocketToken> _tmpClose = new List<ClientSocketToken>();//存储需要断开来的客户端
            while (!token.IsCancellationRequested)
            {
                try
                {
                    Thread.Sleep(1000);//本线程可以每秒检测一次
                    if (ClientDic.Count <= 0)
                    {
                        continue;
                    }
                    //检测心跳包是否超时的计算
                    //获取当前时间
                    long timeNow =  Utility.Utility.GetTimeStamp();
                    //遍历取出所有客户端
                    foreach (ClientSocketToken serverClientSocket in ClientDic.Values)
                    {
                        //超时，断开
                        if (timeNow - serverClientSocket.lastPingTime > pingInterval * 4)
                        {
                            //记录要断开链接的客户端
                            //防止在遍历过程中对dic操作（操作过程中，dic会被锁死）
                            _tmpClose.Add(serverClientSocket);
                        }
                    }
                    //逐一取出执行断开操作
                    foreach (ClientSocketToken clientSocket in _tmpClose)
                    {
                        RSJWYLogger.Log(RSJWYFameworkEnum.NetworkTcpServer,$"{clientSocket.socket.RemoteEndPoint.ToString()}在允许时间{pingInterval * 4}秒内发送心跳包超时，连接关闭！！");
                        CloseClient(clientSocket);
                    }
                    _tmpClose.Clear();//操作完成后清除客户端
                }
                catch (Exception ex)
                {
                    RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer, $"检测心跳包时发生错误：{ex.Message}");
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
                    MsgBase _msg = null;
                    //取出并移除取出来的数据
                    if (!serverMsgQueue.TryDequeue(out _msg))
                    {
                        RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$"取出并处理消息队列失败！！");
                        continue;
                    }
                    //处理取出来的数据
                    if (_msg != null)
                    {
                        //如果接收到是心跳包
                        if (_msg is MsgPing)
                        {
                            MsgPing _clientMsgPing = _msg as MsgPing;
                            ClientSocketToken socketToken = ClientDic[_msg.targetSocket];
                            //更新接收到的心跳包时间（后台运行）
                            socketToken.lastPingTime =  Utility.Utility.GetTimeStamp();
                            //创建消息并返回
                            MsgPing msgPong = SendMsgMethod.SendMsgPing(socketToken.lastPingTime);
                            msgPong.targetSocket = socketToken.socket;
                            SendMessage(msgPong);//返回客户端
                        }
                        else
                        {
                            //是其他协议，交给unity处理
                            UnityMsgQueue.Enqueue(_msg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer, $"消息分发时发生错误：{ex.Message}");
                }
               
            }
        }

        #endregion

    }
}