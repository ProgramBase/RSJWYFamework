using System;
using System.IO;
using ProtoBuf;
using RSJWYFamework.Runtime.Network.Public;

namespace RSJWYFamework.Runtime.Default.Manager
{
    public class ProtobufSocketMsgEncode : ISocketMsgEncode
    {
        public byte[] EncodeMsgBody(object msgBase)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, msgBase);
                return ms.ToArray();
            }
        }

        public object DecodeMsgBody(Memory<byte> bytes)
        {
            var bodyBytes = bytes.ToArray();
            using (var memory = new MemoryStream(bodyBytes ,0, bodyBytes.Length))
            {
                MsgBase _tmp = (MsgBase)Serializer.NonGeneric.Deserialize(typeof(MsgBase), memory);
                return _tmp;
            }
        }
    }

    public class ProtobufSocketMsgBodyEncrypt : ISocketMsgBodyEncrypt
    {
        public byte[] Encrypt(Memory<byte> data)
        {
            return data.ToArray();
        }

        public byte[] Decrypt(Memory<byte> data)
        {
            return data.ToArray();
        }
    }


    /// <summary>
    /// 转流，通讯规范，服务器和客户端共同规范
    /// </summary>
    
    /// <summary>
    /// 基础协议类 序列化反序列化，服务器和客户端共用
    /// </summary>、
    [ProtoContract]
    [ProtoInclude(1, typeof(MsgPing))]
    public class MsgBase
    {
        public MsgBase()
        {
            guid = Guid.NewGuid();
        }
        /// <summary>
        /// 协议类型
        /// </summary>
        [ProtoMember(10)]
        public string ProtoType { get; set; }
        /// <summary>
        /// 消息GUID
        /// </summary>
        [ProtoMember(11)]
        public Guid guid;
    
    }
    /// <summary>
    /// 心跳包
    /// </summary>
    [ProtoContract]
    public class MsgPing : MsgBase
    {
        public MsgPing()
        {
            //确定具体是哪个协议，无需标记转流
            ProtoType = this.GetType().FullName;
        }

        [ProtoMember(1)]
        public long timeStamp;
    }
}