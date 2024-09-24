using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using ProtoBuf;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Net.Public;
using RSJWYFamework.Runtime.Utility;
using RSJWYFamework.Runtime.Utility.Extensions;
using UnityEngine;

namespace RSJWYFamework.Runtime.NetWork.Public
{
    public static partial class MessageTool
    {
        
        
        /// <summary>
        /// 设置时区的信息
        /// </summary>
        static TimeZoneInfo timeZoneInfo;


        /// <summary>
        /// 消息加密密钥
        /// </summary>
        internal const string MsgKey = "1'dXj:=}kv*gM[*.bk9!LZ:Fk(Vp}l.tH;Q.d4BHbbX#p.vm7a/3{yXAIGec3vby";


        static MessageTool()
        {
            ////获取所支持的时区
            //var timeZones = TimeZoneInfo.GetSystemTimeZones();
            //foreach (TimeZoneInfo timeZone in timeZones)
            //{
            //    Debug.Log(timeZone.Id);
            //}
            //时区设置
            timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
        }

        /// <summary>
        /// 编码协议名
        /// </summary>
        /// <param name="msgBase">要编码的消息</param>
        /// <returns></returns>
        public static byte[] EncodeName(MsgBase msgBase)
        {
            byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msgBase.ProtoType.ToString());//编码协议名
            byte[] AESNameBytes =Utility.Utility.AESTool.AESEncrypt(nameBytes, MessageTool.MsgKey); //加密协议名
            uint nameLen = (uint)AESNameBytes.Length;//记录协议名长度
            byte[] nameLenBytes =Utility.Utility.UIntToByteArray(nameLen);//获取记录长度的数组
            byte[] bytes = new byte[4 + nameLen];//开新的存储空间用于返回编码好的协议长度及协议名（协议长度通过前面4位数组存储
            //拷贝并存储
            Array.Copy(nameLenBytes, 0, bytes, 0, 4);//将协议名长度拷贝到数组中
            Array.Copy(AESNameBytes, 0, bytes, 4, nameLen);//将编码好的协议名拷贝到存储空间去，前两位存储长度，所以从第三位存储协议名
            return bytes;
        }
        /// <summary>
        /// 解码协议名
        /// </summary>
        /// <param name="bytes">数据数组</param>
        /// <param name="offset">开始读索引</param>
        /// <param name="count">返回存储协议名信息的数组长度（包含协议名长度和协议名体长度）</param>
        /// <returns>返回解析出来的协议</returns>
        public static MyProtocolEnum DecodeName(byte[] bytes, int offset, out int count)
        {
            count = 0;//初始0
            //判断存储协议名长度的数组是否存在
            if (offset+4 > bytes.Length)
            {
                //存储协议名长度的数组不存在
                Debug.LogError("解码协议长度出错！！存储协议名长度的数组位不存在，无法获取本条协议名" );
                return MyProtocolEnum.None;//协议不存在
            }
            
            int len = (int)Utility.Utility.ByteArrayToUInt(bytes.GetFromByteArrToMemory(offset,4).ToArray());//解码协议名长度
            if (offset+4 + len > bytes.Length)
            {
                //只包含了协议名长度信息，但与之对应存储协议名称的数组不存在
                Debug.LogError("解码协议名出错！！只包含了协议长度信息，没有包协议名数组!!");
                return MyProtocolEnum.None;//协议不存在
            }
            //协议长度和协议名都存在
            //记录存储协议名信息的数组长度，并返回，规避存储协议名长度的位置，以方便解析协议体
            count = 4 + len;
            //开始解析
            try
            { 
                //解密协议名
                byte[] nameBytes = Utility.Utility.AESTool.AESDecrypt(bytes.GetFromByteArrToMemory(offset+4,len).ToArray(), MsgKey);
                //确认协议类型
                string name = System.Text.Encoding.UTF8.GetString(nameBytes);
                MyProtocolEnum _mpe = (MyProtocolEnum)System.Enum.Parse(typeof(MyProtocolEnum), name);//查找对应的枚举
                return _mpe;
            }
            catch (Exception ex)
            {
                Debug.LogError($"出现错误，传入的协议不存在,无法从本地枚举信息中找到传入的枚举，错误信息：{ex.ToString()} ");
                return MyProtocolEnum.None;
            }
        }
        /// <summary>
        /// 消息体序列化
        /// </summary>
        /// <param name="msgBase">协议体（消息）</param>
        /// <returns>返回序列化后的消息数组</returns>
        public static byte[] EncodeMsgBody(MsgBase msgBase)
        {
            using (var mermory = new MemoryStream())
            {
                //将协议类进行序列化，转换成数组
                Serializer.Serialize(mermory, msgBase);
                byte[] bytes = mermory.ToArray();

                //对协议体加密
                byte[] AESBodyBytes = Utility.Utility.AESTool.AESEncrypt(bytes, MsgKey);

                return AESBodyBytes;
            }
        }
        
        /// <summary>
        /// 消息体反序列化
        /// </summary>
        /// <param name="protocol">协议名称枚举</param>
        /// <param name="bytes">消息数组</param>
        /// <param name="offset">开始读索引</param>
        /// <param name="count">整条消息的长度</param>
        /// <returns>解析后的协议体（即消息）</returns>
        public static MsgBase DecodeMsgBody(MyProtocolEnum protocol,byte[] bytes ,int offset,int count)
        {
            if (count<=0)
            {
                Debug.LogError("协议解密出错，数据长度为0");
                return null;
            }
            //协议反序列化
            try
            {
                byte[] AESBytes = new byte[count];
                Array.Copy(bytes,offset, AESBytes, 0,count);//拷贝到AESBytes
                byte[] bodyBytes= Utility.Utility.AESTool.AESDecrypt(AESBytes, MsgKey);//解密协议体
                //反序列化
                using (var memory=new MemoryStream(bodyBytes, 0, bodyBytes.Length))
                {
                    Type t = System.Type.GetType($"{typeof(MsgBase).Namespace}.{protocol.ToString()}");//转化类
                    MsgBase _tmp= (MsgBase)Serializer.NonGeneric.Deserialize(t, memory);
                    return _tmp;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"协议{protocol}解密出错: {ex}");
                return null;
            }
        }
        /// <summary>
        /// 反序列化本次的消息
        /// </summary>
        /// <param name="byteArray">存储消息的自定义数组</param>
        /// <param name="msgLength">消息整体长度</param>
        /// <returns>解码后的消息</returns>
        internal static MsgBase DecodeMsg(byte[] byteArray)
        {
            //确认数据是否有误
            if (byteArray.Length <= 0)
            {
                throw new RSJWYException(RSJWYFameworkEnum.NetworkTool, $"发生异常，数据长度于0，无法对数据处理");
            }
            var readIndex = 0;
             //解析完协议名后要从哪开始读下一阶段的数据
            MyProtocolEnum protocol = DecodeName(byteArray, 0, out var nameCount); //解析协议名
            if (protocol == MyProtocolEnum.None)
            {
                throw new RSJWYException(RSJWYFameworkEnum.NetworkTool,$"解析协议名出错,协议名不存在！！返回的协议名为: {MyProtocolEnum.None.ToString()}");
            }
            //读取没有问题
            readIndex += nameCount; //移动开始读位置，开始解析协议体
            //解析协议体-计算协议体长度
            int bodyCount = byteArray.Length - nameCount - 4; //剩余数组长度（剩余的长度就是协议体长度）要去掉校验码
            //映射处理
            Memory<byte> byteArrayMemory = byteArray.AsMemory();
            Memory<byte> remoteCRC32Bytes = byteArrayMemory.Slice(readIndex + bodyCount, 4);//取校验码
            Memory<byte> bodyBytes = byteArrayMemory.Slice(readIndex, bodyCount);//取数据载体
            //校验CRC32
            uint localCRC32 = Utility.Utility.CRC32.GetCrc32(bodyBytes.ToArray()); //获取协议体的CRC32校验码
            uint remoteCRC32 = Utility.Utility.ByteArrayToUInt(remoteCRC32Bytes.ToArray()); // 运算获取远端计算的验码
            if (localCRC32 != remoteCRC32)
            {
                throw new RSJWYException(RSJWYFameworkEnum.NetworkTool,$"CRC32校验失败！！远端CRC32：{remoteCRC32}，本机计算的CRC32：{localCRC32}。协议体的一致性遭到破坏");
            }
            //校验码检测通过
            try
            {
                //解析协议体
                MsgBase msgBase = DecodeMsgBody(protocol, byteArray, readIndex, bodyCount);
                if (msgBase == null)
                {
                    throw new RSJWYException(RSJWYFameworkEnum.NetworkTool,$"解析协议名出错！！无法匹配协议基类协议名不存在！！返回的协议名为: {MyProtocolEnum.None.ToString()}");
                }
                return msgBase;
            }
            catch (SocketException ex)
            {
                throw new RSJWYException(RSJWYFameworkEnum.NetworkTool, $"反序列化消息时发生错误：{ex}");
            }
            finally
            {
            }
        }
        /// <summary>
        /// 将消息进行序列化
        /// </summary>
        /// <param name="msgBase"></param>
        /// <returns>序列化后的消息</returns>
        internal static ByteArrayMemory EncodeMsg(MsgBase msgBase)
        {
            // 协议体编码
            Memory<byte> nameBytes = EncodeName(msgBase); // 协议名编码
            Memory<byte> bodyBytes = EncodeMsgBody(msgBase); // 编码协议体

            // 获取校验码
            uint crc32 = Utility.Utility.CRC32.GetCrc32(bodyBytes.ToArray()); // 获取协议体的CRC32校验码
            Memory<byte> bodyCrc32Bytes = Utility.Utility.UIntToByteArray(crc32); // 获取协议体的CRC32校验码数组

            // 总长度计算
            int totalLength = nameBytes.Length + bodyBytes.Length + bodyCrc32Bytes.Length + sizeof(int); // 加上头部长度的大小

            // 创建足够大小的数组来存储整个消息
            Memory<byte> sendBytes = new byte[totalLength];

            // 使用 Memory<byte> 来避免多次拷贝
            Memory<byte> sendMemory = sendBytes;

            // 组装字节流数据
            // //编码记录长度（头部长度不包括头部本身的长度），并复制到memory开始位
            BitConverter.GetBytes(totalLength - sizeof(int)).AsSpan().CopyTo(sendMemory.Span); 
            //复制数据到memory（从指定位置开始）
            nameBytes.CopyTo(sendMemory.Slice(sizeof(int)));
            bodyBytes.CopyTo(sendMemory.Slice(sizeof(int) + nameBytes.Length));
            bodyCrc32Bytes.CopyTo(sendMemory.Slice(sizeof(int) + nameBytes.Length + bodyBytes.Length));

            // 将拼接好的信息用自定义的消息数组保存
            ByteArrayMemory ba = new ByteArrayMemory(sendBytes);
            
            return ba;
        }

    }
}