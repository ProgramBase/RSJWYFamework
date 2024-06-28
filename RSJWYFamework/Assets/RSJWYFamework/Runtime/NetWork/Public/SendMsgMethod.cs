using RSJWYFamework.Runtime.Net.Public;
using RSJWYFamework.Runtime.Utility;
using UnityEngine;

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
            timeStamp =Utility.GetTimeStamp()
        };
        return _MsgPing;
    }
}