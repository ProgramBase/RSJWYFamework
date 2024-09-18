using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Net.Public;
using RSJWYFamework.Runtime.NetWork.Base;
using UnityEngine;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.NetWork.Public;
using RSJWYFamework.Runtime.Utility;

namespace RSJWYFamework.Runtime.NetWork.TCP.Server
{
    /// <summary>
    /// 网络链接事件枚举
    /// </summary>
    public enum NetServerStatus
    {
        None = 0,
        ServerServicecOpen ,
        ServerServicecClose,

        /// <summary>
        /// 网络重连
        /// </summary>
        ReConnect
    }

    /// <summary>
    /// 服务器模块核心，仅允许通过ServerController调用
    /// </summary>
    internal class TcpServerService
    {
        #region 字段
        
        internal ISocketTCPServerController SocketTcpClientController;
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
        internal static ConcurrentDictionary<System.Net.Sockets.Socket, ClientSocket> ClientDic = new ConcurrentDictionary<System.Net.Sockets.Socket, ClientSocket>();

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
        static bool isThreadOver = false;

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        bool isInit = false;

        #endregion
        #region 初始化和客户端接入

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
                //开启新的异步监听
                ListenSocket.BeginAccept(AcceptCallBack, null);
                isThreadOver = false;

                //消息处理线程-后台处理
                msgThread = new Thread(MsgThread);
                msgThread.IsBackground = true;//后台运行
                msgThread.Start();
                //心跳包监测处理线程-后台处理
                pingThread = new Thread(PingThread);
                pingThread.IsBackground = true;
                pingThread.Start();
                //消息发送线程
                msgSendThread = new Thread(MsgSendListenThread);
                msgSendThread.IsBackground = true;
                msgSendThread.Start();

                isInit = true;
                //输出日志
                RSJWYLogger.Log(RSJWYFameworkEnum.NetworkTcpServer,$"服务端启动服务：{ListenSocket.LocalEndPoint.ToString()} 成功！！,当前客户端数量：{ClientDic.Count.ToString()}");
            }
            catch (Exception e)
            {
                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$" 服务端启动监听 IP:{ip}，Port{port}失败！！错误信息：\n {e}");
            }

        }
        /// <summary>
        /// 有客户端链接上来执行本回调
        /// </summary>
        void AcceptCallBack(IAsyncResult ar)
        {
            if (isThreadOver)
            {
                return;
            }
            try
            {
                //结束用于连接的异步请求，获取客户端socket
                System.Net.Sockets.Socket client = ListenSocket.EndAccept(ar);
                //获取完成继续开启异步监听，监听下一次的链接
                ListenSocket.BeginAccept(AcceptCallBack, null);
                //创建新客户端容器存储客户端信息
                ClientSocket _ServerClient = new()
                {
                    //存储
                    socket = client,
                    //存储连接时间
                    lastPingTime =  Utility.Utility.GetTimeStamp(),
                    ReadBuff = new()
                };
                //存储到字典
                //ClientDic.Add(client, _ServerClient);
                ClientDic.TryAdd(client, _ServerClient);
                // //记录链接上来的客户端
                // lock ((clientList as ICollection).SyncRoot)
                // {
                //     clientList.Add(client);
                // }
                //广播客户端已上线
                SocketTcpClientController.ClientConnectedCallBack(_ServerClient);
                //开启异步接收消息
                client.BeginReceive(
                    _ServerClient.ReadBuff.Bytes, _ServerClient.ReadBuff.WriteIndex, _ServerClient.ReadBuff.Remain, 0, ReceiveCallBack, client);
                
                RSJWYLogger.Log(RSJWYFameworkEnum.NetworkTcpServer,$"和客户端{client.RemoteEndPoint.ToString()}建立链接成功！！当前客户端数量:{ClientDic.Count}" );
            }
            catch (SocketException ex)
            {
                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$"客户端建立连接失败！！错误信息：\n {ex.ToString()}" );
            }
        }

        #endregion

        #region 消息处理

        /// <summary>
        /// 接收完成回调
        /// </summary>
        /// <param name="ar"></param>
        void ReceiveCallBack(IAsyncResult ar)
        {
            //获取客户端
            System.Net.Sockets.Socket client = (System.Net.Sockets.Socket)ar.AsyncState;
            if (!ClientDic.ContainsKey(client))
            {
                RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer,$"无法找到用于存储的客户端容器，无法执行接收,可能已经断开链接，当前数量为：{ClientDic.Count}");
                return;//找不到存储的容器
            }
            //找对应的客户端容器
            ClientSocket _serverClient = ClientDic[client];
            try
            {
                ByteArray readBuff = _serverClient.ReadBuff;//获取客户端容器的字节容器
                int count = client.EndReceive(ar,out var SocketErr);//获取接收到的字节长度
                
                // 检查SocketError是否表示连接已断开
                if (SocketErr is SocketError.ConnectionReset or SocketError.ConnectionAborted)
                {
                    RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer,$"读取客户端：{client.RemoteEndPoint.ToString()}发来的消息出错！！SocketError：{SocketErr}");
                    CloseClient(_serverClient);
                    return;
                }
                if (count <= 0)
                {
                    //没有接收到数据
                    RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer,$"读取客户端：{client.RemoteEndPoint.ToString()}发来的消息出错！！数据长度小于0，客户端连可能已经断开，服务器关闭对此客户端的链接。");
                    //服务器关闭链接
                    CloseClient(_serverClient);
                    return;
                }
                //不需要断开连接
                readBuff.WriteIndex += count;//移动已经写入索引
                OnReceiveData(_serverClient);//把数据交给数据处理函数
                ////检查是否需要扩容
                readBuff.CheckAndMoveBytes();

                //本轮消息处理完成后或者这次处理的数据有分包情况，开启下一次的异步监听，如果分包则继续接收未接收完成的数据
                //等待下一次数据处理接收
                client.BeginReceive(readBuff.Bytes, readBuff.WriteIndex, readBuff.Remain, 0, ReceiveCallBack, client);

            }
            catch (SocketException ex)
            {
                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$"读取客户端：{client.RemoteEndPoint.ToString()}发来的消息出错！！错误信息： \n {ex.ToString()}" );
                CloseClient(_serverClient);
            }
        }

        /// <summary>
        /// 接收数据处理，处理分包粘包情况
        /// </summary>
        void OnReceiveData(ClientSocket serverClientSocket)
        {
            //取字节流数组
            ByteArray _byteArray = serverClientSocket.ReadBuff;

            MessageTool.DecodeMsg(_byteArray, serverMsgQueue, serverClientSocket.socket,
                () =>
                {
                    CloseClient(serverClientSocket);
                });
        }
        /// <summary>
        /// 发送信息到客户端
        /// </summary>
        internal bool SendMessage(MsgBase msgBase)
        {
            var _client=msgBase.targetSocket;//取出目标客户端
            if (_client == null || !_client.Connected)
            {
                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$"Socket链接未设置或者未建立链接");
                return false;//链接不存在或者未建立链接
            }
            //写入数据
            try
            {
                //转数组
                ByteArray _sendBytes = MessageTool.EncodMsg(msgBase);
                //创建容器
                ServerToClientMsg _msg = new()
                {
                    msgTargetSocket = msgBase.targetSocket,
                    msg = msgBase,
                    sendBytes = _sendBytes
                };
                //写入到队列，向客户端发送消息，根据客户端和绑定的数据发送
                msgSendQueue.Enqueue(_msg);
                return true;//链接不存在或者未建立链接
            }
            catch (SocketException ex)
            {
                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$"向客户端发送消息失败 SendMessage Error:{ex.ToString()}");
                CloseClient(ClientDic[_client]);
                return false;//链接不存在或者未建立链接
            }
        }
        /// <summary>
        /// 发送消息到客户端结束后回调
        /// </summary>
        void SendCallBack(IAsyncResult ar)
        {
            System.Net.Sockets.Socket client = (System.Net.Sockets.Socket)ar.AsyncState;//获取目标客户端的socket
            ServerToClientMsg _msgbase;//存储从队列里取出来的数据
            ByteArray _ba;//存储字节流数组
            try
            {
                if (client == null || !client.Connected)
                {
                    return;
                }
                //获取已经发送的字节流长度
                int count = client.EndSend(ar);
                //判断是否发送完成(消息是否发送完整)
                //取出消息类但不移除，用作数据比较，根据消息MSG队列，获取相应的客户端内的消息数组队列
                msgSendQueue.TryPeek(out _msgbase);
                _ba = _msgbase.sendBytes;
                _ba.ReadIndex += count;//已发送索引
                if (_ba.length == 0)//代表发送完整
                {
                    ServerToClientMsg _msDelete;
                    msgSendQueue.TryDequeue(out _msDelete);//取出但不使用，只为了从队列中移除
                    _ba = null;//发送完成

                }
                //发送不完整，再次发送
                if (_ba != null)
                {
                    client.BeginSend(_ba.Bytes, _ba.ReadIndex, _ba.length, 0, SendCallBack, client);
                }
                else
                {
                    //本条数据发送完成，激活线程，继续处理下一条
                    lock (msgSendThreadLock)
                    {
                        //释放锁，继续执行信息发送
                        Monitor.Pulse(msgSendThreadLock);
                    }

                }
            }
            catch (SocketException e)
            {
                RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$"向客户端发送消息失败 SendCallBack Error:{e}" );
                CloseClient(ClientDic[client]);
            }
        }

        #endregion

        #region 线程
        /// <summary>
        /// 消息队列发送监控线程
        /// </summary>
        void MsgSendListenThread()
        {
            while (!isThreadOver)
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
        void PingThread()
        {
            List<ClientSocket> _tmpClose = new List<ClientSocket>();//存储需要断开来的客户端
            while (!isThreadOver)
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
                    foreach (ClientSocket serverClientSocket in ClientDic.Values)
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
                    foreach (ClientSocket clientSocket in _tmpClose)
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
        void MsgThread()
        {
            while (!isThreadOver)
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
                            ClientSocket _socket = ClientDic[_msg.targetSocket];
                            //更新接收到的心跳包时间（后台运行）
                            _socket.lastPingTime =  Utility.Utility.GetTimeStamp();
                            //创建消息并返回
                            MsgPing msgPong = SendMsgMethod.SendMsgPing(_socket.lastPingTime);
                            msgPong.targetSocket = _socket.socket;
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

        #region Unity主线程调用

        /// <summary>
        /// 需要Unity处理的消息
        /// </summary>
        internal void TCPUpdate()
        {
            if (!isInit)
            {
                return;
            }
            //处理unity该处理的消息
            if (ListenSocket != null)
            {
                if (UnityMsgQueue.Count <= 0)
                {
                    return;//没有消息
                }
                //取出消息
                if (UnityMsgQueue.Count > 0)
                {
                    MsgBase msgBase = null;
                    //取出并移除数据
                    if (UnityMsgQueue.TryDequeue(out msgBase))
                    {
                        SocketTcpClientController.FromClientReceiveMsgCallBack( ClientDic[msgBase.targetSocket], msgBase);//交给执行回调
                    }
                    else
                    {
                        RSJWYLogger.Error(RSJWYFameworkEnum.NetworkTcpServer,$"非正常错误！服务器在处理返回给unity处理的信息时，取出并处理消息队列失败！！");
                    }
                }
            }
        }
        #endregion

        #region 关闭链接


        /// <summary>
        /// 关闭客户端
        /// </summary>
        public void CloseClient(ClientSocket client)
        {
            SocketTcpClientController.ClientReConnectedCallBack(client);
            //清除客户端
            client.socket.Close();//关闭链接
            //移除已连接客户端
            ClientSocket _a = new();//创建用于存储返回的值，不使用
            ClientDic.TryRemove(client.socket, out _a);
            // lock ((clientList as ICollection).SyncRoot)
            // {
            //     clientList.Remove(client.socket);
            // }
            RSJWYLogger.Log(RSJWYFameworkEnum.NetworkTcpServer,$"一个客户端断开连接，当前连接总数：{ClientDic.Count}");
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
                CloseClient(ClientDic[_tmp[i]]);
            }
            isThreadOver = true;
            ListenSocket.Close();
            RSJWYLogger.Log(RSJWYFameworkEnum.NetworkTcpServer,$"已关闭所有链接上来的客户端");

        }
        #endregion
    }

}
