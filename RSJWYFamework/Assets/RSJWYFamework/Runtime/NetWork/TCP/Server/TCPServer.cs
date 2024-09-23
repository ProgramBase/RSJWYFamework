using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
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

        internal ISocketTCPServerController TcpServerController;
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
                    var _buffer = new byte[1048576]; 
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
                //心跳包监测处理线程-后台处理
                pingThread = new Thread(() =>PingThread(cts.Token));
                pingThread.IsBackground = true;
                pingThread.Start();
                isInit = true;
                StartAccept(null);
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
                //输出日志
                RSJWYLogger.Log(RSJWYFameworkEnum.NetworkTcpServer,$"服务端开启监听：{ListenSocket.LocalEndPoint.ToString()}");
            }
            catch (Exception e)
            {
                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$" AcceptAsync监听时发生异常\n {e}");
            }
        } 
        /// <summary>
        /// 接受连接请求
        /// </summary>
        private void ProcessAccept(SocketAsyncEventArgs socketAsyncEArgs)  
        {  
            try  
            {  
                Interlocked.Increment(ref m_clientCount);//计数 //获取一个空闲的  

                //创建并存储客户端信息
                var clientToken = new ClientSocketToken
                {
                    lastPingTime = Utility.Utility.GetTimeStamp(),
                    socket = socketAsyncEArgs.AcceptSocket,
                    ReadBuff = new(),
                    readSocketAsyncEA = RWSocketAsynEA.Get(),
                    writeSocketAsyncEA = RWSocketAsynEA.Get(),
                    ConnectTime = DateTime.Now,
                    Remote = socketAsyncEArgs.AcceptSocket.RemoteEndPoint,
                    IPAddress = ((IPEndPoint)(socketAsyncEArgs.AcceptSocket.RemoteEndPoint)).Address,
                    sendQueue = new(),
                    cts = new(),
                    msgSendThreadLock = new ()
                };
                //绑定线程
                //消息发送线程
                clientToken.msgSendThread = new Thread(
                    () =>MsgSendListenThread(cts.Token,clientToken.sendQueue,clientToken.msgSendThreadLock));
                clientToken.msgSendThread.IsBackground = true;
                clientToken.msgSendThread.Start();
                
                //添加和绑定
                ClientDic.TryAdd(clientToken.socket, clientToken);
                clientToken.readSocketAsyncEA.UserToken = clientToken;
                clientToken.writeSocketAsyncEA.UserToken = clientToken;
                
                RSJWYLogger.Log(RSJWYFameworkEnum.NetworkTcpServer,$"一个客户端连接上来：{clientToken.Remote}");
                
                //接受消息传入请求
                if (!socketAsyncEArgs.AcceptSocket.ReceiveAsync( clientToken.readSocketAsyncEA)) 
                    Task.Run(() => ProcessReceive( clientToken.readSocketAsyncEA)); // 异步执行接收处理，避免递归调用
            }  
            catch (Exception e)  
            {  
                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$" ReceiveAsync接受连接时发生异常\n {e}");
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
                    RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer,"在套接字上完成的最后一个操作不是接收或发送，{e.LastOperation}");  
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
                    while (readBuff.length>4)
                    {
                        //获取消息长度
                        int msgLength = BitConverter.ToInt32(readBuff.Bytes, readBuff.ReadIndex);
                        //判断是不是分包数据
                        if (readBuff.length < msgLength + 4)
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
                        var _msgBase= MessageTool.DecodeMsg(readBuff.Bytes, readBuff.ReadIndex, msgLength,out var _count);
                        //创建消息容器
                        var _msgContainer = new ClientMsgContainer()
                        {
                            targetToken= token,
                            msg= _msgBase,
                        };
                        serverMsgQueue.Enqueue(_msgContainer);
                        //处理完后移动数据位
                        readBuff.ReadIndex += msgLength;
                        readBuff.MoveBytes();//整理数据
                        //结束本次处理循环，如果粘包，下一个循环将会处理
                    }
                    readBuff.CheckAndMoveBytes();
                    //继续接收. 为什么要这么写,请看Socket.ReceiveAsync方法的说明  
                    if (!token.socket.ReceiveAsync(socketAsyncEA))
                        Task.Run(() => ProcessReceive(socketAsyncEA));
                }
                else
                {
                    CloseClientSocket(token.socket);
                }
            }
            catch (Exception ex)
            {
                RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer,$"读取客户端：{token.TokenID}-{token.Remote}发来的消息出错！！错误信息： \n {ex.ToString()}" );
                CloseClientSocket(token.socket);
            }
        }
        
        /// <summary>
        /// 发送信息到客户端
        /// </summary>
        /// <param name="msgBase">消息内容</param>
        /// <param name="targetToken">目标客户端标志</param>
        /// <returns>仅确保数据转换完毕，可以发送，但不确保数据发送成功</returns>
        internal void SendMessage(MsgBase msgBase,ClientSocketToken targetToken)
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
                ByteArray _sendBytes = MessageTool.EncodMsg(msgBase);
                //创建容器
                ServerToClientMsgContainer _msg = new()
                {
                    targetToken = targetToken,
                    msg = msgBase,
                    sendBytes = _sendBytes
                };
                targetToken.sendQueue.Enqueue(_msg);

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
        /// <param name="e"></param>
        private void ProcessSend(SocketAsyncEventArgs socketAsyncEA)
        {
            var ClientToken = (ClientSocketToken)socketAsyncEA.UserToken;
            try
            {
                if (socketAsyncEA.SocketError == SocketError.Success&&socketAsyncEA.BytesTransferred > 0)
                {
                    //获取本次数据操作长度，对比应该发送长度
                    int senlength = socketAsyncEA.BytesTransferred;
                    //取出消息类但不移除，用作数据比较，根据消息MSG队列，获取相应的客户端内的消息数组队列
                    ClientToken.sendQueue.TryPeek(out var _msgbase);
                    var _ba = _msgbase.sendBytes;
                    _ba.ReadIndex += senlength;//已发送索引
                    if (_ba.length == 0)//代表发送完整
                    {
                        ClientToken.sendQueue.TryDequeue(out var _);//取出但不使用，只为了从队列中移除
                        _ba = null;//发送完成
                    }
                    //发送不完整，再次发送
                    if (_ba != null)
                    {
                        socketAsyncEA.SetBuffer(_ba.ReadIndex,_ba.length);
                        if (!_msgbase.targetToken.socket.SendAsync(socketAsyncEA))
                            Task.Run(() => ProcessSend(socketAsyncEA));
                    }
                    else
                    {
                        //本条数据发送完成，激活线程，继续处理下一条
                        lock (ClientToken.msgSendThreadLock)
                        {
                            //释放锁，继续执行信息发送
                            Monitor.Pulse(ClientToken.msgSendThreadLock);
                        }

                    }
                }
                else
                {
                    CloseClientSocket(ClientToken.socket);
                }

            }
            catch (Exception exception)
            { 
                CloseClientSocket(ClientToken.socket);
                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$"向客户端发送消息失败 SendCallBack Error:{exception}" );
            }
        }  
        
        
        
        /// <summary>
        /// 关闭客户端
        /// </summary>
        /// <param name="targetSocket"></param>
        private void CloseClientSocket(System.Net.Sockets.Socket targetSocket)
        {  
            if (!ClientDic.TryRemove(targetSocket, out var clientToken))
            {
                return;
            }
            Interlocked.Decrement(ref m_clientCount);
            TcpServerController.ClientReConnectedCallBack(clientToken);
            clientToken.Close();
            // 释放SocketAsyncEventArgs,并放回池子中
            if (clientToken.readSocketAsyncEA!=null)
                RWSocketAsynEA.Release(clientToken.readSocketAsyncEA);
            if (clientToken.writeSocketAsyncEA != null)
                RWSocketAsynEA.Release(clientToken.writeSocketAsyncEA);
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
            //关闭所有已经链接上来的socket
            List<System.Net.Sockets.Socket> _tmp = ClientDic
                .Select(x=>x.Value.socket)
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

        }


        #endregion
        
         #region 线程
        /// <summary>
        /// 消息队列发送监控线程
        /// </summary>
        void MsgSendListenThread(CancellationToken token,ConcurrentQueue<ServerToClientMsgContainer> sendQueue
        ,object ThreadLock)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (sendQueue.Count <= 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    ServerToClientMsgContainer _msgbase;
                    //取出消息队列内的消息，但不移除队列，以获取目标客户端
                    sendQueue.TryPeek(out _msgbase);
                    _msgbase.targetToken.writeSocketAsyncEA.SetBuffer(_msgbase.sendBytes.Bytes, _msgbase.sendBytes.ReadIndex,_msgbase.sendBytes.length);
                    //当前线程执行休眠，等待消息发送完成后继续
                    lock (ThreadLock)
                    {
                        if (!_msgbase.targetToken.socket.SendAsync( _msgbase.targetToken.writeSocketAsyncEA))
                            Task.Run(()=>ProcessSend(_msgbase.targetToken.writeSocketAsyncEA));
                        //等待SendCallBack完成回调释放本锁再继续执行，超时10秒
                        bool istimeout = Monitor.Wait(ThreadLock, 20000);
                        if (!istimeout)
                        {
                            RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer,$"消息发送时间超时（超过10s），请检查网络质量，关闭本客户端的链接");
                            CloseClientSocket(_msgbase.targetToken.socket);
                        }
                    }
                }
                catch (Exception ex)
                {
                    RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer, $"消息发送时发生错误：{ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer, $"请求取消任务");
                        break;
                    }
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
                        CloseClientSocket(clientSocket.socket);
                    }
                    _tmpClose.Clear();//操作完成后清除客户端
                }
                catch (Exception ex)
                {
                    RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer, $"检测心跳包时发生错误：{ex.Message}");
                    if (!token.IsCancellationRequested)
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
                        //如果接收到是心跳包
                        if (_msg.msg is MsgPing)
                        {
                            MsgPing _clientMsgPing = _msg.msg as MsgPing;
                            //更新接收到的心跳包时间（后台运行）
                            _msg.targetToken.lastPingTime =  Utility.Utility.GetTimeStamp();
                            //创建消息并返回
                            MsgPing msgPong = SendMsgMethod.SendMsgPing(_msg.targetToken.lastPingTime);
                            RSJWYLogger.Log($"<color=blue>服务器返回心跳包</color>");
                            SendMessage(msgPong, _msg.targetToken);//返回客户端
                        }
                        else
                        {
                            //是其他协议，交给unity处理
                            if (TcpServerController == null)
                                throw new RSJWYException(RSJWYFameworkEnum.NetworkTool, $"没有绑定控制器，无法发送客户端来的消息");
                            TcpServerController.FromClientReceiveMsgCallBack(_msg.targetToken,_msg.msg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer, $"消息分发时发生错误：{ex.Message}");
                    if (!token.IsCancellationRequested)
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