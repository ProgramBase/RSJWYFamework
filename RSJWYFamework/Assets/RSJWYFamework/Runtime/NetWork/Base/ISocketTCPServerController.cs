

using MyFamework.Runtime.Base;
using RSJWYFamework.Runtime.Event;
using RSJWYFamework.Runtime.Net.Public;
using RSJWYFamework.Runtime.NetWork.TCP.Server;

namespace RSJWYFamework.Runtime.NetWork.Base
{
    /// <summary>
    /// socket服务端控制器接口
    /// </summary>
    public interface ISocketTCPServerController:ModleInterface
    {
        /// <summary>
        /// 客户端链接上来事件回调-多线程下调用
        /// </summary>
        /// <param name="eventEnum"></param>
        void ClientConnectedCallBack(ClientSocketToken clientSocketToken);

        /// <summary>
        /// 客户端断开链接事件回调-多线程下调用
        /// </summary>
        /// <param name="eventEnum"></param>
        void CloseClientReCallBack(ClientSocketToken clientSocketToken);
        /// <summary>
        /// 服务端状态广播-多线程下调用
        /// </summary>
        /// <param name="netServerStatus"></param>
        void ServerServiceStatus(NetServerStatus netServerStatus);
        /// <summary>
        /// 接收到消息时的回调-多线程下调用
        /// </summary>
        /// <param name="msgBase"></param>
        void FromClientReceiveMsgCallBack(ClientSocketToken clientSocketToken, MsgBase msgBase);

        /// <summary>
        /// 向客户端发消息
        /// </summary>
        void SendMsgToClient(MsgBase msgBase,ClientSocketToken clientSocketToken);

        /// <summary>
        /// 向所有已链接上来的客户端发消息
        /// </summary>
        void SendMsgToClientAll(MsgBase msgBase);
        /// <summary>
        /// 初始化设置
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        void InitServer(string ip = "127.0.0.1", int port = 6000);
        
        
        /// <summary>
        /// 添加时初始化
        /// </summary>
        void Init();
        /// <summary>
        /// 关闭
        /// </summary>
        void Close();
    }
}