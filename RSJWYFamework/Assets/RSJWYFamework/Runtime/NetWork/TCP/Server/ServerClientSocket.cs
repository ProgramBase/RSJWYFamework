using System;
using System.Net;
using RSJWYFamework.Runtime.Net.Public;
using RSJWYFamework.Runtime.NetWork.Public;

namespace RSJWYFamework.Runtime.NetWork.TCP.Server
{
    /// <summary>
    /// 服务器模块 单独客户端容器，存储客户端的相关信息等
    /// </summary>
    public class ClientSocketToken
    {
        /// <summary>
        /// 存储连接的客户端
        /// </summary>
        public System.Net.Sockets.Socket socket { get;internal set; }
        /// <summary>
        /// 初次连接时间（心跳包）
        /// </summary>
        public long lastPingTime { get; internal set; }
        /// <summary>
        /// 存储数据-接收到的数据暂存容器
        /// </summary>
        internal ByteArray ReadBuff;
        
        /// <summary>  
        /// 客户端IP地址  
        /// </summary>  
        public IPAddress IPAddress { get; set; }  
  
        /// <summary>  
        /// 远程地址  
        /// </summary>  
        public EndPoint Remote { get; set; }  
        
        /// <summary>  
        /// 连接时间  
        /// </summary>  
        public DateTime ConnectTime { get; set; }  

        /// <summary>
        /// 客户端汇报的ID
        /// 作为客户端的唯一标识符，以免出现同一个客户端多链接，
        /// 需要做一个定时检查，出现重复拒绝链接以及移除已有链接，重新发起连接
        /// </summary>
        /// <returns></returns>
        public string TokenID;
    }
    /// <summary>
    /// 服务器模块 消息发送数据容器
    /// </summary>
    internal class ServerToClientMsg
    {
        /// <summary>
        /// 消息目标服务器
        /// </summary>
        internal System.Net.Sockets.Socket msgTargetSocket;
        /// <summary>
        /// 消息
        /// </summary>
        internal MsgBase msg;
        /// <summary>
        /// 已转换完成的消息数组
        /// </summary>
        internal ByteArray sendBytes;
    }
}
