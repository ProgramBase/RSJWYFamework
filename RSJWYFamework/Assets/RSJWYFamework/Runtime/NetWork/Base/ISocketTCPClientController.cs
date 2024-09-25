using MyFamework.Runtime.Base;
using RSJWYFamework.Runtime.Event;
using RSJWYFamework.Runtime.Net.Public;
using RSJWYFamework.Runtime.NetWork.TCP.Client;

namespace RSJWYFamework.Runtime.Socket.Base
{
    /// <summary>
    /// socket客户端控制器接口
    /// </summary>
    public interface ISocketTCPClientController:ModleInterface
    {
        /// <summary>
        /// 连接事件回调
        /// </summary>
        /// <param name="eventEnum">链接枚举</param>
        void ClientStatus(NetClientStatus eventEnum);
        /// <summary>
        /// 接收到消息时的回调
        /// </summary>
        /// <param name="msgBase"></param>
        void ReceiveMsgCallBack(MsgBase msgBase);
        
        /// <summary>
        /// 向服务器发消息
        /// </summary>
        void ClientSendToServerMsg(MsgBase msg);
        
        /// <summary>
        /// 初始化设置
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        void InitTCPClient(string ip = "127.0.0.1", int port = 6000);
        
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