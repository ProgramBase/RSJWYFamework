using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using ProtoBuf;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Network.Public;
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
        
        // 定义一个固定的字节数组作为心跳包
        public static readonly byte[]  HeartbeatPacket = new byte[] 
        { 
            0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x11, 0x22, 
            0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00 
        };

        /// <summary>
        /// 消息体序列化
        /// </summary> 
        /// <param name="msgBase">协议体（消息）</param>
        /// <param name="iSocketMsgBodyEncrypt">加密接口</param>
        /// <param name="iSocketMsgEncode">编解码接口</param>
        /// <returns>返回序列化后的消息数组</returns>
        public static byte[] EncodeMsgBody(object msgBase,ISocketMsgBodyEncrypt iSocketMsgBodyEncrypt,ISocketMsgEncode iSocketMsgEncode)
        {
            var encodebody = iSocketMsgEncode.EncodeMsgBody(msgBase);
            return iSocketMsgBodyEncrypt.Encrypt(encodebody);
        }
        
        /// <summary>
        /// 消息体反序列化
        /// </summary>
        /// <param name="bytes">消息数组</param>
        /// <param name="iSocketMsgBodyEncrypt">加密接口</param>
        /// <returns>解析后的协议体（即消息）</returns>
        public static object DecodeMsgBody(Memory<byte> bytes ,ISocketMsgBodyEncrypt iSocketMsgBodyEncrypt,ISocketMsgEncode iSocketMsgEncode)
        {
            if (iSocketMsgBodyEncrypt == null || iSocketMsgEncode==null)
            {
                throw new RSJWYException(RSJWYFameworkEnum.NetworkTool, $"所需要的2个接口不能为空-加解密-编解码");
            }
            //协议反序列化
            try
            {
                // 直接使用 Array.Copy 进行解密而不是先拷贝再解密
                var bodyBytes =  iSocketMsgBodyEncrypt.Decrypt(bytes); // 解密协议体
                // 反序列化协议体
                return iSocketMsgEncode.DecodeMsgBody(bodyBytes);
                
            }
            catch (Exception ex)
            {
                Debug.LogError($"解密协议出错: {ex}");
                return null;
            }
        }

        /// <summary>
        /// 反序列化本次的消息
        /// </summary>
        /// <param name="byteArray">存储消息的自定义数组</param>
        /// <param name="iSocketMsgBodyEncrypt">解密接口服务</param>
        /// <param name="iSocketMsgEncode">消息序列化反序列化接口</param>
        /// <returns>封装的消息信息</returns>
        internal static (object msgBase,bool IsPingPong) DecodeMsg(Memory<byte> byteArray,ISocketMsgBodyEncrypt iSocketMsgBodyEncrypt,ISocketMsgEncode iSocketMsgEncode)
        {
            //确认数据是否有误
            if (byteArray.Length <= 0)
            {
                throw new RSJWYException(RSJWYFameworkEnum.NetworkTool, $"发生异常，数据长度于0，无法对数据处理");
            }
            //解析协议体-计算协议体长度
            int bodyCount = byteArray.Length - 4; //剩余数组长度（剩余的长度就是协议体长度）要去掉校验码
            //映射处理
            Memory<byte> remoteCRC32Bytes = byteArray.Slice(bodyCount, 4);//取校验码
            Memory<byte> bodyBytes = byteArray.Slice(0,bodyCount);//取数据载体
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
                //对比是不是心跳包
                if (!bodyBytes.Span.SequenceEqual(HeartbeatPacket))
                {
                    //解析协议体
                    var msgBase = DecodeMsgBody(bodyBytes, iSocketMsgBodyEncrypt,iSocketMsgEncode);
                    if (msgBase == null)
                    {
                        throw new RSJWYException(RSJWYFameworkEnum.NetworkTool,$"反序列化消息失败！返回的装箱消息为空！");
                    }
                    return (msgBase,false);
                }
                else
                {
                    return (null,true);
                }
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
        /// <param name="msgBase">要处理的消息体</param>
        /// <param name="iSocketMsgBodyEncrypt">加密接口</param>
        /// <param name="iSocketMsgEncode">消息序列化反序列化接口</param>
        /// <returns>序列化后的消息</returns>
        internal static ByteArrayMemory EncodeMsg(object msgBase,ISocketMsgBodyEncrypt iSocketMsgBodyEncrypt,ISocketMsgEncode iSocketMsgEncode)
        {
            // 协议体编码
            //Memory<byte> nameBytes = EncodeName(msgBase); // 协议名编码
            Memory<byte> bodyBytes = EncodeMsgBody(msgBase,iSocketMsgBodyEncrypt,iSocketMsgEncode); // 编码协议体

            // 获取校验码
            uint crc32 = Utility.Utility.CRC32.GetCrc32(bodyBytes.ToArray()); // 获取协议体的CRC32校验码
            Memory<byte> bodyCrc32Bytes = Utility.Utility.UIntToByteArray(crc32); // 获取协议体的CRC32校验码数组

            // 总长度计算
            int totalLength = bodyBytes.Length + bodyCrc32Bytes.Length + sizeof(int); // 加上头部长度的大小

            // 创建足够大小的数组来存储整个消息
            Memory<byte> sendBytes = new byte[totalLength];


            // 组装字节流数据
            // //编码记录长度（头部长度不包括头部本身的长度），并复制到memory开始位
            BitConverter.GetBytes(totalLength - sizeof(int)).AsSpan().CopyTo(sendBytes.Span); 
            //复制数据到memory（从指定位置开始）
            bodyBytes.CopyTo(sendBytes.Slice(sizeof(int)));
            bodyCrc32Bytes.CopyTo(sendBytes.Slice(sizeof(int) + bodyBytes.Length));

            // 将拼接好的信息用自定义的消息数组保存
            ByteArrayMemory ba = new ByteArrayMemory(sendBytes);
            
            return ba;
        }
        /// <summary>
        /// 创建心跳包特征消息-不执行任何加密
        /// </summary>
        /// <returns></returns>
        internal static ByteArrayMemory SendPingPong()
        {
            var bodyBytes = HeartbeatPacket;
            uint crc32 = Utility.Utility.CRC32.GetCrc32(bodyBytes.ToArray()); // 获取协议体的CRC32校验码
            Memory<byte> bodyCrc32Bytes = Utility.Utility.UIntToByteArray(crc32); // 获取协议体的CRC32校验码数组
            // 总长度计算
            int totalLength = bodyBytes.Length + bodyCrc32Bytes.Length + sizeof(int); // 加上头部长度的大小
            // 创建足够大小的数组来存储整个消息
            Memory<byte> sendBytes = new byte[totalLength];
            // 组装字节流数据
            // //编码记录长度（头部长度不包括头部本身的长度），并复制到memory开始位
            BitConverter.GetBytes(totalLength - sizeof(int)).AsSpan().CopyTo(sendBytes.Span); 
            //复制数据到memory（从指定位置开始）
            bodyBytes.CopyTo(sendBytes.Slice(sizeof(int)));
            bodyCrc32Bytes.CopyTo(sendBytes.Slice(sizeof(int) + bodyBytes.Length));

            // 将拼接好的信息用自定义的消息数组保存
            ByteArrayMemory ba = new ByteArrayMemory(sendBytes);
            return ba;
        }

    }
}