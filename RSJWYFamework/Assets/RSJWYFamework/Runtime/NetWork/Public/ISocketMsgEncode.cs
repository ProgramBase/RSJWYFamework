using System;

namespace RSJWYFamework.Runtime.Network.Public
{
    /// <summary>
    /// 消息编解码接口
    /// </summary>
    public interface ISocketMsgEncode
    {
        /// <summary>
        /// 对消息体进行编码
        /// </summary>
        /// <param name="msgBase">消息类型</param>
        /// <returns>编码的消息</returns>
        byte[] EncodeMsgBody(object msgBase);

        /// <summary>
        /// 对消息体进行解码
        /// </summary>
        /// <param name="bytes">待解码的消息体本体-内存切片</param>
        /// <returns>解码后的装箱数据</returns>
        object DecodeMsgBody(Memory<byte> bytes);
    }
}