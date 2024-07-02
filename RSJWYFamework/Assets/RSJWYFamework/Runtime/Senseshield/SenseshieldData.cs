using System;

namespace RSJWYFamework.Runtime.Senseshield
{
    #region SenseShieldLicenseJson
    
    /// <summary>
    /// 许可信息
    /// </summary>
    public class SenseShieldLicenseJson
    {
        /// <summary>
        /// 
        /// </summary>
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
    /// 本地锁信息
    /// </summary>
    public class SenseShieldLockInfoJson
    {
        
    }
    /// <summary>
    /// 锁内文件列表
    /// </summary>
    public class SenseShieldFileListJson
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
    /// 锁内文件列表
    /// </summary>
    public class SenseShieldFileListArrJson
    {
        public SenseShieldFileListJson[] Files { get; set; }
    }
    #endregion


}