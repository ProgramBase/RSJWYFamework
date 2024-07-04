using RSJWYFamework.Runtime.NetWork.UDP;

namespace RSJWYFamework.Runtime.NetWork.Base
{
    public interface ISocketUDPController
    {
        /// <summary>
        /// 初始化UDP监听
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void InitListen(string ip="any", int port=5000);

        /// <summary>
        /// 接收到的字符串指令
        /// </summary>
        /// <param name="command">指令</param>
        public void ReceiveMsgCallBack(string command);
    }
}