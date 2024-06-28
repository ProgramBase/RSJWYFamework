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
        /// <summary>
        /// 查找许可信息(仅对硬件锁有效？？)
        /// 仅对硬件锁有效似乎没有这个限制了
        /// </summary>
        /// <returns></returns>
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
                RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield,$"{StrPrint}");
                RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield,"SlmFindLicenseEasy Success!");
                SlmRuntime.slm_free(desc);
                if (ret != SSErrCode.SS_OK)
                {
                    RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield,$"slm释放API堆区域失败 失败:0x{ret:X8}");
                }

                return _json;
            }
        }
        /// <summary>
        /// 安全登录许可,用 JSON 传递参数,并且检查时间（是否到期或者是否早于开始时间）、次数、并发数是否归零， 如果有计数器，则永久性减少对应的计数器，对于网络锁则临时增加网络并发计数器。
        /// </summary>
        /// <returns></returns>
        public static void LoginSS()
        {
            SLM_HANDLE_INDEX Handle = 0;
            uint ret = 0;
            IntPtr desc = IntPtr.Zero;
            IntPtr a = IntPtr.Zero;
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
                RSJWYLogger.LogError(RSJWYFameworkEnum.SenseShield, $"SLM 登录 失败:0x{ret:X8}");
            }
            else
            {
                RSJWYLogger.Log(RSJWYFameworkEnum.SenseShield, $"Slmlogin 成功!:0x{ret:X8}");
            }
        }
        
        public static void KeepAlive()
        {
            uint ret = 0;
            SLM_HANDLE_INDEX Handle = 0;
            string StrMsg = string.Empty;
            
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