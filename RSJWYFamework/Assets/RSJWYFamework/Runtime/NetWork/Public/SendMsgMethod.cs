using RSJWYFamework.Runtime.Net.Public;

namespace RSJWYFamework.Runtime.NetWork.Public
{
    public static partial class SendMsgMethod
    {
        /// <summary>
        /// 心跳包
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static MsgPing SendMsgPing(long timeStamp)
        {
            MsgPing _MsgPing = new()
            {
                timeStamp =Utility.Utility.GetTimeStamp()
            };
            return _MsgPing;
        }
    }
}