using RSJWYFamework.Runtime.NetWork.Public;
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
        /// 接收到的数组指令
        /// </summary>
        /// <param name="bytes">指令</param>
        public void ReceiveMsgCallBack(UDPReciveMsg bytes);
    }
}