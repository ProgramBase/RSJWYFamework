using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using SenseShield;
using SLM_HANDLE_INDEX = System.UInt32;

namespace RSJWYFamework.Runtime.Senseshield
{
    /// <summary>
    /// 
    /// </summary>
    internal static class SenseshieldServerHelp
    {
        /// <summary>
        /// 开发者ID长度数组限制
        /// </summary>
        public const int DEVELOPER_ID_LENGTH = 8;
        /// <summary>
        /// 设备SN长度数组限制
        /// </summary>
        public const int DEVICE_SN_LENGTH = 16;
        /// <summary>
        /// 事件回调函数
        /// </summary>
        private static callback pfn;
        /// <summary>
        /// 加解密最大数组大小
        /// </summary>
        private const int MaxBlockSize = 1520;
        /// <summary>
        /// 缓冲区大小限制，实际要-1
        /// 因为我们需要确保长度增加到最近的16的倍数，而不是超过它。
        /// </summary>
        private const int Alignment = 16;

        /// <summary>
        /// 可申请的Virbox托管内存总大小为 256kb。
        /// </summary>
        private const UInt32 MemoryMax = 262144;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="apikey">开发者API密码</param>
        /// <returns></returns>
        public static bool Init(string apikey)
        {
            uint ret = 0;
            ST_INIT_PARAM initPram = new ST_INIT_PARAM();
            initPram.version =SSDefine.SLM_CALLBACK_VERSION02;
            initPram.flag = SSDefine.SLM_INIT_FLAG_NOTIFY;
            pfn = new callback(handle_service_msg);     // 响应回调通知只有在 slm_init 后 slm_cleanup 之前有效。
            initPram.pfn = pfn;

            // 指定开发者 API 密码，示例代码指定 Demo 开发者的 API 密码。
            // 注意：正式开发者运行测试前需要修改此值，可以从 Virbox 开发者网站获取 API 密码。

            initPram.password = Utility.Utility.ConvertHexStringToByteArray(apikey);
            ret = SlmRuntime.slm_init(ref initPram);
            if (ret == SSErrCode.SS_OK)
            {
                RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield,"Slm_Init初始化成功!");
                return true;
            }
            else if (ret == SSErrCode.SS_ERROR_DEVELOPER_PASSWORD)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"Slm_Init 失败:0x{ret:X8}(错误开发人员密码). 请登录 Virbox 开发者中心(https://developer.lm.virbox.com), 获取 API 密码，并替换 'initPram.password' 变量内容。");
                return false;
            }
            else
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"Slm_Init 失败:0x{ret:X8}");
                return false;
            }
        }
        /// <summary>
        /// 释放API内分配堆区域  
        /// </summary>
        /// <param name="buffer"></param>
        public static void SLMFree(IntPtr buffer)
        {
            uint ret = 0;
            SlmRuntime.slm_free(buffer);
        }
        
        /// <summary>
        /// 把错误代码转为string
        /// </summary>
        /// <param name="err">错误信息</param>
        /// <returns></returns>
        public static string GetErrFormat(uint err)
        {
            IntPtr  result;
            result = SlmRuntime.slm_error_format(err, SSDefine.LANGUAGE_ENGLISH_ASCII);
            if (result != IntPtr.Zero)
            {
                string error = Marshal.PtrToStringAnsi(result);
                return $"slm_error_format success, code = 0x{err:X8}, message = {error}";
            }
            RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield, $"slm_error_format Failure! 0x{err:X8}");
            return $"slm_error_format Failure! 0x{err:X8}";
        }
        
        #region 信息获取

        /// <summary>
        /// 获取指定设备下指定许可的全部信息 
        /// 获取到指定设备的 许可ID 列表，方便统计锁内许可总数 
        /// </summary>
        /// <param name="device">许可信息</param>
        /// <param name="id">许可id</param>
        /// <returns></returns>
        public static SenseShieldLicenseInfoJson GetDeviceAndLicenseIDAllInfo(SenseShieldLicenseJsonItem device,UInt32 id)
        {
            if (device == null)
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield, $"未传入许可信息");
            
            uint ret = 0;
            IntPtr license_info = IntPtr.Zero;
            string Info = JsonConvert.SerializeObject(device);
            ret = SlmRuntime.slm_get_license_info(Info, id, ref license_info);
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield, $"slm_get_license_info Failure! 0x{ret:X8}");
                return default;
            }
            string StrPrint = Marshal.PtrToStringAnsi(license_info);
            var ids = Utility.Utility.LoadJson<SenseShieldLicenseInfoJson>(StrPrint);
            return ids;
        }
        
        
        
        /// <summary>
        /// 获取到当前设备信息，通过设备信息获取许可信息，主要实现不用登录许可，便可查看许可内容的功能 
        /// 传入获取到的本地锁信息，获取其授权ID
        /// 返回的第一个是0号许可
        /// 详情：https://h.virbox.com/docs/virboxlm-intro/VirboLM-system/#0%E5%8F%B7%E8%AE%B8%E5%8F%AF
        /// </summary>
        /// <param name="device">锁信息</param>
        /// <returns>包含的许可id，第一个是0号许可</returns>
        public static UInt32[] GetLicenseId(SenseShieldLicenseJsonItem device)
        {
            if (device == null)
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield, $"未传入许可信息");
            IntPtr license_ids= IntPtr.Zero;
            uint ret = 0;
            string Info = JsonConvert.SerializeObject(device);
            ret = SlmRuntime.slm_enum_license_id(Info, ref license_ids);
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield, $"slm_enum_license_id Failure! 0x{ret:X8}"); 
                return default;
            }
            string StrPrint = Marshal.PtrToStringAnsi(license_ids);
            //RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield, $"slm_enum_license_id {StrPrint}");
            var ids = Utility.Utility.LoadJson<UInt32[]>(StrPrint);
            return ids;

        }
        
        /// <summary>
        /// 获取所有本地锁信息 （云、软、硬）锁
        /// </summary>
        /// <returns></returns>
        public static SenseShieldLicenseJsonItem[] GetAllDevice()
        {
            IntPtr device_info = IntPtr.Zero;
            uint ret = 0;

            ret = SlmRuntime.slm_enum_device(ref device_info);
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield, $"slm_enum_device Failure:0x{ret:X8}");
                return null;
            }
            string StrPrint = Marshal.PtrToStringAnsi(device_info);
            var _lis = Utility.Utility.LoadJson<SenseShieldLicenseJsonItem[]>(StrPrint);
            SlmRuntime.slm_free(device_info);
            return _lis;
        }

        /// <summary>
        /// 获得 Runtime 库 和 Virbox许可服务 的版本信息
        /// </summary>
        /// <returns></returns>
        public static (UInt32 api_version, UInt32 ss_version) GetVersion()
        {
            uint ret = 0;
            UInt32 api_version = 0;
            UInt32 ss_version = 0;
            ret = SlmRuntime.slm_get_version(ref api_version, ref ss_version);
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield, $"slm_get_version Failure:0x{ret:X8}");
                return (default, default);
            }
            return (api_version, ss_version);
        }

        /// <summary>
        /// 获取开发商ID（每一个sdk都是独有的）
        /// </summary>
        /// <returns></returns>
        public static string GetDeveloperId()
        {
            uint ret = 0;
            // slm_get_developer_id
            byte[] developer_id = new byte[DEVELOPER_ID_LENGTH];
            ret = SlmRuntime.slm_get_developer_id(developer_id);
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield, "SLM 获取开发人员 ID 失败:0x{ret:X8}");
                return string.Empty;
            }
            else
            {
                RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield, "SLM 获取开发人员 ID 成功!");
                // 将开发商ID转化为字符串
                return BitConverter.ToString(developer_id).Replace("-", "");
            }
        }
        
        /// <summary>
        /// 查找许可信息(仅对硬件锁有效？？)
        /// 仅对硬件锁有效似乎没有这个限制了
        /// </summary>
        /// <param name="license_id">许可ID</param>
        /// <returns>许可信息json</returns>
        public static List<SenseShieldLicenseJsonItem> FindLicense(UInt32 license_id=1)
        {
            uint ret = 0;
            IntPtr desc = IntPtr.Zero;
            ret = SlmRuntime.slm_find_license(license_id, INFO_FORMAT_TYPE.JSON, ref desc);
            
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"SLM 查找许可证 失败:0x{ret:X8}");
                return null;
            }
            else
            {
                string StrPrint = Marshal.PtrToStringAnsi(desc);
                // 去掉前后的方括号
                //StrPrint = StrPrint.Substring(1);
                //StrPrint = StrPrint.Substring(0, StrPrint.Length - 1);
                var _json = Utility.Utility.LoadJson<List<SenseShieldLicenseJsonItem>>(StrPrint);
                RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield,"SlmFindLicenseEasy Success!");
                SlmRuntime.slm_free(desc);
                if (ret != SSErrCode.SS_OK)
                {
                    RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"slm查找许可证后释放API堆区域失败 失败:0x{ret:X8}");
                }

                return _json;
            }
        }
        
          /// <summary>
        /// 获取会话信息
        /// </summary>
        /// <param name="Handle">许可句柄，通过登录获得</param>
        /// <returns></returns>
        public static SenseShieldSessionInfoJson GetInfoSessionInfo(SLM_HANDLE_INDEX Handle)
        {
            uint ret = 0;
            IntPtr desc = IntPtr.Zero;
            ret = SlmRuntime.slm_get_info(Handle,INFO_TYPE.SESSION_INFO,INFO_FORMAT_TYPE.JSON,ref desc);
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"SLM 获取信息(local_info) Failure:0x{ret:X8}");
                return null;
            }
            else
            {
                var _json = Utility.Utility.LoadJson<SenseShieldSessionInfoJson>(Marshal.PtrToStringAnsi(desc));
                RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield,"SLM 获取信息(local_info) Success!");
                SlmRuntime.slm_free(desc);
                if (ret != SSErrCode.SS_OK)
                {
                    RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"slm获取信息(local_info)释放API堆区域失败 失败:0x{ret:X8}");
                }
                return _json;
            }
        }
        /// <summary>
        /// 获取许可信息
        /// </summary>
        /// <param name="Handle">许可句柄，通过登录获得</param>
        /// <returns></returns>
        public static SenseShieldLicenseInfoJson GetInfoLicenseInfo(SLM_HANDLE_INDEX Handle)
        {
            uint ret = 0;
            IntPtr desc = IntPtr.Zero;
            ret = SlmRuntime.slm_get_info(Handle,INFO_TYPE.LICENSE_INFO,INFO_FORMAT_TYPE.JSON,ref desc);
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"SLM 获取信息(local_info) Failure:0x{ret:X8}");
                return null;
            }
            else
            {
                var _json = Utility.Utility.LoadJson<SenseShieldLicenseInfoJson>(Marshal.PtrToStringAnsi(desc));
                RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield,"SLM 获取信息(local_info) Success!");
                SlmRuntime.slm_free(desc);
                if (ret != SSErrCode.SS_OK)
                {
                    RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"slm获取信息(local_info)释放API堆区域失败 失败:0x{ret:X8}");
                }
                return _json;
            }
        }
        /// <summary>
        /// 获取硬件锁锁信息 
        /// </summary>
        /// <param name="Handle">许可句柄，通过登录获得</param>
        /// <returns></returns>
        public static SenseShieldLockInfoJson GetInfoLockInfo(SLM_HANDLE_INDEX Handle)
        {
            uint ret = 0;
            IntPtr desc = IntPtr.Zero;
            ret = SlmRuntime.slm_get_info(Handle,INFO_TYPE.LOCK_INFO,INFO_FORMAT_TYPE.JSON,ref desc);
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"SLM 获取信息(local_info) Failure:0x{ret:X8}");
                return null;
            }
            else
            {
                var _json = Utility.Utility.LoadJson<SenseShieldLockInfoJson>(Marshal.PtrToStringAnsi(desc));
                RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield,"SLM 获取信息(local_info) Success!");
                SlmRuntime.slm_free(desc);
                if (ret != SSErrCode.SS_OK)
                {
                    RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"slm获取信息(local_info)释放API堆区域失败 失败:0x{ret:X8}");
                }
                return _json;
            }
        }
        /// <summary>
        ///文件列表
        /// </summary>
        /// <param name="Handle">许可句柄，通过登录获得</param>
        /// <returns></returns>
        public static List<SenseShieldFileJsonItem> GetInfoFileList(SLM_HANDLE_INDEX Handle)
        {
            uint ret = 0;
            IntPtr desc = IntPtr.Zero;
            ret = SlmRuntime.slm_get_info(Handle,INFO_TYPE.FILE_LIST,INFO_FORMAT_TYPE.JSON,ref desc);
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"SLM 获取信息(local_info) Failure:0x{ret:X8}");
                return null;
            }
            else
            {
                string _a = Marshal.PtrToStringAnsi(desc);
                var _json = Utility.Utility.LoadJson<List<SenseShieldFileJsonItem>>(Marshal.PtrToStringAnsi(desc));
                RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield,"SLM 获取信息(local_info) Success!");
                SlmRuntime.slm_free(desc);
                if (ret != SSErrCode.SS_OK)
                {
                    RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"slm获取信息(local_info)释放API堆区域失败 失败:0x{ret:X8}");
                }
                return _json;
            }
        }

        #endregion

        #region virbox代托管内存

        /*
         Virbox许可服务 的托管内存原理是 APP 利用有效的许可作为凭证，在 Virbox许可服务 模块内数据加密且数据校验，其内存二进制数据没有明文， 并且无法非法修改,黑客极难查看与篡改使用。
         用户可以把自己 APP 的一些敏感数据保存到 Virbox许可服务 的托管内存，比如帐号口令，数据库的帐号与密码，涉及到操作权限的临时数据放到 Virbox许可服务 内存托管里面。
         另外一方面 APP 跟 Virbox许可服务 耦合度极大的提高，防止黑客脱离 Virbox许可服务 调试与运行。
         内存托管的好处：
         1.敏感数据内存不泄密、无法篡改。
         2.可以跨线程安全交互数据。
         3.APP软件、许可、Virbox许可服务 三者强耦合，软件防止被破解能力极高。（黑客需要手工剥离和重建才能使用软件）
         说明：托管内存每次申请的最大内存为 SLM_MEM_MAX_SIZE ，每个内存通过一个 mem_id 标识。可申请的内存总大小为 256kb。
        */
        
        /// <summary>
        /// 申请Virbox托管内存
        /// </summary>
        /// <param name="handle">登录的许可句柄</param>
        /// <param name="size">申请的内存大小，不得超过256KB，262144b</param>
        /// <returns>virbox返回的托管ID</returns>
        public static UInt32 ApplyForMemoryAlloc(SLM_HANDLE_INDEX handle, UInt32 size)
        {
            if (size > MemoryMax)
                throw new RSJWYException(RSJWYFameworkEnum.SenseShield, $"申请{size}超过最大{MemoryMax}限制大小");
            uint ret = 0;
            UInt32 mem_index = 0;
            ret = SlmRuntime.slm_mem_alloc(handle, 1024, ref mem_index);
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield, $"slm_mem_alloc Failure:0x{ret:X8}");
            }

            return mem_index;
        }

        /// <summary>
        /// 向Virbox托管内存写入数据
        /// </summary>
        /// <param name="handle">登录的许可句柄</param>
        /// <param name="mem_id">托管内存ID</param>
        /// <param name="offset">托管数据偏移（从内存哪一位置开始写入）</param>
        /// <param name="writebuff"></param>
        /// <returns>写入长度、是否成功</returns>
        public static (UInt32 writelen, bool success) MemoryWrite(SLM_HANDLE_INDEX handle, UInt32 mem_id, UInt32 offset,
            byte[] writebuff)
        {
            var writeLen = (UInt32)writebuff.Length;
            if (writeLen > MemoryMax)
                throw new RSJWYException(RSJWYFameworkEnum.SenseShield,
                    $"要写入的长度{(UInt32)writebuff.Length}超过托管内存最大大小{MemoryMax}，禁止写入");
            if (offset >= MemoryMax)
                throw new RSJWYException(RSJWYFameworkEnum.SenseShield, $"偏移{offset}超过托管内存最大大小{MemoryMax}，咋地，你要疯啊！！");


            uint ret = 0;
            UInt32 numberofbyteswritten = 0;
            ret = SlmRuntime.slm_mem_write(handle, mem_id, offset, writeLen, writebuff, ref numberofbyteswritten);
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield, $"slm_mem_write Failure:0x{ret:X8}");

                return (default, false);
            }

            return (numberofbyteswritten, true);
        }

        /// <summary>
        /// 从Virbox托管内中获取数据
        /// </summary>
        /// <param name="handle">登录的许可句柄</param>
        /// <param name="mem_id">托管内存ID</param>
        /// <param name="offset">读取托管数据偏移 </param>
        /// <param name="len">读取托管数据长度 </param>
        /// <returns>读取长度、读取的数组、读取成功？</returns>
        /// <exception cref="RSJWYException"></exception>
        public static (UInt32 readlen, byte[] readbuff, bool success) MemoryRead(SLM_HANDLE_INDEX handle, UInt32 mem_id,
            UInt32 offset, UInt32 len)
        {
            if (offset >= MemoryMax)
                throw new RSJWYException(RSJWYFameworkEnum.SenseShield, $"读取的位置{offset}超过托管内存最大大小{MemoryMax}，这，，读不到啊");

            uint ret = 0;
            UInt32 readlen = 0;
            byte[] mem_read_buf = new byte[len];

            ret = SlmRuntime.slm_mem_read(handle, mem_id, offset, len, mem_read_buf, ref readlen);
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield, $"slm_mem_write Failure:0x{ret:X8}");
                return (default, default, false);
            }

            return (readlen, mem_read_buf, true);
        }
        /// <summary>
        /// 释放Virbox托管内存
        /// </summary>
        /// <param name="handle">登录的许可句柄</param>
        /// <param name="mem_id">托管内存ID</param>
        /// <returns></returns>
        public static bool  MemoryFree(SLM_HANDLE_INDEX handle, UInt32 mem_id)
        {
            uint ret = 0;
            ret  = SlmRuntime.slm_mem_free(handle,mem_id);
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"slm_mem_free Failure:0x{ret:X8}");
                return false;
            }
            return true;
        }
        
        #endregion
        
        #region 数据区操作

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="handle">登录的许可句柄</param>
        /// <param name="type">操作区域</param>
        /// <param name="dataSize">数据区大小</param>
        /// <returns></returns>
        public static byte[] ReadUserData(SLM_HANDLE_INDEX handle,LIC_USER_DATA_TYPE type,UInt32 dataSize)
        {
            if (dataSize>65535 )
                throw new RSJWYException(RSJWYFameworkEnum.SenseShield, $"设置的数据区大小：{dataSize}b，超出最大可读64K规定限制");
            uint ret = 0;
            byte[] readbuf = new byte [dataSize];
            
            ret = SlmRuntime.slm_user_data_read(handle, type, readbuf, 0, dataSize);
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"slm_user_data_read Failure:0x{ret:X8}");
            }
            //ret = SlmRuntime.slm_user_data_read(Handle, LIC_USER_DATA_TYPE.RAW, readbuf, 0, datasize);
            return readbuf;
        }
        /// <summary>
        /// 写许可的读写数据区
        /// 仅读写区可以通过应用程序写入数据 
        /// </summary>
        /// <param name="handle">登录句柄</param>
        /// <param name="writeData">要写入的数据，数据长度不得大于读写区长度</param>
        /// <param name="offset">加密锁内数据区的偏移，即锁内数据区的写入位置 ，默认从0位写</param>
        /// <returns></returns>
        public static bool WriteUserDataRaw(SLM_HANDLE_INDEX handle,byte[] writeData,UInt32 offset=0)
        {
            var writeLen = (UInt32)writeData.Length;
            var rawLen = GetUserDtatSize(handle, LIC_USER_DATA_TYPE.RAW).size;
            if (writeLen >rawLen )
                throw new RSJWYException(RSJWYFameworkEnum.SenseShield,$"要写入的长度{(UInt32)writeData.Length}超过读写区最大长度{rawLen}，禁止写入");
            if (offset >=rawLen )
                throw new RSJWYException(RSJWYFameworkEnum.SenseShield,$"偏移{offset}超过读写区最大长度{rawLen}，咋地，你要疯啊！！");
            uint ret = 0;
            ret = SlmRuntime.slm_user_data_write(handle, writeData, offset, writeLen);
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"slm_user_data_write Failure:0x{ret:X8}");
                return false;
            }
            //ret = SlmRuntime.slm_user_data_read(Handle, LIC_USER_DATA_TYPE.RAW, readbuf, 0, datasize);
            return true;
        }
     
        /// <summary>
        /// 获得许可的用户数据区大小 
        /// </summary>
        /// <param name="handle">许可句柄值</param>
        /// <param name="type">用户数据区类型</param>
        /// <returns></returns>
        public static (UInt32 size,bool success) GetUserDtatSize(SLM_HANDLE_INDEX handle,LIC_USER_DATA_TYPE type)
        {
            uint ret = 0;
            UInt32 dataSize=0;
            ret = SlmRuntime.slm_user_data_getsize(handle, type, ref dataSize);
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"slm_user_data_getsize Failure:0x{ret:X8}");
                return (default(UInt32),false);
            }
            return (dataSize,true);
        }
        

        #endregion

        #region 通过许可加密解密

        /// <summary>
        /// 通过许可内的AES密钥进行加密
        /// 加密方式采用 AES对称加密，密钥在加密锁内（指硬件锁、云锁、软锁，下同）生成，
        /// 且没有任何机会能出锁，在保证效率的同时，也最大化的加强了安全性。
        /// </summary>
        /// <param name="Handle">login许可登录后的句柄</param>
        /// <param name="data">要加密的数据</param>
        /// <returns>加密后的数组，前面会增加16位字节，记录原始数据长度，后面会增加到16位对齐</returns>
        public static byte[] Encrypt(SLM_HANDLE_INDEX Handle,byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new RSJWYException(RSJWYFameworkEnum.SenseShield, $"传入的要加密的数据无效");
            if (Handle == default(SLM_HANDLE_INDEX))
                throw new RSJWYException(RSJWYFameworkEnum.SenseShield, $"许可登录后的内存句柄无效");
       
            //计算最近的16字节对齐大小
            int aligningLength = (((data.Length + Alignment-1)/16)+1)*16;
            aligningLength += Alignment; //最前面两位数组存储加密前大小
            //存储原数据长度
            byte[] encryptData = new byte[aligningLength];
            byte[] uint32Bytes = BitConverter.GetBytes((uint)data.Length);
            // 将uint32字节复制到16字节数组的开始位置
            Array.Copy(uint32Bytes, 0, encryptData, 0, uint32Bytes.Length);
            //复制原数据到整个数组
            Array.Copy(data, 0, encryptData, 16, data.Length);
            //二次确认是否已对齐，后续所有操作都是在对齐的情况下进行
            if (encryptData.Length <= 0 || encryptData.Length % 16 != 0) 
                throw new RSJWYException(RSJWYFameworkEnum.SenseShield, $"没有正确对齐");
            //分割数据为二维数组，并开始加密
            //记录要分割的份数
            int totalBlocks = (encryptData.Length + MaxBlockSize - 1) / MaxBlockSize;
            
            //取出数据并分割后存储数组
            byte[][] encryptArr = new byte[totalBlocks][]; 
            //加密后数组
            byte[][] afterEncryptArr = new byte[totalBlocks][]; 
            uint ret = 0;
            
            for (int i = 0; i < totalBlocks; i++)
            {
                // 计算当前块的大小，可能是小于最大块大小的最后一个块
                int blockSize = Math.Min(MaxBlockSize, encryptData.Length - (i * MaxBlockSize));
                // 创建一个新的数组来存储当前块的数据
                encryptArr[i] = new byte[blockSize];
                //加密后的数组
                afterEncryptArr[i] = new byte[blockSize];
                // 从原始数组中复制数据到当前块
                Array.Copy(encryptData, i * MaxBlockSize, encryptArr[i], 0, blockSize);
                //加密
                ret = SlmRuntime.slm_encrypt(Handle,encryptArr[i],afterEncryptArr[i],(uint)encryptArr[i].Length);
                if (ret != SSErrCode.SS_OK)
                {
                    //只要有一轮次加密失败，即返回异常
                    RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"slm_encrypt失败:0x{ret:X8}");
                    return null;
                }
                /*else
                {
                    RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield, "slm_encrypt成功！");
                }*/
            }
            //完成整体加密，组合
            
            return Utility.Utility.ConvertJaggedArrayToOneDimensional(afterEncryptArr);
        }
        /// <summary>
        /// 通过许可内的AES密钥进行解密
        /// 加密方式采用 AES对称解密，密钥在加密锁内（指硬件锁、云锁、软锁，下同）生成，
        /// 且没有任何机会能出锁，在保证效率的同时，也最大化的加强了安全性。
        /// </summary>
        /// <param name="Handle">login许可登录后的句柄</param>
        /// <param name="data">要加密的数据</param>
        /// <returns>解密后的数组，会将前面的16位记录原始数据长度的字节移除以及对齐的内容也将会移除</returns>
        public static byte[] Decrypt(SLM_HANDLE_INDEX Handle,byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new RSJWYException(RSJWYFameworkEnum.SenseShield, $"传入的要解密的数据无效");
            if (Handle == default(SLM_HANDLE_INDEX))
                throw new RSJWYException(RSJWYFameworkEnum.SenseShield, $"许可登录后的内存句柄无效");
            
            //拆分数组，提交解密流程
            int totalBlocks = (data.Length + MaxBlockSize - 1) / MaxBlockSize;
            //取出数据并分割后存储数组
            byte[][] decryptArr = new byte[totalBlocks][]; 
            //解密后数组
            byte[][] afterdecryptArr = new byte[totalBlocks][]; 
            uint ret = 0;
            
            for (int i = 0; i < totalBlocks; i++)
            {
                // 计算当前块的大小，可能是小于最大块大小的最后一个块
                int blockSize = Math.Min(MaxBlockSize, data.Length - (i * MaxBlockSize));
                // 创建一个新的数组来存储当前块的数据
                decryptArr[i] = new byte[blockSize];
                //加密后的数组
                afterdecryptArr[i] = new byte[blockSize];
                // 从原始数组中复制数据到当前块
                Array.Copy(data, i * MaxBlockSize, decryptArr[i], 0, blockSize);
                //加密
                ret = SlmRuntime.slm_decrypt(Handle,decryptArr[i],afterdecryptArr[i],(uint)decryptArr[i].Length);
                if (ret != SSErrCode.SS_OK)
                {
                    //只要有一轮次解密失败，即返回异常
                    RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"slm_decrypt失败:0x{ret:X8}");
                    return null;
                }
                //完成整体加密，组合
            }
            //处理数组，把记录原始长度的和位数的移除
            var decrypData= Utility.Utility.ConvertJaggedArrayToOneDimensional(afterdecryptArr);
            var Originallength = BitConverter.ToUInt32(decrypData, 0);
            byte[] decryp = new byte[Originallength];
            Array.Copy(decrypData,16,decryp,0,Originallength);
            return decryp;

        }
        #endregion
        
        #region 登录登出维持许可

        /// <summary>
        /// 安全登录许可
        /// ,并且检查时间（是否到期或者是否早于开始时间）、次数、并发数是否归零，
        /// 如果有计数器，则永久性减少对应的计数器，对于网络锁则临时增加网络并发计数器。
        /// </summary>
        /// <param name="license_id">登陆的许可ID</param>
        public static (bool loginSuccess,SLM_HANDLE_INDEX Handle) LoginSS(uint license_id=1)
        {
            SLM_HANDLE_INDEX Handle = 0;
            uint ret = 0;
            IntPtr desc = IntPtr.Zero;
            IntPtr a = IntPtr.Zero;
            //03. LOGIN
            ST_LOGIN_PARAM stLogin = new ST_LOGIN_PARAM();
            stLogin.size = (UInt32)Marshal.SizeOf(stLogin);
            // 指定登录的许可ID，Demo 设置登录0号许可，开发者正式使用时可根据需要调整此参数。
            stLogin.license_id = license_id;

            // 指定登录许可ID的容器，Demo 设置为本地加密锁，开发者可根据需要调整此参数。
            stLogin.login_mode = SSDefine.SLM_LOGIN_MODE_SLOCK;

            ret = SlmRuntime.slm_login(ref stLogin, INFO_FORMAT_TYPE.STRUCT, ref Handle, a);
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield, $"SLM 登录 失败:0x{ret:X8}");
                return (false,Handle);
            }
            else
            {
                RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield, $"Slmlogin 成功!:0x{ret:X8}");
                return (true,Handle);
            }
        }
        /// <summary>
        /// 保持登录会话心跳，避免变为“僵尸句柄”。
        /// 需要配合登陆登出使用
        /// </summary>
        /// <param name="Handle">许可句柄，通过登录获得</param>
        /// <returns></returns>
        public static bool KeepAlive(SLM_HANDLE_INDEX Handle)
        {
            uint ret = 0;
            ret = SlmRuntime.slm_keep_alive(Handle);
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"SlmKeepAliveEasy 失败:0x{ret:X8}");
                //System.Diagnostics.Debug.Assert(true);
                return false;
            }
            else
            {
               // WriteLineGreen("SlmKeepAlive轻松成功!");
                return true;
            } 
        }
        /// <summary>
        /// 许可登出，并且释放许可句柄等资源 
        /// </summary>
        /// <param name="Handle">许可句柄值</param>
        public static void  LoginOutSS(SLM_HANDLE_INDEX Handle)
        {
            if (Handle <= 0) return;
            uint ret = 0;
            ret = SlmRuntime.slm_logout(Handle);
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"slm_logout Failure0x{ret:X8}");
            }
        }
        /// <summary>
        /// 是非线程安全的，此函数不建议开发者调用，
        /// 因为程序退出时系统会自动回收没有释放的内存，
        /// 若开发者调用，为了保证多线程调用 Runtime API 的安全性，此函数建议在程序退出时调用。
        /// 一旦调用了此函数，以上所有API（除 slm_init ）均不可使用。 
        /// </summary>
        public static void CleanUp()
        {
            uint ret = 0;
            if (ret != SSErrCode.SS_OK)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"slm_cleanup Failure0x{ret:X8}");
            }
            else
            {
                RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield,$"slm_cleanup Success!，已完成反向初始化");
            }
        }

        #endregion

        #region 证书工具
        
        
        static UInt32 device_cert_verify(SLM_HANDLE_INDEX slm_handle)
        {
            UInt32 sts = SSErrCode.SS_OK;
            UInt32 retsize = 0;
            byte[] device_cert = new byte[2048];   //设备证书

            UInt32 i = 0;
            byte[] cert_pub_key = new byte[512];
            byte[] S5_pub_key = new byte[512];
            byte[] cert_sn = new byte[256];
            byte[] Subject = new byte[256];
            int subject_len = Subject.Length;
            const int SIGN_SIZE = 32 + 9;
            byte[] sign_buffer = new byte[SIGN_SIZE];
            byte[] signature_buff = new byte[256];
            //===========证书验证签名
            //读取设备证书
            sts = SlmRuntime.slm_get_device_cert(slm_handle,device_cert, (uint)device_cert.Length, ref retsize);
            if (SSErrCode.SS_OK == sts)
            {
                RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield,$"[OK],slm_get_device_cert,cert dump:\n");
                //hexdump(device_cert,retsize);
            }
            else
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"[ERROR],slm_get_device_cert,ret=0x{sts:8X},description: {SSDefine.LANGUAGE_CHINESE_ASCII}\n");
                return 0xffff;
            }

            //签名格式 SENSELOCK+32字节随机数
            byte[] signdata = Encoding.UTF8.GetBytes("SENSELOCK");
            //strcpy((char*)sign_buffer, "SENSELOCK");
            //sp_rands(&(sign_buffer[9]),sizeof(sign_buffer)-9);
            for (i = 0; i < 9; i++)
            {
                sign_buffer[i] = signdata[i];
            }
            Random rand_byte = new Random();
            byte n;
            for (i = 0; i < 32; i++)
            {
                n = (byte)rand_byte.Next(255);
                sign_buffer[9 + i] = (byte)(n % 256);
            }

            sts = SlmRuntime.slm_sign_by_device(slm_handle, sign_buffer, (uint)sign_buffer.Length, 
                signature_buff, (uint)signature_buff.Length,ref retsize);
            bool bresult = verifyDevice(sign_buffer, signature_buff, device_cert);
            if (bresult)
            {
                RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield,$"RSA verifyDevice OK");
            }
            else
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"RSA verifyDevice failed");
            }
            return sts;
        }

        //device_cert_verify证书验证签名函数，实现签名和验证签名
        /*设备证书签名*/
        static bool verifyDevice(byte[] signdata, byte[] signature, byte[] certs_byte)
        {
            bool result = false;
            //byte[] certs_byte = cert;
            X509Certificate2Collection Collection = new X509Certificate2Collection();
            Collection.Import(certs_byte);
            RSACryptoServiceProvider rsa;

            for (int i = 0; i < Collection.Count; i++)
            {
                if (Collection[i].Subject.StartsWith("CN=DEVICEID"))
                {
                    rsa = (RSACryptoServiceProvider)Collection[i].PublicKey.Key;
                    result = rsa.VerifyData(signdata, SHA1.Create(), signature);
                    break;
                }
            }

            return result;
        }
        static  uint calc_integerHash(uint input)
        {
            uint h = input;
            h ^= h >> 16;
            h *= 0x85ebca6b;
            h ^= h >> 13;
            h *= 0xc2b2ae35;
            h ^= h >> 16;

            return h;
        }

        static uint execute_hash_code(SLM_HANDLE_INDEX slm_handle)
        {
            const int MAX_BUFFER_SIZE = 1702;
            UInt32 ret = SSErrCode.SS_OK;
            UInt32 retlen = 0;

            uint input;
            uint [] output= new uint[1];
            uint[] a = new uint[4];
            const uint salt = 0x12345678;
            byte[] inbuff = new byte[MAX_BUFFER_SIZE];
            byte[] outbuff = new byte[MAX_BUFFER_SIZE];
            input = output[0] = 0;
            a[0] = a[1] = a[2] = a[3] = 0;
            Random rand_data = new Random();

            a[0] = (uint)rand_data.Next(2147483647);
            a[1] = (uint)rand_data.Next(2147483647);
            a[2] = (uint)rand_data.Next(2147483647);
            a[3] = (uint)rand_data.Next(2147483647);
            //
            //memcpy(&inbuff[0], &a, sizeof(a));
            Buffer.BlockCopy(a, 0, inbuff, 0, 1 * sizeof(uint));
            //memcpy(&inbuff[4], &b, sizeof(a));
            Buffer.BlockCopy(a, 4, inbuff, 4, 1 * sizeof(uint));
            //memcpy(&inbuff[8], &c, sizeof(a));
            Buffer.BlockCopy(a, 8, inbuff, 8, 1 * sizeof(uint));
            //memcpy(&inbuff[12], &d, sizeof(a));
            Buffer.BlockCopy(a, 12, inbuff, 12, 1 * sizeof(uint));

            ret = SlmRuntime.slm_execute_static(slm_handle, "h5safe.evx",
                inbuff, 4 * 4, outbuff, MAX_BUFFER_SIZE, ref retlen);

            //memcpy(&output, outbuff, sizeof(output));
            Buffer.BlockCopy(outbuff, 0, output, 0, 4);
            // input = ( ((a % b) & c) | d ) ^ salt;
            input = (((a[0] % a[1]) & a[2]) | a[3]) ^ salt;
            input = calc_integerHash(input);

            if (input != output[0])
            {
                Console.WriteLine("锁内代码校验失败！{0},{1}", input,output[0]);
                ret = 0xffff;
            }
            else
            {
                Console.WriteLine("锁内代码校验成功！{0}",input);
            }
            return ret;
        }

        #endregion
       
        /// <summary>
        /// 回调函数信息提示
        /// </summary>
        /// <param name="message"></param>
        /// <param name="wparam"></param>
        /// <param name="lparam"></param>
        /// <returns></returns>
        public static uint handle_service_msg(uint message, UIntPtr wparam, UIntPtr lparam)
        {
            uint ret = SSErrCode.SS_OK;
            string StrMsg = string.Empty;
            char[] szmsg = new char[1024];
            byte[] lock_sn_bytes = new byte[DEVICE_SN_LENGTH];
            string lock_sn = "";

            switch (message)
            {
                case SSDefine.SS_ANTI_INFORMATION:   // 信息提示
                    RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"SS_ANTI_INFORMATION is:0x{0:message} wparam is {wparam}");
                    break;
                case SSDefine.SS_ANTI_WARNING:       // 警告
                    // 反调试检查。一旦发现如下消息，建议立即停止程序正常业务，防止程序被黑客调试。
                    switch ((uint)(wparam))
                    {
                        case SSDefine.SS_ANTI_PATCH_INJECT:
                            RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"信息类型=:0x{message:X8} 具体错误码= 0x{wparam:X8}，攻击行为：注入");
                            break;
                        case SSDefine.SS_ANTI_MODULE_INVALID:
                            RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"信息类型=:0x{message:X8} 具体错误码= 0x{wparam:X8}，攻击行为：非法模块DLL");
                            break;
                        case SSDefine.SS_ANTI_ATTACH_FOUND:
                            RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"信息类型=:0x{message:X8} 具体错误码= 0x{wparam:X8}，攻击行为：附加调试");                            
                            break;
                        case SSDefine.SS_ANTI_THREAD_INVALID:
                             RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"信息类型=:0x{message:X8} 具体错误码= 0x{wparam:X8}，攻击行为：线程非法");                      
                            break;
                        case SSDefine.SS_ANTI_THREAD_ERROR:
                             RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"信息类型=:0x{message:X8} 具体错误码= 0x{wparam:X8}，攻击行为：线程错误"); 
                            break;
                        case SSDefine.SS_ANTI_CRC_ERROR:
                             RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"信息类型=:0x{message:X8} 具体错误码= 0x{wparam:X8}，攻击行为：内存模块 CRC 校验");
                            break;
                        case SSDefine.SS_ANTI_DEBUGGER_FOUND:
                             RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"信息类型=:0x{message:X8} 具体错误码= 0x{wparam:X8}，攻击行为：发现调试器");
                            break;
                        default:
                             RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"信息类型=:0x{message:X8} 具体错误码= 0x{wparam:X8}，攻击行为：其他未知错误");
                            break;
                    }
                    break;
                case SSDefine.SS_ANTI_EXCEPTION:         // 异常
                    RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"SS_ANTI_EXCEPTION is:0x{0:message} wparam is {wparam}");
                    break;
                case SSDefine.SS_ANTI_IDLE:              // 暂保留
                    RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"SS_ANTI_IDLE is:0x{0:message} wparam is {wparam}");
                    break;
                case SSDefine.SS_MSG_SERVICE_START:      // 服务启动
                    RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield,$"SS_MSG_SERVICE_START is:0x{0:message} wparam is {wparam}");
                    break;
                case SSDefine.SS_MSG_SERVICE_STOP:       // 服务停止
                    RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"SS_MSG_SERVICE_STOP is:0x{0:message} wparam is {wparam}");
                    break;
                case SSDefine.SS_MSG_LOCK_AVAILABLE:     // 锁可用（插入锁或SS启动时锁已初始化完成），wparam 代表锁号
                    // 锁插入消息，可以根据锁号查询锁内许可信息，实现自动登录软件等功能。
                    Marshal.Copy((IntPtr)(long)wparam, lock_sn_bytes, 0, DEVICE_SN_LENGTH);
                    lock_sn = BitConverter.ToString(lock_sn_bytes).Replace("-", "");
                    RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield,$"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{lock_sn:x8}锁插入");
                    break;
                case SSDefine.SS_MSG_LOCK_UNAVAILABLE:   // 锁无效（锁已拔出），wparam 代表锁号
                    // 锁拔出消息，对于只使用锁的应用程序，一旦加密锁拔出软件将无法继续使用，建议发现此消息提示用户保存数据，程序功能锁定等操作。
                    Marshal.Copy((IntPtr)(long)wparam, lock_sn_bytes, 0, DEVICE_SN_LENGTH);
                    lock_sn = BitConverter.ToString(lock_sn_bytes).Replace("-", "");
                    RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{lock_sn:x8}锁拔出");
                    break;
            }
            // 输出格式化后的消息内容
            //printf("%s\n", szmsg);
            return ret;
        }
        
     
        
        /// <summary>
        /// 密钥转16进值
        /// </summary>
        /// <param name="HexString"></param>
        /// <returns></returns>
        public static byte[] StringToHex(string HexString)
        {
            byte[] returnBytes = new byte[HexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(HexString.Substring(i * 2, 2), 16);

            return returnBytes;
        }
        /// <summary>
        /// 16进值
        /// </summary>
        /// <param name="buf"></param>
        public static void hexWriteLine(byte[] buf)
        {
            int i =0;
        
            for (i = 0; i < buf.Length; i++)
            {
                Console.Write("{0:X2} ", buf[i]);
                if (i % 16 == 15)
                {
                    Console.WriteLine();
                }

            }
            Console.WriteLine();
            return;
        }


        /** HEXDUMP函数 */
        /*
        hex_view = 4096 bytes
        offset 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F 
        0000 | ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? | ................
        0001 | ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? | ................
        0002 | ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? | ................
        0003 | ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? | ................
        0004 | ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? | ................
        0005 | ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? | ................
        0006 | ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? | ................
        */
        public static void hexall(byte[] buff)
        {
            int i = 0, j = 0;
            int cur = 0;
            int linemax = 0;
            int nprinted = 0;
            Boolean flag = false;
            int len = buff.Length;
            Char[] chars;
            if (0 == len)
            {
                return;
            }
            //UTF8Encoding utf8 = new UTF8Encoding();
            ASCIIEncoding asiic = new ASCIIEncoding();
            int charCount = asiic.GetCharCount(buff);
            chars = new Char[charCount];
            int charsDecodedCount = asiic.GetChars(buff, 0, len, chars, 0);

            Console.Write("hex_view = {0:d} bytes\r\noffset 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F", len); Console.WriteLine();
            i = 0; j = 0; flag = true;
            do
            {
                Console.Write("{0:X4} | ", (nprinted / 16));
                if (nprinted >= len)
                {
                    flag = false;
                    break;
                }
                linemax = 16;
                for (j = 0; j < linemax; j++)
                {
                    cur = i + j;
                    if (cur >= len)
                    {
                        flag = false;
                        Console.Write("   ");
                    }
                    else
                    {
                        Console.Write("{0:X2} ", buff[cur]);
                        nprinted++;
                    }
                }
                Console.Write("| ");
                for (j = 0; j < linemax; j++)
                {
                    cur = i + j;
                    if (cur >= len)
                    {
                        flag = false;
                        break;
                    }
                    if (buff[cur] > 30 && buff[cur] < 127)
                    { //Console.Write("{0:c}", buff[cur]);

                        Console.Write("{0}", chars[cur]);
                        //Console.Write(buff[cur].ToString);
                    }
                    else
                    { Console.Write("."); }
                }
                i += 16;
                Console.WriteLine();
            } while (flag);
            Console.WriteLine();
            return;
        }

    }
}