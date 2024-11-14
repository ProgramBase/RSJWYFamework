using System.Threading;
using RSJWYFamework.Runtime.Config;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Module;
using RSJWYFamework.Runtime.Senseshield;
using UnityEngine;
using SLM_HANDLE_INDEX = System.UInt32;

namespace RSJWYFamework.Runtime.Default

{
    /// <summary>
    /// 深信服Virbox加密服务
    /// </summary>
    public class DefaultSenseshieldManager:IModule,ILife
    {
        /// <summary>
        /// 登录的会话ID
        /// </summary>
        private SLM_HANDLE_INDEX Handle=0;
        // 回调事件需要定义为类成员变量，若定义在函数中，当函数执行完毕时对象被回收，当收到服务回调通知时程序会异常终止。
        // 回调通知生效的时间周期：slm_init - slm_cleanup 期间，当执行清理操作后回调函数将无法收到任何服务通知消息。
        private static callback pfn;

        /// <summary>
        /// 维持句柄
        /// </summary>
        private Thread _keepAlive;
        
        /// <summary>
        /// 通知多线程自己跳出
        /// </summary>
        static bool isThreadOver = false;
        /// <summary>
        /// 是否初始化
        /// </summary>
        public bool isInit { get; private set; }
        /// <summary>
        /// 是否登录
        /// </summary>
        public bool isLogin { get; private set; }

        private ProjectConfig pc;
        
        public void Start()
        {
            if (!isInit) return;
            var _loginHandle =SenseshieldServerHelp.LoginSS((uint)pc.licenseID);
            isLogin = _loginHandle.loginSuccess;
            if (_loginHandle.loginSuccess)
            {
                Handle = _loginHandle.Handle;
            }
        }

        public void Init()
        {
            var pc =  Resources.Load<ProjectConfig>("ProjectConfig");
            isInit=SenseshieldServerHelp.Init(pc.developerPW);
            if (!isInit)
            {
                RSJWYLogger.Error(RSJWYFameworkEnum.SenseShield,"初始化Virbox失败");
            }
            else
            {
                RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield,$"本机授权许可总数为：{SenseshieldServerHelp.GetAllDevice().Length}");
            }
        }

        public void Close()
        {
            if (isInit)
            {
                isThreadOver = true;
                if (Handle>0)
                {
                    SenseshieldServerHelp.LoginOutSS(Handle);
                }
                SenseshieldServerHelp.CleanUp();
                isLogin = false;
            }
            
        }

        public void Update(float time, float deltaTime)
        {
            
        }

        public void UpdatePerSecond(float time)
        {
            if (isInit&&isLogin&&Handle!=0)
            {
                SenseshieldServerHelp.KeepAlive(Handle);
            }
        }

        public void FixedUpdate()
        {
        }

        public void LateUpdate()
        {
        }

        public uint Priority()
        {
            return 100;
        }
    }
       /*
        //main方法，测试主程序
        //官方示例代码流程
        static void Main(string[] args)
        {
            uint ret = 0;
            SLM_HANDLE_INDEX Handle = 0;
            
            string StrMsg = string.Empty;
            IntPtr a = IntPtr.Zero;

            // slm_get_developer_id
            byte[] developer_id = new byte[DEVELOPER_ID_LENGTH];
            ret = SlmRuntime.slm_get_developer_id(developer_id);
            if (ret != SSErrCode.SS_OK)
            {
                StrMsg = string.Format("slm_get_developer_id Failure:0x{0:X8}", ret);
                WriteLineRed(StrMsg);
            }
            else
            {
                WriteLineGreen("slm_get_developer_id Success!");

                // 将开发商ID转化为字符串
                string developerIDStr = BitConverter.ToString(developer_id).Replace("-", "");
                WriteLineYellow(developerIDStr);
            }

            //01. init
            ST_INIT_PARAM initPram = new ST_INIT_PARAM();
            initPram.version =SSDefine.SLM_CALLBACK_VERSION02;
            initPram.flag = SSDefine.SLM_INIT_FLAG_NOTIFY;
            pfn = new callback(handle_service_msg);     // 响应回调通知只有在 slm_init 后 slm_cleanup 之前有效。
            initPram.pfn = pfn;

            // 指定开发者 API 密码，示例代码指定 Demo 开发者的 API 密码。
            // 注意：正式开发者运行测试前需要修改此值，可以从 Virbox 开发者网站获取 API 密码。
            initPram.password = new byte[]{0xDB,0x3B,0x83,0x8B,0x2E,0x4F,0x08,0xF5,0xC9,0xEF,0xCD,0x1A,0x5D,0xD1,0x63,0x41};

            ret = SlmRuntime.slm_init(ref initPram);
            if (ret == SSErrCode.SS_OK)
            {
                WriteLineGreen("Slminit Success!");
            }
            else if (ret == SSErrCode.SS_ERROR_DEVELOPER_PASSWORD)
            {
                StrMsg = string.Format("Slminit Failure:0x{0:X8}(ERROR_DEVELOPER_PASSWORD). Please login to the Virbox Developer Center(https://developer.lm.virbox.com), get the API password, and replace the 'initPram.password' variable content.", ret);
                WriteLineRed(StrMsg);
            }
            else
            {
                StrMsg = string.Format("Slm_Init Failure:0x{0:X8}", ret);
                WriteLineRed(StrMsg);
            }

            //02. find License
            IntPtr desc = IntPtr.Zero;
            ret = SlmRuntime.slm_find_license(1, INFO_FORMAT_TYPE.JSON, ref desc);
            if (ret != SSErrCode.SS_OK)
            {
                StrMsg = string.Format("slm_find_license Failure:0x{0:X8}", ret);
                WriteLineRed(StrMsg);
            }
            else
            {
                string StrPrint = Marshal.PtrToStringAnsi(desc);
                WriteLineYellow(StrPrint);
                WriteLineGreen("SlmFindLicenseEasy Success!");
                SlmRuntime.slm_free(desc);
                if (ret != SSErrCode.SS_OK)
                {
                    StrMsg = string.Format("slm_free Failure:0x{0:X8}", ret);
                    WriteLineRed(StrMsg);
                }
            }


            //03. LOGIN
            ST_LOGIN_PARAM stLogin = new ST_LOGIN_PARAM();
            stLogin.size = (UInt32)Marshal.SizeOf(stLogin);

            // 指定登录的许可ID，Demo 设置登录0号许可，开发者正式使用时可根据需要调整此参数。
            stLogin.license_id = 0;

            // 指定登录许可ID的容器，Demo 设置为本地加密锁，开发者可根据需要调整此参数。
            stLogin.login_mode = SSDefine.SLM_LOGIN_MODE_LOCAL;

            ret = SlmRuntime.slm_login(ref stLogin, INFO_FORMAT_TYPE.STRUCT, ref Handle, a);
            if (ret != SSErrCode.SS_OK)
            {
                StrMsg = string.Format("Slm_Login Failure:0x{0:X8}", ret);
                WriteLineRed(StrMsg);
            }
            else
            {
                WriteLineGreen("Slmlogin Success!");
            }

            //04. KEEP ALIVE
            ret = SlmRuntime.slm_keep_alive(Handle);
            if (ret != SSErrCode.SS_OK)
            {
                StrMsg = string.Format("SlmKeepAliveEasy Failure:0x{0:X8}", ret);
                WriteLineRed(StrMsg);
                System.Diagnostics.Debug.Assert(true);
            }
            else
            {
                WriteLineGreen("SlmKeepAliveEasy Success!");
            }

            //05. get_info
            //lock_info
            ret = SlmRuntime.slm_get_info(Handle,INFO_TYPE.LOCK_INFO,INFO_FORMAT_TYPE.JSON,ref desc);
            if (ret != SSErrCode.SS_OK)
            {
                StrMsg = string.Format("slm_get_info(local_info) Failure:0x{0:X8}", ret);
                WriteLineRed(StrMsg);
            }
            else
            {
                string StrPrint = Marshal.PtrToStringAnsi(desc);
                WriteLineYellow(StrPrint);
                WriteLineGreen("slm_get_info(local_info) Success!");
                if (ret != SSErrCode.SS_OK)
                {
                    StrMsg = string.Format("slm_free Failure:0x{0:X8}", ret);
                    WriteLineRed(StrMsg);
                }
            }
            //session_info
            ret = SlmRuntime.slm_get_info(Handle, INFO_TYPE.SESSION_INFO, INFO_FORMAT_TYPE.JSON, ref desc);
            if (ret != SSErrCode.SS_OK)
            {
                StrMsg = string.Format("slm_get_info(session_info) Failure:0x{0:X8}", ret);
                WriteLineRed(StrMsg);
            }
            else
            {
                string StrPrint = Marshal.PtrToStringAnsi(desc);
                WriteLineYellow(StrPrint);
                WriteLineGreen("slm_get_info(session_info) Success!");
                if (ret != SSErrCode.SS_OK)
                {
                    StrMsg = string.Format("slm_free Failure:0x{0:X8}", ret);
                    WriteLineRed(StrMsg);
                }
            }
            //license_info
            ret = SlmRuntime.slm_get_info(Handle, INFO_TYPE.LICENSE_INFO, INFO_FORMAT_TYPE.JSON, ref desc);
            if (ret != SSErrCode.SS_OK)
            {
                StrMsg = string.Format("slm_get_info(license_info) Failure:0x{0:X8}", ret);
                WriteLineRed(StrMsg);
            }
            else
            {
                string StrPrint = Marshal.PtrToStringAnsi(desc);
                WriteLineYellow(StrPrint);
                WriteLineGreen("slm_get_info(license_info) Success!");
                if (ret != SSErrCode.SS_OK)
                {
                    StrMsg = string.Format("slm_free Failure:0x{0:X8}", ret);
                    WriteLineRed(StrMsg);
                }
            }

            //07 08. slm_encrypt  slm_decrypt
            //slm_encrypt
             string StrData = "test data.......";
            byte[] Data = System.Text.ASCIIEncoding.Default.GetBytes(StrData);
            byte[] Enc = new byte[StrData.Length];
            byte[] Dec = new byte[StrData.Length];

            WriteLineYellow(string.Format("[Before the encryption DATA]:{0}", StrData));
            ret = SlmRuntime.slm_encrypt(Handle,Data,Enc,(uint)StrData.Length);
            if (ret != SSErrCode.SS_OK)
            {
                StrMsg = string.Format("slm_encrypt Failure:0x{0:X8}", ret);
                WriteLineRed(StrMsg);
            }
            else
            {
                WriteLineYellow(string.Format("[encrypted DATA]:{0}",System.Text.ASCIIEncoding.Default.GetString(Enc) ));
                WriteLineGreen("slm_encrypt Success!"); 
            }
            //slm_decrypt
            ret = SlmRuntime.slm_decrypt(Handle,Enc,Dec,(uint)StrData.Length);
            if (ret != SSErrCode.SS_OK)
            {
                StrMsg = string.Format("slm_decrypt Failure:0x{0:X8}", ret);
                WriteLineRed(StrMsg);
            }
            else
            {
                WriteLineYellow(string.Format("[decrypted DATA]:{0}", System.Text.ASCIIEncoding.Default.GetString(Dec)));
                WriteLineGreen("slm_decrypt Success!");
            }

            //09. 10. 11.  slm_user_data_getsize slm_user_data_read  slm_user_data_write
            //slm_user_data_getsize
            UInt32 dataSize=0;
            ret = SlmRuntime.slm_user_data_getsize(Handle, LIC_USER_DATA_TYPE.RAW, ref dataSize);
            if (ret != SSErrCode.SS_OK)
            {
                StrMsg = string.Format("slm_user_data_getsize Failure:0x{0:X8}", ret);
                WriteLineRed(StrMsg);
            }
            else
            {
                WriteLineGreen("slm_user_data_getsize Success!");
                if (dataSize > 0)
                {
                    //slm_user_data_read
                    byte[] readbuf = new byte [dataSize];
                    ret = SlmRuntime.slm_user_data_read(Handle, LIC_USER_DATA_TYPE.RAW, readbuf, 0, dataSize);
                    if (ret != SSErrCode.SS_OK)
                    {
                        StrMsg = string.Format("slm_user_data_read Failure:0x{0:X8}", ret);
                        WriteLineRed(StrMsg);
                    }
                    else
                    {
                        // 读写区需要在用开发者管理工具写许可的时候，设置读写区大小，此处做判断，如果读写区未初始化（数据全为0）则写入数据，否则输出内容
                        // 判断数据是否为空，空写入数据，否则输出内容
                        UInt32 flag = 0;
                        for (int i = 0; i < readbuf.Length; i++)
                        {
                            if (readbuf[i] == 0)
                                flag = 1;
                            else
                            {
                                flag = 2;
                                break;
                            }
                        }
                        if (flag == 1)
                        {
                            //slm_user_data_write
                            string buf = "";//输入要写入数据区内容
                            byte[] writebuf = System.Text.ASCIIEncoding.Default.GetBytes(buf);
                            ret = SlmRuntime.slm_user_data_write(Handle, writebuf, 0, (UInt32)buf.Length);
                            if (ret != SSErrCode.SS_OK)
                            {
                                StrMsg = string.Format("slm_user_data_write Failure:0x{0:X8}", ret);
                                WriteLineRed(StrMsg);
                            }
                            else
                            {
                                WriteLineYellow(string.Format("[Write RAW DATA]:{0}", writebuf));
                                WriteLineGreen("slm_user_data_write Success!");
                            }
                        }
                        else if(flag ==2)
                        {
                            WriteLineYellow(string.Format("[Read RAW DATA]:{0}", System.Text.ASCIIEncoding.Default.GetString(readbuf)));
                        }
                        WriteLineGreen("slm_user_data_read Success!");
                    }
                }
                else 
                {
                    WriteLineYellow(string.Format("[No data area]:{0}", dataSize));
                }
            }

            ////12. 13. 14. 15. 16 slm_mem_alloc - slm_mem_write -slm_mem_read - slm_mem_free
            //slm_mem_alloc
            UInt32 mem_index = 0;
            ret = SlmRuntime.slm_mem_alloc(Handle,1024,ref mem_index);
            if (ret != SSErrCode.SS_OK)
            {
                StrMsg = string.Format("slm_mem_alloc Failure:0x{0:X8}", ret);
                WriteLineRed(StrMsg);
            }
            else
            {
                WriteLineYellow(string.Format("[mem_index ]:{0}", mem_index));
                WriteLineGreen("slm_mem_alloc Success!");
            }
            //slm_mem_write
            string mem_buff = "test memory data...";
            UInt32 mem_size = (UInt32)mem_buff.Length;
            UInt32 mem_len = 0;
            byte[] mem_write_buf = System.Text.ASCIIEncoding.Default.GetBytes(mem_buff);
            byte[] mem_read_buf = new byte[mem_size];


            ret = SlmRuntime.slm_mem_write(Handle, mem_index, 0, mem_size, mem_write_buf, ref mem_len);
            if (ret != SSErrCode.SS_OK)
            {
                StrMsg = string.Format("slm_mem_write Failure:0x{0:X8}", ret);
                WriteLineRed(StrMsg);
                System.Diagnostics.Debug.Assert(true);
            }
            else
            {
                WriteLineYellow(string.Format("[Mem Write]:{0}", mem_buff));
                WriteLineGreen("slm_mem_write Success!");
            }
            //slm_mem_read
            ret = SlmRuntime.slm_mem_read(Handle,mem_index,0,mem_size,mem_read_buf,ref mem_len);
            if (ret != SSErrCode.SS_OK)
            {
                StrMsg = string.Format("slm_mem_write Failure:0x{0:X8}", ret);
                WriteLineRed(StrMsg);
                System.Diagnostics.Debug.Assert(true);
            }
            else
            {
                string StrPrint = string.Format("[Mem Read]:{0}", System.Text.ASCIIEncoding.Default.GetString(mem_read_buf));
                WriteLineYellow(StrPrint);
                WriteLineGreen("slm_mem_write Success!");
            }
            //slm_mem_free
            ret  = SlmRuntime.slm_mem_free(Handle,mem_index);
            if (ret != SSErrCode.SS_OK)
            {
                StrMsg = string.Format("slm_mem_write Failure:0x{0:X8}", ret);
                WriteLineRed(StrMsg);
                System.Diagnostics.Debug.Assert(true);
            }
            else
            {
                WriteLineGreen("slm_mem_write Success!");
            }
            
            //17 slm_logout
            ret = SlmRuntime.slm_logout(Handle);
            if (ret != SSErrCode.SS_OK)
            {
                StrMsg = string.Format("slm_logout Failure:0x{0:X8}", ret);
                WriteLineRed(StrMsg); 
            }
            else
            {
                WriteLineGreen("slm_logout Success!");
            }

            //18. slm_error_format
            IntPtr  result;
            result  = SlmRuntime.slm_error_format(2,SSDefine.LANGUAGE_ENGLISH_ASCII);
            if (result != IntPtr.Zero)
            {
                string error = Marshal.PtrToStringAnsi(result);
                StrMsg = string.Format("slm_error_format success, code = 0x{0:X8}, message = {1}", ret, error);
                WriteLineGreen(StrMsg);
            }
            else
            {
                WriteLineRed("slm_error_format Failure!");
            }

            //19. slm_get_version
            UInt32 api_version = 0;
            UInt32 ss_version = 0;
            ret = SlmRuntime.slm_get_version(ref api_version, ref ss_version);
            if (ret != SSErrCode.SS_OK)
            {
                StrMsg = string.Format("slm_get_version Failure:0x{0:X8}", ret);
                WriteLineRed(StrMsg);
            }
            else
            {
                StrMsg = string.Format("api_version :0x{0:X8},ss_version:0X{0:X8}", api_version, ss_version);
                WriteLineYellow(StrMsg);
                WriteLineGreen("slm_get_version Success!");
            }

            //20. slm_enum_device
            IntPtr device_info = IntPtr.Zero;
            ret = SlmRuntime.slm_enum_device(ref device_info);
            if (ret != SSErrCode.SS_OK)
            {
                StrMsg = string.Format("slm_enum_device Failure:0x{0:X8}", ret);
                WriteLineRed(StrMsg);
            }
            else
            {
                string StrPrint = Marshal.PtrToStringAnsi(device_info);
                WriteLineYellow(StrPrint);
                WriteLineGreen("slm_enum_device Success!");
            }

            //21. slm_enum_license_id
            IntPtr license_ids= IntPtr.Zero;
            string dev_info=Marshal.PtrToStringAnsi(device_info);
            JArray arrDeviceInfo = (JArray)JsonConvert.DeserializeObject(dev_info);
            for (int i = 0; i < arrDeviceInfo.Count; i++)
            {
                string Info = arrDeviceInfo[i].ToString();
                ret = SlmRuntime.slm_enum_license_id(Info, ref license_ids);
                if (ret != SSErrCode.SS_OK)
                {
                    StrMsg = string.Format("slm_enum_license_id Failure:0x{0:X8}", ret);
                    WriteLineRed(StrMsg);
                }
                else
                {
                    string StrPrint = Marshal.PtrToStringAnsi(license_ids);
                    WriteLineYellow(StrPrint);
                    WriteLineGreen("slm_enum_license_id Success!");
                }

                //23. slm_get_license_info,举例0号许可
                IntPtr license_info = IntPtr.Zero;
                ret = SlmRuntime.slm_get_license_info(Info, 0, ref license_info);
                if (ret != SSErrCode.SS_OK)
                {
                    StrMsg = string.Format("slm_get_license_info Failure:0x{0:X8}", ret);
                    WriteLineRed(StrMsg);
                }
                else
                {
                    string StrPrint = Marshal.PtrToStringAnsi(license_info);
                    WriteLineYellow(StrPrint);
                    WriteLineGreen("slm_get_license_info Success!");
                }
            }

            // 暂停程序，演示响应拔插锁消息回调通知功能，点击任意键继续。
            WriteLineYellow("Program pause, demonstrate the response to unplug dongle callback message notification function, press any key to continue.");
            Console.ReadKey();

            ret = SlmRuntime.slm_logout(Handle);

            //22. slm_cleanup
            ret = SlmRuntime.slm_cleanup();
            if (ret != SSErrCode.SS_OK)
            {
                StrMsg = string.Format("slm_cleanup Failure:0x{0:X8}", ret);
                WriteLineRed(StrMsg);
            }
            else
            {
                WriteLineGreen("slm_cleanup Success!");
            }

            Console.ReadKey();
        }
        */
}
