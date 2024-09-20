using System.Text;
using RSJWYFamework.Runtime.Event;
using RSJWYFamework.Runtime.Net.Public;
using RSJWYFamework.Runtime.NetWork.TCP.Client;
using RSJWYFamework.Runtime.NetWork.TCP.Server;

namespace RSJWYFamework.Runtime.NetWork.Event
{
    /// <summary>
    /// TCP服务端事件
    /// </summary>
    public abstract class TCPServerSoketEventArgs:EventArgsBase
    {
        public ClientSocketToken ClientSocketToken { get; internal set; }
        public override void Release()
        {
            ClientSocketToken = null;
        }
    }
    public abstract class TCPClientSoketEventArgs:EventArgsBase
    {
        public MsgBase clientSocket;
        public override void Release()
        {
            clientSocket = null;
        }
    }

    #region TCP服务端
    /// <summary>
    /// 客户端连接上来的事件
    /// </summary>
    public sealed class ServerClientConnectedCallBackEventArgs :TCPServerSoketEventArgs
    {
        public override void Release()
        {
            base.Release();
        }
    }
    /// <summary>
    /// 客户端离线的事件
    /// 在接收到本消息时，已从controller中移除，广播完后，将关闭并移除
    /// </summary>
    public sealed class ServerClientReConnectedCallBackEventArgs : TCPServerSoketEventArgs
    {
        public override void Release()
        {
            base.Release();
        }
    }
    /// <summary>
    /// 收到客户端发来的消息
    /// </summary>
    public sealed class FromClientReceiveMsgCallBackEventArgs : TCPServerSoketEventArgs
    {
        public MsgBase msgBase { get; internal set; }
        public override void Release()
        {
            base.Release();
        }
    }
    /// <summary>
    /// 向客户端发送消息
    /// </summary>
    public sealed class ServerToClientMsgEventArgs : TCPServerSoketEventArgs
    {
        public MsgBase msgBase;
        public override void Release()
        {
            base.Release();
        }
    }
    /// <summary>
    /// 向所有客户端发送消息
    /// </summary>
    public sealed class ServerToClientMsgAllEventArgs : TCPServerSoketEventArgs
    {
        public MsgBase msgBase;
        public override void Release()
        {
            base.Release();
        }
    }
    /// <summary>
    /// 服务端状态事件
    /// </summary>
    public sealed class ServerStatusEventArgs : EventArgsBase
    {
        public NetServerStatus status;
        public override void Release()
        {
            status = NetServerStatus.None;
        }
    }
    #endregion

    #region TCP客户端

    /// <summary>
    /// 客户端状态消息
    /// </summary>
    public sealed class ClientStatusEventArgs : EventArgsBase
    {
        public NetClientStatus netClientStatus { get; internal set; }
        public override void Release()
        {
            netClientStatus = NetClientStatus.None;
        }
    }
    /// <summary>
    /// 向服务器发送消息
    /// </summary>
    public sealed class ClientSendToServerEventArgs : EventArgsBase
    {
        public MsgBase msg;
        public override void Release()
        {
            msg = null;
        }
    }
    /// <summary>
    /// 接收到服务器发来的消息
    /// </summary>
    public sealed class ClientReceivesMSGFromServer: EventArgsBase
    {
        public MsgBase msg { get; internal set; }
        public override void Release()
        {
            msg = null;
        }
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