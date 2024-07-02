using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RSJWYFamework.Runtime.Utility
{
    public static partial class Utility
    {
        /*
        /// <summary>
        /// 设置时区的信息
        /// </summary>
        static TimeZoneInfo timeZoneInfo;
        public RSJWYTool()
        {
            ////获取所支持的时区
            //var timeZones = TimeZoneInfo.GetSystemTimeZones();
            //foreach (TimeZoneInfo timeZone in timeZones)
            //{
            //    Debug.Log(timeZone.Id);
            //}
            //时区设置
            timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
        }*/
        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        /// <returns>时间戳</returns>
        public static long GetTimeStamp()
        {
            //根据当前时区计算时间戳
            //TimeSpan ts = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo) - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); 
            DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
            long timestamp = nowUtc.ToUnixTimeSeconds();
            return Convert.ToInt64(timestamp);
        }
        /// <summary>
        /// 16进制的字符串转为字节数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            // 确保字符串长度是偶数
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("The hex string length must be even.", nameof(hexString));
            }

            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
            {
                // 将每两个字符转换为一个字节
                string hexByte = hexString.Substring(i, 2);
                bytes[i / 2] = Convert.ToByte(hexByte, 16);
            }
            return bytes;
        }
        
        /// <summary>
        /// 字符串转JSON
        /// </summary>
        /// <param name="JsonTxT"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T LoadJson<T>(string JsonTxT)
        {
            return JsonConvert.DeserializeObject<T>(JsonTxT);
        }
        /// <summary>
        /// 二维数组转一维
        /// </summary>
        /// <param name="jaggedArray"></param>
        /// <returns></returns>
        public static byte[] ConvertJaggedArrayToOneDimensional(byte[][] jaggedArray)
        {
            List<byte> flatList = new List<byte>();
            for (int i = 0; i < jaggedArray.Length; i++)
            {
                for (int j = 0; j < jaggedArray[i].Length; j++)
                {
                    flatList.Add(jaggedArray[i][j]);
                }
            }
            return flatList.ToArray();
        }
    }
}