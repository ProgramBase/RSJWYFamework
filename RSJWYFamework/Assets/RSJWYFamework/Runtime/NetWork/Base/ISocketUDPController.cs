using MyFamework.Runtime.Base;
using RSJWYFamework.Runtime.NetWork.Public;
using RSJWYFamework.Runtime.NetWork.UDP;

namespace RSJWYFamework.Runtime.NetWork.Base
{
    public interface ISocketUDPController:ModleInterface
    {

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init();

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close();
        
        /// <summary>
        /// 初始化UDP监听
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void InitListen(string ip="any", int port=5000);

        /// <summary>
        /// 接收到的数组指令-多线程调用，注意线程安全
        /// </summary>
        /// <param name="bytes">指令</param>
        public void ReceiveMsgCallBack(UDPMsg bytes);
    }
}