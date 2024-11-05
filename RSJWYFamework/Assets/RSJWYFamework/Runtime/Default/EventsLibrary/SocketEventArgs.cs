using System.Text;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Event;
using RSJWYFamework.Runtime.NetWork.TCP.Client;
using RSJWYFamework.Runtime.NetWork.TCP.Server;

namespace RSJWYFamework.Runtime.Default.EventsLibrary
{
    /// <summary>
    /// TCP服务端事件
    /// </summary>
    public abstract record TCPServerSoketEventArgs:RecordEventArgsBase
    {
        /// <summary>
        /// 消息的关联客户端
        /// </summary>
        public ClientSocketToken ClientSocketToken;
        /// <summary>
        /// 消息的载体
        /// </summary>
        public MsgBase msgBase;
    }
    public abstract record TCPClientSoketEventArgs:RecordEventArgsBase
    {
        /// <summary>
        /// 消息载体
        /// </summary>
        public MsgBase msgBase;
    }

    #region TCP服务端
    /// <summary>
    /// 客户端连接上来的事件
    /// </summary>
    public sealed record ServerClientConnectedCallBackEventArgs :TCPServerSoketEventArgs
    {
    }
    /// <summary>
    /// 客户端离线的事件
    /// </summary>
    public sealed record ServerCloseClientCallBackEventArgs : TCPServerSoketEventArgs
    {
    }
    /// <summary>
    /// 收到客户端发来的消息
    /// </summary>
    public sealed record FromClientReceiveMsgCallBackEventArgs : TCPServerSoketEventArgs
    {
    }
    /// <summary>
    /// 向客户端发送消息
    /// </summary>
    public sealed record ServerToClientMsgEventArgs : TCPServerSoketEventArgs
    {
    }
    /// <summary>
    /// 向所有客户端发送消息
    /// </summary>
    public sealed record ServerToClientMsgAllEventArgs : TCPServerSoketEventArgs
    {
    }
    /// <summary>
    /// 服务端状态事件
    /// </summary>
    public sealed record ServerStatusEventArgs : RecordEventArgsBase
    {
        public NetServerStatus status;
    }
    #endregion

    #region TCP客户端

    /// <summary>
    /// 客户端状态消息
    /// </summary>
    public sealed record ClientStatusEventArgs : RecordEventArgsBase
    {
        public NetClientStatus netClientStatus;
    }
    /// <summary>
    /// 向服务器发送消息
    /// </summary>
    public sealed record ClientSendToServerEventArgs : TCPClientSoketEventArgs
    {
    }
    /// <summary>
    /// 接收到服务器发来的消息
    /// </summary>
    public sealed record ClientReceivesMSGFromServer: TCPClientSoketEventArgs
    {
    }

    #endregion

    #region UDP服务

    /// <summary>
    /// UDP事件基类
    /// </summary>
    public abstract class UDPSoketEventArgs:EventArgsBase
    {
        public StringBuilder command;
        
        public string ip;
        public int port;
        public override void Release()
        {
            command.Clear();
        }
    }

    /// <summary>
    /// UDP接收到消息
    /// </summary>
    public sealed class UDPReceiveMsgCallBack : UDPSoketEventArgs
    {
        
    }
    /// <summary>
    /// UDP发送消息
    /// </summary>
    public sealed class UDPSendMsg : UDPSoketEventArgs
    {
    }



    #endregion
}