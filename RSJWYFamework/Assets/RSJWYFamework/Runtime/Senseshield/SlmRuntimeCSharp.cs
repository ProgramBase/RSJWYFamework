using System;
using System.Runtime.InteropServices;
using SenseShield;
using SLM_HANDLE_INDEX = System.UInt32;

namespace RSJWYFamework.Runtime.Senseshield
{
    public delegate uint callback(uint message, UIntPtr wparam, UIntPtr lparam);
    /// <summary>
    /// 深思数盾运行时RuntimeAPI
    /// </summary>
    internal class SlmRuntime
    {
#if DEBUG
        const string dll_name = "Assets/Plugins/senseshield/x64/slm_runtime_dev.dll";
#else
        // 正式发版，使用具有反调试功能的运行时库（不允许调试）
        const string dll_name = "Assets/Plugins/senseshield/x64/slm_runtime.dll";
#endif


        /// <summary>
        /// Runtime API初始化函数，调用所有Runtime API必须先调用此函数进行初始化
        /// </summary>
        ///  <param name="init_param"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_init", CallingConvention = CallingConvention.StdCall)]
        internal static extern UInt32 slm_init(ref ST_INIT_PARAM initParam);


        /// <summary>
        /// 列举锁内某id许可
        /// </summary>
        /// <param name="license_id"></param>
        /// <param name="format"></param>
        /// <param name="license_desc"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_find_license", CallingConvention = CallingConvention.StdCall)]
        internal static extern UInt32 slm_find_license(UInt32 license_id, INFO_FORMAT_TYPE format,
            ref IntPtr license_desc);

        /// <summary>
        /// 安全登录许可
        /// </summary>
        /// <param name="license_param"></param>
        /// <param name="param_format"></param>
        /// <param name="slm_handle"></param>
        /// <param name="auth"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_login", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_login(ref ST_LOGIN_PARAM license_param, INFO_FORMAT_TYPE param_format,
            ref SLM_HANDLE_INDEX slm_handle, IntPtr auth);

        /// <summary>
        /// 枚举已登录的用户token
        /// </summary>
        /// <param name="access_token">默认用户的token，指向一个字符串的IntPtr</param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_get_cloud_token", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_get_cloud_token(ref IntPtr access_token);

        /// <summary>
        /// 许可登出，并且释放许可句柄等资源
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_logout", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_logout(SLM_HANDLE_INDEX slm_handle);

        /// <summary>
        /// 保持登录会话心跳，避免变为“僵尸句柄”。 
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_keep_alive", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_keep_alive(
            SLM_HANDLE_INDEX slm_handle);

        /// <summary>
        /// 保持登录会话心跳，避免变为“僵尸句柄”。 
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <param name="module_id"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_check_module", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_check_module(
            SLM_HANDLE_INDEX slm_handle,
            UInt32 module_id);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <param name="inbuffer"></param>
        /// <param name="outbuffer"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_encrypt", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_encrypt(
            SLM_HANDLE_INDEX slm_handle,
            [In, MarshalAs(UnmanagedType.LPArray)] byte[] inbuffer,
            [In, Out, MarshalAs(UnmanagedType.LPArray)]
            byte[] outbuffer,
            UInt32 len);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <param name="inbuffer"></param>
        /// <param name="outbuffer"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_decrypt", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_decrypt(
            SLM_HANDLE_INDEX slm_handle,
            [In, MarshalAs(UnmanagedType.LPArray)] byte[] inbuffer,
            [In, Out, MarshalAs(UnmanagedType.LPArray)]
            byte[] outbuffer,
            UInt32 len);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <param name="type"></param>
        /// <param name="pmem_size"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_user_data_getsize", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_user_data_getsize(
            SLM_HANDLE_INDEX slm_handle,
            LIC_USER_DATA_TYPE type,
            ref UInt32 pmem_size);


        /// <summary>
        /// 读许可数据，可以读取RW和ROM
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <param name="type"></param>
        /// <param name="readbuf"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_user_data_read", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_user_data_read(
            SLM_HANDLE_INDEX slm_handle,
            LIC_USER_DATA_TYPE type,
            [Out, MarshalAs(UnmanagedType.LPArray)]
            byte[] readbuf,
            UInt32 offset,
            UInt32 len);

        /// <summary>
        /// 写许可的读写数据区 ,数据区操作之前请先确认内存区的大小，可以使用slm_user_data_getsize获得
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <param name="writebuf"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_user_data_write", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_user_data_write(
            SLM_HANDLE_INDEX slm_handle,
            [In, MarshalAs(UnmanagedType.LPArray)] byte[] writebuf,
            UInt32 offset,
            UInt32 len);


        /// <summary>
        /// 获取已登录许可的状态信息，例如许可信息、硬件锁信息等 
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <param name="info_type"></param>
        /// <param name="format"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_get_info", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_get_info(
            SLM_HANDLE_INDEX slm_handle,
            INFO_TYPE info_type,
            INFO_FORMAT_TYPE format,
            ref IntPtr result);


        /// <summary>
        /// 执行锁内算法
        /// </summary>
        /// <param name="slm_handle">许可句柄值</param>
        /// <param name="exfname">锁内执行文件名</param>
        /// <param name="inbuf">输入缓冲区</param>
        /// <param name="insize">输入长度</param>
        /// <param name="poutbuf">输出缓存区</param>
        /// <param name="outsize">输出缓存长度</param>
        /// <param name="pretsize">实际返回缓存长度</param>
        /// <returns>成功返回SS_OK，失败返回相应的错误码</returns>
        [DllImport(dll_name, EntryPoint = "slm_execute_static", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_execute_static(
            SLM_HANDLE_INDEX slm_handle,
            [In, MarshalAs(UnmanagedType.LPStr)] string exfname,
            [In, MarshalAs(UnmanagedType.LPArray)] byte[] inbuf,
            UInt32 insize,
            [Out, MarshalAs(UnmanagedType.LPArray)]
            byte[] poutbuf,
            UInt32 outsize,
            ref UInt32 pretsize);

        /// <summary>
        /// 许可动态执行代码，由开发商API gen_dynamic_code生成
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <param name="exf_buffer"></param>
        /// <param name="exf_size"></param>
        /// <param name="inbuf"></param>
        /// <param name="insize"></param>
        /// <param name="poutbuf"></param>
        /// <param name="outsize"></param>
        /// <param name="pretsize"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_execute_dynamic", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_execute_dynamic(
            SLM_HANDLE_INDEX slm_handle,
            [In, MarshalAs(UnmanagedType.LPArray)] byte[] exf_buffer,
            UInt32 exf_size,
            [In, MarshalAs(UnmanagedType.LPArray)] byte[] inbuf,
            UInt32 insize,
            [Out, MarshalAs(UnmanagedType.LPArray)]
            byte[] poutbuf,
            UInt32 outsize,
            ref UInt32 pretsize);

        /// <summary>
        /// SS内存托管内存申请
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <param name="size"></param>
        /// <param name="mem_id"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_mem_alloc", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_mem_alloc(
            SLM_HANDLE_INDEX slm_handle,
            UInt32 size,
            ref UInt32 mem_id);

        /// <summary>
        /// 释放托管内存
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <param name="mem_id"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_mem_free", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_mem_free(
            SLM_HANDLE_INDEX slm_handle,
            UInt32 mem_id);

        /// <summary>
        /// SS内存托管读
        /// </summary>
        /// <param name="slm_handle">许可句柄值</param>
        /// <param name="mem_id">托管内存id</param>
        /// <param name="offset">偏移</param>
        /// <param name="len">长度</param>
        /// <param name="readbuff">缓存</param>
        /// <param name="readlen">返回实际读的长度</param>
        /// <returns>成功返回SS_OK，失败返回相应的错误码</returns>
        [DllImport(dll_name, EntryPoint = "slm_mem_read", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_mem_read(
            SLM_HANDLE_INDEX slm_handle,
            UInt32 mem_id,
            UInt32 offset,
            UInt32 len,
            [In, MarshalAs(UnmanagedType.LPArray)] byte[] readbuff,
            ref UInt32 readlen);

        /// <summary>
        /// SS内存托管内存写入
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <param name="mem_id"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        /// <param name="writebuff"></param>
        /// <param name="numberofbyteswritten"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_mem_write", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_mem_write(
            SLM_HANDLE_INDEX slm_handle,
            UInt32 mem_id,
            UInt32 offset,
            UInt32 len,
            [In, MarshalAs(UnmanagedType.LPArray)] byte[] writebuff,
            ref UInt32 numberofbyteswritten);


        /// <summary>
        /// 检测是否正在调试
        /// </summary>
        /// <param name="auth">auth 验证数据(目前填IntPtr.Zero即可）</param>
        /// <returns>SS_UINT32错误码, 返回SS_OK代表未调试</returns>
        [DllImport(dll_name, EntryPoint = "slm_is_debug", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_is_debug64(IntPtr auth);

        /// <summary>
        /// 获取锁的设备证书
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <param name="device_cert"></param>
        /// <param name="buff_size"></param>
        /// <param name="return_size"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_get_device_cert", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_get_device_cert(
            SLM_HANDLE_INDEX slm_handle,
            [In, Out, MarshalAs(UnmanagedType.LPArray)]
            byte[] device_cert,
            UInt32 buff_size,
            ref UInt32 return_size);

        /// <summary>
        /// 设备正版验证
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <param name="verify_data"></param>
        /// <param name="verify_data_size"></param>
        /// <param name="signature"></param>
        /// <param name="signature_buf_size"></param>
        /// <param name="signature_size"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_sign_by_device", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_sign_by_device(
            SLM_HANDLE_INDEX slm_handle,
            [In, MarshalAs(UnmanagedType.LPArray)] byte[] verify_data,
            UInt32 verify_data_size,
            [Out, MarshalAs(UnmanagedType.LPArray)]
            byte[] signature,
            UInt32 signature_buf_size,
            ref UInt32 signature_size);


        /// <summary>
        /// 获取时间修复数据，用于生成时钟校准请求
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <param name="rand"></param>
        /// <param name="lock_time"></param>
        /// <param name="pc_time"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_adjust_time_request", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_adjust_time_request(
            SLM_HANDLE_INDEX slm_handle,
            [Out, MarshalAs(UnmanagedType.LPArray)]
            byte[] rand,
            ref UInt32 lock_time,
            ref UInt32 pc_time
        );

        /// <summary>
        /// 闪烁指示灯
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <param name="led_ctrl"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_led_control", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_led_control(
            SLM_HANDLE_INDEX slm_handle,
            ref ST_LED_CONTROL led_ctrl);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="api_version"></param>
        /// <param name="ss_version"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_get_version", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_get_version(
            ref UInt32 api_version,
            ref UInt32 ss_version);

        /// <summary>
        /// 升级许可
        /// </summary>
        /// <param name="d2c_pkg">许可D2C数据</param>
        /// <param name="error_msg">错误信息（json）</param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_update", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_update(
            [In, MarshalAs(UnmanagedType.LPStr)] string d2c_pkg,
            ref IntPtr error_msg);

        /// <summary>
        ///  将D2C包进行升级
        /// </summary>
        /// <param name="lock_sn"></param>
        /// <param name="d2c_pkg"></param>
        /// <param name="error_msg"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_update_ex", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_update_ex(
            [In, MarshalAs(UnmanagedType.LPArray)] byte[] lock_sn,
            [In, MarshalAs(UnmanagedType.LPStr)] string d2c_pkg,
            ref IntPtr error_msg);

        /// <summary>
        ///  枚举本地锁信息
        /// </summary>
        /// <param name="device_info"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_enum_device", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_enum_device(
            ref IntPtr device_info);

        /// <summary>
        ///   释放API内分配堆区域 
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_free", CallingConvention = CallingConvention.StdCall)]
        public static extern void slm_free(IntPtr buffer);

        /// <summary>
        ///   获取 Runtime 库对应的开发商ID 
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_get_developer_id", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_get_developer_id(
            [Out, MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer);

        /// <summary>
        /// 通过错误码获得错误信息
        /// </summary>
        /// <param name="error_code"></param>
        /// <param name="language_id"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_error_format", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr slm_error_format(
            UInt32 error_code,
            UInt32 language_id
        );

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_cleanup", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_cleanup();

        /// <summary>
        /// 碎片代码执行（开发者不必关心）
        /// </summary>
        /// <param name="slm_handle"></param> 
        /// <param name="snippet_code"></param>
        /// <param name="code_size"></param>
        /// <param name="input"></param>
        /// <param name="input_size"></param>
        /// <param name="output"></param>
        /// <param name="outbuf_size"></param>
        /// <param name="output_size"></param> 
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_snippet_execute", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_snippet_execute(
            SLM_HANDLE_INDEX slm_handle,
            [In, MarshalAs(UnmanagedType.LPArray)] byte[] d2c_pkg,
            UInt32 code_size,
            [In, MarshalAs(UnmanagedType.LPArray)] byte[] input,
            UInt32 input_size,
            [Out, MarshalAs(UnmanagedType.LPArray)]
            byte[] output,
            UInt32 outbuf_size,
            ref UInt32 language_id);


        /// <summary>
        /// 获得指定许可的公开区数据区大小，需要登录0号许可
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <param name="license_id"></param>
        /// <param name="pmem_size"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_pub_data_getsize", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_pub_data_getsize(
            SLM_HANDLE_INDEX slm_handle,
            UInt32 license_id,
            ref UInt32 pmem_size);

        /// <summary>
        /// 获得指定许可的公开区数据区大小，需要登录0号许可
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <param name="license_id"></param>
        /// <param name="readbuf"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_pub_data_read", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_pub_data_read(
            SLM_HANDLE_INDEX slm_handle,
            UInt32 license_id,
            [Out, MarshalAs(UnmanagedType.LPArray)]
            byte[] readbuf,
            UInt32 offset,
            UInt32 len);

        /// <summary>
        /// 锁内短码升级
        /// </summary>
        /// <param name="lock_sn"></param>
        /// <param name="inside_file"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_d2c_update_inside", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_d2c_update_inside(
            [In, MarshalAs(UnmanagedType.LPStr)] string lock_sn,
            [In, MarshalAs(UnmanagedType.LPStr)] string inside_file);

        /// <summary>
        /// 枚举指定设备下所有许可ID
        /// </summary>
        /// <param name="device_info"></param>
        /// <param name="license_ids"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_enum_license_id", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_enum_license_id(
            [In, MarshalAs(UnmanagedType.LPStr)] string device_info,
            ref IntPtr license_ids);

        /// <summary>
        /// 枚举指定设备下所有许可ID
        /// </summary>
        /// <param name="device_info"></param>
        /// <param name="license_id"></param>
        /// <param name="license_info"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_get_license_info", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_get_license_info(
            [In, MarshalAs(UnmanagedType.LPStr)] string device_info,
            UInt32 license_id,
            ref IntPtr license_info);

        /// <summary>
        /// 使用已登录的云许可进行签名（仅支持云锁）
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <param name="sign_data"></param>
        /// <param name="sign_length"></param>
        ///  <param name="signature"></param>
        ///   <param name="max_buf_size"></param>
        ///    <param name="signature_length"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_license_sign", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_license_sign(
            SLM_HANDLE_INDEX slm_handle,
            [In, MarshalAs(UnmanagedType.LPArray)] byte[] sign_data,
            UInt32 sign_length,
            [In, MarshalAs(UnmanagedType.LPArray)] byte[] signature,
            UInt32 max_buf_size,
            ref UInt32 signature_length);

        /// <summary>
        /// 对云许可签名后的数据进行验签（仅支持云锁）
        /// </summary>
        /// <param name="sign_data"></param>
        /// <param name="sign_length"></param>
        ///  <param name="signature"></param>
        ///   <param name="signature_length"></param>
        ///    <param name="sign_info"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_license_verify", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_license_verify(
            [In, MarshalAs(UnmanagedType.LPArray)] byte[] sign_data,
            UInt32 sign_length,
            [In, MarshalAs(UnmanagedType.LPArray)] byte[] signature,
            UInt32 signature_length,
            ref IntPtr sign_info);

        /// <summary>
        /// 通过证书类型，获取已登录许可的设备证书
        /// </summary>
        /// <param name="slm_handle"></param>
        /// <param name="cert_type"></param>
        ///  <param name="cert"></param>
        ///   <param name="cert_size"></param>
        ///    <param name="cert_len"></param>
        /// <returns></returns>
        [DllImport(dll_name, EntryPoint = "slm_get_cert", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 slm_get_cert(
            SLM_HANDLE_INDEX slm_handle,
            CERT_TYPE cert_type,
            [Out, MarshalAs(UnmanagedType.LPArray)]
            byte[] cert,
            UInt32 cert_size,
            ref UInt32 cert_len);
    }
}