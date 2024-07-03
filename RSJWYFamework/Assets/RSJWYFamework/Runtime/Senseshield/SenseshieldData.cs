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
        /// <summary>
        /// 
        /// </summary>+
        public string developer_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string lm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string account_name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string user_guid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string host_name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ip { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int port { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string type { get; set; }
    }
    /// <summary>
    /// 会话信息
    /// </summary>
    public class SenseShieldSessionInfoJson
    {
        /// <summary>
        /// 应用进程 ID
        /// </summary>
        public int app_process_id { get; set; }
        /// <summary>
        /// 应用超时
        /// </summary>
        public int app_time_out { get; set; }
        /// <summary>
        /// （云锁不支持）
        /// </summary>
        public string bios { get; set; }
        /// <summary>
        /// （云锁不支持）
        /// </summary>
        public string cpuinfo { get; set; }
        /// <summary>
        /// （只支持软锁）
        /// </summary>
        public string user_guid { get; set; }
        /// <summary>
        /// （只支持云锁）
        /// </summary>
        public string license_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string mac { get; set; }
    }
    /// <summary>
    /// 许可信息
    /// </summary>
    public class SenseShieldLicenseInfoJson
    {
        /// <summary>
        /// 
        /// </summary>
        public int license_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool enable { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string guid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int start_time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int end_time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int first_use_time { get; set; }
        /// <summary>
        /// 并发类型
        /// </summary>
        public string concurrent_type { get; set; }
        /// <summary>
        /// 并发数
        /// </summary>
        public int concurrent { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string module { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int last_update_timestamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int last_update_timesn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int bind_node { get; set; }
        /// <summary>
        /// (云锁不支持）
        /// </summary>
        public int rom_size { get; set; }
        /// <summary>
        /// (云锁不支持）
        /// </summary>
        public int raw_size { get; set; }
        /// <summary>
        /// (云锁不支持）
        /// </summary>
        public int pub_size { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string developer_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string user_guid { get; set; }
    }
    
    /// <summary> 
    /// 锁内文件列表
    /// </summary>
    public class SenseShieldFileJsonItem
    {
        /// <summary>
        /// 标志哪些域有效，不必关心
        /// </summary>
        public int validate { get; set; }
        /// <summary>
        /// (文件类型。二进制文件=0；可执行文件(evx)=1；密钥文件=2)
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// (文件访问权限，最大权限为 0xFF)
        /// </summary>
        public int privilege { get; set; }
        /// <summary>
        /// (文件大小，单位：字节)
        /// </summary>
        public int size { get; set; }
        /// <summary>
        /// (文件创建时间，UTC时间秒)
        /// </summary>
        public int time { get; set; }
        /// <summary>
        /// (文件名称)
        /// </summary>
        public string name { get; set; }
    }
    
    /// <summary>
    /// 所有拥有的锁的信息基类，（云、软、硬）锁
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

}