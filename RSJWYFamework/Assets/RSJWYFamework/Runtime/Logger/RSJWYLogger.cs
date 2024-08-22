using System.Diagnostics;
using RSJWYFamework.Runtime.Main;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RSJWYFamework.Runtime.Logger
{

    public static class RSJWYLogger
    {
        /// <summary>
        /// 日志记录器
        /// </summary>
        private static UnityEngine.Logger logger_Unity;
        
        /// <summary>
        /// 日志记录器
        /// </summary>
        private static UnityEngine.Logger logger_rf;

        static RSJWYLogger()
        {
            
           
        }
        /// <summary>
        /// 日志
        /// </summary>
        [Conditional("DEBUG")]
        public static void Log(string info)
        {
            /*logger_rf.Debug(info);*/
            
            Debug.Log(info);
            if (!Debugger.IsLogging())
                Main.Main.ExceptionLogManager
                    .UnityLogMessageReceivedThreadedEvent(info, null, LogType.Log);
        }
        /// <summary>
        /// 日志
        /// </summary>
        [Conditional("DEBUG")]
        public static void Log(RSJWYFameworkEnum @enum,string info)
        {
            //logger_rf.Debug($"{@enum}:{info}");
            
            UnityEngine.Debug.Log($"{@enum}:{info}");
            if (!Debugger.IsLogging())
                Main.Main.ExceptionLogManager
                    .UnityLogMessageReceivedThreadedEvent($"{@enum}:\n{info}", null, LogType.Log);
        }

        /// <summary>
        /// 警告
        /// </summary>
        public static void Warning(string info)
        {
           // logger_rf.Warn(info);
            
            UnityEngine.Debug.LogWarning(info);
            if (!Debugger.IsLogging())
            {
                // 获取堆栈跟踪信息
                StackTrace stackTrace = new StackTrace(1, true); // 1 表示跳过当前方法帧，true 表示包含文件信息
                Main.Main.ExceptionLogManager
                    .UnityLogMessageReceivedThreadedEvent(info, stackTrace.ToString(), LogType.Warning);
            }
        }
        /// <summary>
        /// 警告
        /// </summary>
        public static void Warning(RSJWYFameworkEnum @enum,string info)
        {
            //logger_rf.Warn($"{@enum}:{info}");
            
            UnityEngine.Debug.LogWarning($"{@enum}:{info}");;
            if (!Debugger.IsLogging())
                Main.Main.ExceptionLogManager
                    .UnityLogMessageReceivedThreadedEvent($"{@enum}:\n{info}", null, LogType.Warning);
        }

        /// <summary>
        /// 错误
        /// </summary>
        public static void Error(string info)
        {
           // logger_rf.Error(info);
            
            UnityEngine.Debug.LogError(info);
            if (!Debugger.IsLogging())
                Main.Main.ExceptionLogManager
                    .UnityLogMessageReceivedThreadedEvent(info, null, LogType.Error);
        }
        /// <summary>
        /// 错误
        /// </summary>
        public static void Error(RSJWYFameworkEnum @enum,string info)
        {
            //logger_rf.Error($"{@enum}:{info}");
            
            UnityEngine.Debug.LogError($"{@enum}:{info}");;
            if (!Debugger.IsLogging())
                Main.Main.ExceptionLogManager
                    .UnityLogMessageReceivedThreadedEvent($"{@enum}:\n{info}", null, LogType.Error);
        }

        /// <summary>
        /// 异常
        /// </summary>
        public static void Exception(System.Exception exception)
        {
           // logger_rf.Fatal(exception);
            
            UnityEngine.Debug.LogException(exception);
            if (!Debugger.IsLogging())
                Main.Main.ExceptionLogManager
                    .UnityLogMessageReceivedThreadedEvent(exception.Message, exception.StackTrace, LogType.Exception);
        }
        /// <summary>
        /// 异常
        /// </summary>
        public static void Exception(RSJWYFameworkEnum @enum,System.Exception exception)
        {
           // logger_rf.Fatal($"{@enum}:{exception.Message}");
            
            UnityEngine.Debug.LogException(exception);
            if (!Debugger.IsLogging())
                Main.Main.ExceptionLogManager
                    .UnityLogMessageReceivedThreadedEvent($"{@enum}:{exception.Message}", exception.StackTrace, LogType.Exception);
        }

    }
}