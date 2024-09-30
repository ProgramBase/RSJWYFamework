using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Net.Public;
using RSJWYFamework.Runtime.NetWork.Public;
using RSJWYFamework.Runtime.ReferencePool;

namespace RSJWYFamework.Runtime.NetWork.TCP.Server
{
    /// <summary>
    /// 服务器模块 单独客户端容器，存储客户端的相关信息等
    /// </summary>
    public class ClientSocketToken
    {
        /// <summary>
        /// 心跳包维持记录
        /// </summary>
        public long lastPingTime { get; internal set; }
        
        /// <summary>  
        /// 客户端IP地址  
        /// </summary>  
        public IPAddress IPAddress { get; internal set; }  
  
        /// <summary>  
        /// 远程地址  
        /// </summary>  
        public EndPoint Remote { get; internal set; }  
        
        /// <summary>  
        /// 连接时间  
        /// </summary>  
        public DateTime ConnectTime { get; internal set; }  
        /// <summary>
        /// 存储连接的客户端
        /// </summary>
        internal System.Net.Sockets.Socket socket { get;set; }
        /// <summary>
        /// 存储数据-接收到的数据暂存容器
        /// </summary>
        internal ByteArrayMemory ReadBuff;

        /// <summary>
        /// 客户端汇报的ID
        /// 作为客户端的唯一标识符，以免出现同一个客户端多链接，
        /// 需要做一个定时检查，出现重复拒绝链接以及移除已有链接，重新发起连接
        /// </summary>
        /// <returns></returns>
        public string TokenID{ get; internal set; }

        /// <summary>
        /// 写
        /// </summary>
        internal SocketAsyncEventArgs readSocketAsyncEA;
        /// <summary>
        /// 读
        /// </summary>
        internal SocketAsyncEventArgs writeSocketAsyncEA;
        /// <summary>
        /// 目标消息
        /// </summary>
        internal ConcurrentQueue<ServerToClientMsgContainer> sendQueue;

        /// <summary>
        /// 通知多线程自己跳出
        /// </summary>
        internal CancellationTokenSource cts;

        /// <summary>
        /// 消息发送线程
        /// </summary>
        internal Thread msgSendThread;
        
        
        /// <summary>
        ///  消息队列发送锁
        /// </summary>
        internal object msgSendThreadLock ;
        
        /// <summary>
        /// 关闭
        /// </summary>
        internal void Close()
        {
            try
            {
                lock (msgSendThreadLock)
                {
                    //释放锁，继续执行信息发送
                    Monitor.Pulse(msgSendThreadLock);
                }
                cts?.Cancel();
                socket?.Shutdown(SocketShutdown.Both);
                socket?.Close();
                //本条数据发送完成，激活线程，继续处理下一条
            }
            catch (Exception e)
            {
                RSJWYLogger.Warning(RSJWYFameworkEnum.NetworkTcpServer, $"客户端关闭时发生错误！{e}");
            }
        }
    }
    /// <summary>
    ///消息发送数据容器
    /// </summary>
    internal class ServerToClientMsgContainer:IReference
    {
        /// <summary>
        /// 消息目标服务器
        /// </summary>
        [Obsolete]
        internal System.Net.Sockets.Socket msgTargetSocket;
        /// <summary>
        /// 消息目标服务器
        /// </summary>
        internal ClientSocketToken targetToken;
        /// <summary>
        /// 消息
        /// </summary>
        internal MsgBase msg;
        /// <summary>
        /// 已转换完成的消息数组
        /// </summary>
        internal ByteArrayMemory SendBytes;

        public void Release()
        {
            
        }
    }
    /// <summary>
    /// 接收到的客户端消息容器
    /// </summary>
    internal class ClientMsgContainer : IReference
    {
        /// <summary>
        /// 消息来源
        /// </summary>
        internal ClientSocketToken targetToken;

        /// <summary>
        /// 消息
        /// </summary>
        internal MsgBase msg;

        public void Release()
        {

        }
    }
}
