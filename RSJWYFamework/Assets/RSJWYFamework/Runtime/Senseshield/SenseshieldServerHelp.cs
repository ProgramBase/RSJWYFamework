using System;
using System.Runtime.InteropServices;
using System.Text;
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
        public const int DEVELOPER_ID_LENGTH = 8;
        public const int DEVICE_SN_LENGTH = 16;
        private static callback pfn;
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
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,"SLM 获取开发人员 ID 失败:0x{ret:X8}");
                return string.Empty;
            }
            else
            {
                RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield,"SLM 获取开发人员 ID 成功!");
                // 将开发商ID转化为字符串
                return BitConverter.ToString(developer_id).Replace("-", "");
            }
        }
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
        /// 查找许可信息(仅对硬件锁有效？？)
        /// 仅对硬件锁有效似乎没有这个限制了
        /// </summary>
        /// <param name="license_id">许可ID</param>
        /// <returns>许可信息json</returns>
        public static SenseShieldLicenseJson FindLicense(UInt32 license_id=1)
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
                StrPrint = StrPrint.Substring(1);
                StrPrint = StrPrint.Substring(0, StrPrint.Length - 1);
                var _json = Utility.Utility.LoadJson<SenseShieldLicenseJson>(StrPrint);
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
        public static SenseShieldFileListArrJson GetInfoFileList(SLM_HANDLE_INDEX Handle)
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
                var _json = Utility.Utility.LoadJson<SenseShieldFileListArrJson>(Marshal.PtrToStringAnsi(desc));
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
                    StrMsg = string.Format("SS_ANTI_INFORMATION is:0x{0:X8} wparam is %p", message, wparam);
                    WriteLineRed(StrMsg);
                    break;
                case SSDefine.SS_ANTI_WARNING:       // 警告
                    // 反调试检查。一旦发现如下消息，建议立即停止程序正常业务，防止程序被黑客调试。

                    switch ((uint)(wparam))
                    {
                        case SSDefine.SS_ANTI_PATCH_INJECT:
                            StrMsg = string.Format("信息类型=:0x{0:X8} 具体错误码= 0x{0:X8}", "注入", message, wparam);
                             WriteLineRed(StrMsg);
                            break;
                        case SSDefine.SS_ANTI_MODULE_INVALID:
                            StrMsg = string.Format("信息类型=:0x{0:X8} 具体错误码= 0x{0:X8}", "非法模块DLL", message, wparam);
                             WriteLineRed(StrMsg);
                            break;
                        case SSDefine.SS_ANTI_ATTACH_FOUND:
                            StrMsg = string.Format("信息类型=:0x{0:X8} 具体错误码= 0x{0:X8}", "附加调试", message, wparam);
                             WriteLineRed(StrMsg);
                            break;
                        case SSDefine.SS_ANTI_THREAD_INVALID:
                             StrMsg = string.Format("信息类型=:0x{0:X8} 具体错误码= 0x{0:X8}", "线程非法", message, wparam);
                             WriteLineRed(StrMsg);
                            break;
                        case SSDefine.SS_ANTI_THREAD_ERROR:
                             StrMsg = string.Format("信息类型=:0x{0:X8} 具体错误码= 0x{0:X8}", "线程错误", message, wparam);
                             WriteLineRed(StrMsg);
                            break;
                        case SSDefine.SS_ANTI_CRC_ERROR:
                             StrMsg = string.Format("信息类型=:0x{0:X8} 具体错误码= 0x{0:X8}", "内存模块 CRC 校验", message, wparam);
                             WriteLineRed(StrMsg); 
                            break;
                        case SSDefine.SS_ANTI_DEBUGGER_FOUND:
                             StrMsg = string.Format("信息类型=:0x{0:X8} 具体错误码= 0x{0:X8}", "发现调试器", message, wparam);
                             WriteLineRed(StrMsg); 
                            break;
                        default:
                             StrMsg = string.Format("信息类型=:0x{0:X8} 具体错误码= 0x{0:X8}", "其他未知错误", message, wparam);
                             WriteLineRed(StrMsg);
                            break;
                    }
                    break;
                case SSDefine.SS_ANTI_EXCEPTION:         // 异常
                    StrMsg = string.Format("SS_ANTI_EXCEPTION is :0x{0:X8} wparam is %p", message, wparam);
                      WriteLineRed(StrMsg);;
                    break;
                case SSDefine.SS_ANTI_IDLE:              // 暂保留
                    StrMsg = string.Format("SS_ANTI_IDLE is :0x{0:X8} wparam is %p", message, wparam);
                      WriteLineRed(StrMsg); 
                    break;
                case SSDefine.SS_MSG_SERVICE_START:      // 服务启动
                    StrMsg = string.Format("SS_MSG_SERVICE_START is :0x{0:X8} wparam is %p", message, wparam);
                      WriteLineRed(StrMsg);
                    break;
                case SSDefine.SS_MSG_SERVICE_STOP:       // 服务停止
                    StrMsg = string.Format("SS_MSG_SERVICE_STOP is :0x{0:X8} wparam is %p", message, wparam);
                      WriteLineRed(StrMsg);
                    break;
                case SSDefine.SS_MSG_LOCK_AVAILABLE:     // 锁可用（插入锁或SS启动时锁已初始化完成），wparam 代表锁号
                    // 锁插入消息，可以根据锁号查询锁内许可信息，实现自动登录软件等功能。
                    Marshal.Copy((IntPtr)(long)wparam, lock_sn_bytes, 0, DEVICE_SN_LENGTH);
                    lock_sn = BitConverter.ToString(lock_sn_bytes).Replace("-", "");

                    StrMsg = string.Format("{0},{1:x8}锁插入", DateTime.Now.ToString(), lock_sn);
                    WriteLineGreen(StrMsg);
                    break;
                case SSDefine.SS_MSG_LOCK_UNAVAILABLE:   // 锁无效（锁已拔出），wparam 代表锁号
                    // 锁拔出消息，对于只使用锁的应用程序，一旦加密锁拔出软件将无法继续使用，建议发现此消息提示用户保存数据，程序功能锁定等操作。
                    Marshal.Copy((IntPtr)(long)wparam, lock_sn_bytes, 0, DEVICE_SN_LENGTH);
                    lock_sn = BitConverter.ToString(lock_sn_bytes).Replace("-", "");

                    StrMsg = string.Format("{0},{1:x8}锁拔出", DateTime.Now.ToString(), lock_sn);
                    WriteLineRed(StrMsg);
                    break;
            }
            // 输出格式化后的消息内容
            //printf("%s\n", szmsg);
            return ret;
        }
     
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
      
        public static void WriteLineGreen(string s)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(s);
            Console.ResetColor();
        }
        public static void WriteLineRed(string s)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(s);
            Console.ResetColor();
        }
        public static void WriteLineYellow(string s)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
        }
        public static void WriteLineBlue(string s)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(s);
            Console.ResetColor();
        }
        public static byte[] StringToHex(string HexString)
        {
            byte[] returnBytes = new byte[HexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(HexString.Substring(i * 2, 2), 16);

            return returnBytes;
        }
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