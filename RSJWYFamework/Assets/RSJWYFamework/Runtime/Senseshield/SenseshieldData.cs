using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RSJWYFamework.Runtime.Senseshield
{
    /// <summary>
    /// 许可信息
    /// </summary>
    public class SenseShieldLicenseJsonItem
    {
        [JsonProperty("developer_id")]
        public string DeveloperId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("lm")]
        public string LM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("account_name")]
        public string AccountName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("user_guid")]
        public string UserGuid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("host_name")]
        public string HostName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("ip")]
        public string IP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("port")]
        public int Port { get; set; }
        /// <summary>
        /// 锁类型
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
    }
    /// <summary>
    /// 会话信息
    /// </summary>
    public class SenseShieldSessionInfoJson
    {
        /// <summary>
        /// 应用进程 ID
        /// </summary>
        [JsonProperty("app_process_id")]
        public int AppProcessId { get; set; }
        /// <summary>
        /// 应用超时
        /// </summary>
        [JsonProperty("app_time_out")]
        public int AppTimeOut { get; set; }
        /// <summary>
        /// （云锁不支持）
        /// </summary>
        [JsonProperty("bios")]
        public string Bios { get; set; }
        /// <summary>
        /// （云锁不支持）
        /// </summary>
        [JsonProperty("cpuinfo")]
        public string CpuInfo { get; set; }
        /// <summary>
        /// （只支持软锁）
        /// </summary>
        [JsonProperty("user_guid")]
        public string UserGuid { get; set; }
        /// <summary>
        /// （只支持云锁）
        /// </summary>
        [JsonProperty("license_id")]
        public string LicenseId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("mac")]
        public string Mac { get; set; }
    }
    /// <summary>
    /// 许可信息
    /// </summary>
    public class SenseShieldLicenseInfoJson
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty("license_id")]
        public int LicenseId { get; set; }
        /// <summary>
        /// 授权是否有效
        /// </summary>
        [JsonProperty("enable")]
        public bool Enable { get; set; }
        /// <summary>
        /// 许可的guid 
        /// </summary>
        [JsonProperty("guid")]
        public string Guid { get; set; }
        /// <summary>
        /// 授权起始时间
        /// </summary>
        [JsonProperty("start_time")]
        public int StartTime { get; set; }
        /// <summary>
        /// 授权到期时间
        /// </summary>
        [JsonProperty("end_time")]
        public int EndTime { get; set; }
        /// <summary>
        /// 第一次使用时间 
        /// </summary>
        [JsonProperty("first_use_time")]
        public int FirstUseTime { get; set; }
        /// <summary>
        /// 并发类型
        /// </summary>
        [JsonProperty("concurrent_type")]
        public string ConcurrentType { get; set; }
        /// <summary>
        /// 并发数
        /// </summary>
        [JsonProperty("concurrent")]
        public int Concurrent { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("version")]
        public int Version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("module")]
        public string Module { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("last_update_timestamp")]
        public int LastUpdateTimestamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("last_update_timesn")]
        public int LastUpdateTimesn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("bind_node")]
        public int BindNode { get; set; }
        /// <summary>
        /// (云锁不支持）
        /// </summary>
        [JsonProperty("rom_size")]
        public int RomSize { get; set; }
        /// <summary>
        /// (云锁不支持）
        /// </summary>
        [JsonProperty("raw_size")]
        public int RawSize { get; set; }
        /// <summary>
        /// (云锁不支持）
        /// </summary>
        [JsonProperty("pub_size")]
        public int PubSize { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("developer_id")]
        public string DeveloperId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("user_guid")]
        public string UserGuid { get; set; }
    }
    
    /// <summary> 
    /// 锁内文件列表
    /// </summary>
    public class SenseShieldFileJsonItem
    {
        /// <summary>
        /// 标志哪些域有效，不必关心
        /// </summary>
        [JsonProperty("validate")]
        public int Validate { get; set; }
        /// <summary>
        /// (文件类型。二进制文件=0；可执行文件(evx)=1；密钥文件=2)
        /// </summary>
        [JsonProperty("type")]
        public int Type { get; set; }
        /// <summary>
        /// (文件访问权限，最大权限为 0xFF)
        /// </summary>
        [JsonProperty("privilege")]
        public int Privilege { get; set; }
        /// <summary>
        /// (文件大小，单位：字节)
        /// </summary>
        [JsonProperty("size")]
        public int Size { get; set; }
        /// <summary>
        /// (文件创建时间，UTC时间秒)
        /// </summary>
        [JsonProperty("time")]
        public int Time { get; set; }
        /// <summary>
        /// (文件名称)
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }


    #region 所有拥有的锁的信息基类，（软、硬）锁

    /// <summary>
    /// 所有拥有的锁的信息基类，（软、硬）锁
    /// </summary>
    public class SenseShieldAllLockInfoBase
    {
        [JsonProperty("developer_id")]
        public string DeveloperId { get; set; }
        [JsonProperty("host_name")]
        public string HostName { get; set; }
        [JsonProperty("lm")]
        public string Lm { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }
    
    /// <summary>
    /// 所有锁信息-本地锁
    /// </summary>
    public class SenseShieldLocalLockInfo : SenseShieldAllLockInfoBase
    {
        [JsonProperty("sn")]
        public string Sn { get; set; }
        [JsonProperty("lock_info")]
        public SenseShieldLocalLockInfoDetails LockInfo { get; set; }
    }

    /// <summary>
    /// 所有锁信息-本地锁详细信息
    /// </summary>
    public class SenseShieldLocalLockInfoDetails
    {
        [JsonProperty("clock")]
        public long Clock { get; set; }
        [JsonProperty("available_space")]
        public int AvailableSpace { get; set; }
        [JsonProperty("total_space")]
        public int TotalSpace { get; set; }
        [JsonProperty("communication_protocol")]
        public int CommunicationProtocol { get; set; }
        [JsonProperty("lock_firmware_version")]
        public string LockFirmwareVersion { get; set; }
        [JsonProperty("lm_firmware_version")]
        public string LmFirmwareVersion { get; set; }
        [JsonProperty("h5_device_type")]
        public int H5DeviceType { get; set; }
        [JsonProperty("clock_type")]
        public int ClockType { get; set; }
        [JsonProperty("shared_enabled")]
        public int SharedEnabled { get; set; }
        [JsonProperty("owner_developer_id")]
        public string OwnerDeveloperId { get; set; }
        [JsonProperty("device_model")]
        public string DeviceModel { get; set; }
        [JsonProperty("hardware_version")]
        public string HardwareVersion { get; set; }
        [JsonProperty("manufacture_date")]
        public DateTime ManufactureDate { get; set; }
        [JsonProperty("lock_sn")]
        public string LockSn { get; set; }
        [JsonProperty("slave_addr")]
        public int SlaveAddr { get; set; }
        [JsonProperty("shell_num")]
        public string ShellNum { get; set; }
        [JsonProperty("user_info")]
        public string UserInfo { get; set; }
        [JsonProperty("inner_info")]
        public string InnerInfo { get; set; }
    }
    /// <summary>
    /// 所有锁信息-软锁
    /// </summary>
    public class SenseShieldSLockLockInfo : SenseShieldAllLockInfoBase
    {
        [JsonProperty("account_name")]
        public string AccountName { get; set; }
        [JsonProperty("user_guid")]
        public string UserGuid { get; set; }
        [JsonProperty("ip")]
        public string Ip { get; set; }
        [JsonProperty("port")]
        public int Port { get; set; }
    }
   
    /// <summary>
    /// 处理获取到的本地所有锁信息时
    /// </summary>
    public class MixedSenseShieldAllLockInfoConverter : JsonConverter
    {
        /// <summary>
        /// 检查类型
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SenseShieldAllLockInfoBase[]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var tokens = JArray.Load(reader);
            var list = new List<SenseShieldAllLockInfoBase>();

            foreach (JObject item in tokens)
            {
                if (item["type"].Value<string>()=="local")
                {
                    list.Add(serializer.Deserialize<SenseShieldLocalLockInfo>(item.CreateReader()));
                }
                else if(item["type"].Value<string>()=="slock")
                {
                    list.Add(serializer.Deserialize<SenseShieldSLockLockInfo>(item.CreateReader()));
                }
            }

            SenseShieldAllLockInfoBase[] arr = list.ToArray();
            return arr;
            
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is SenseShieldSLockLockInfo withDetails)
            {
                // 序列化带有详细信息的对象
                serializer.Serialize(writer, withDetails);
            }
            else if (value is SenseShieldLocalLockInfo simpleInfo)
            {
                // 序列化简单信息的对象
                serializer.Serialize(writer, simpleInfo);
            }
            else
            {
                throw new JsonSerializationException("未知的锁信息类型。");
            }
        }
    }
    

    #endregion
}