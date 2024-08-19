using System.Diagnostics;
using System.IO;
using NLog;
using NLog.Config;
using RSJWYFamework.Runtime.Default.Manager;
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
        private static NLog.Logger logger_Unity;
        
        /// <summary>
        /// 日志记录器
        /// </summary>
        private static NLog.Logger logger_rf;
        
        static RSJWYLogger()
        {
            // 将配置文件移动到StreamingAssets目录下，以便在所有平台上都能正确读取
            string configFilePath = Path.Combine(Application.streamingAssetsPath, "RSJWYFamerowk/NLog.config");

            // 确保配置文件存在于指定路径
            if (!File.Exists(configFilePath))
            {
                Debug.LogError($"文件夹：{configFilePath}处无法找到nlog配置文件");
                return;
            }
            // 设置NLog的配置
            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(configFilePath);
            logger_Unity = LogManager.GetLogger("UnityLog");
            logger_rf = LogManager.GetLogger("RSJWYFamework");

            Application.logMessageReceivedThreaded += HandleLog;
        }
        
        static void HandleLog(string logString, string stackTrace, LogType type)
        {
            // 根据日志类型进行处理
            switch (type)
            {
                case LogType.Error:
                    logger_Unity.Error($"{type}---{logString}");
                    break;
                case LogType.Assert:
                    logger_Unity.Error($"{type}---{logString}");
                    break;
                case LogType.Warning:
                    logger_Unity.Warn($"{type}---{logString}");
                    break;
                case LogType.Log:
                    logger_Unity.Debug($"{type}---{logString}");
                    break;
                case LogType.Exception:
                    logger_Unity.Fatal($"{type}---{logString}");
                    break;
                default:
                    logger_Unity.Trace($"{type}---{logString}");
                    break;
            }
        }
        /// <summary>
        /// 日志
        /// </summary>
        [Conditional("DEBUG")]
        public static void Log(string info)
        {
            logger_rf.Debug(info);
            
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
            logger_rf.Debug($"{@enum}:{info}");
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
            logger_rf.Warn(info);
            
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
            logger_rf.Warn($"{@enum}:{info}");
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
            logger_rf.Error(info);
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
            logger_rf.Error($"{@enum}:{info}");
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
            logger_rf.Fatal(exception);
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
            logger_rf.Fatal($"{@enum}:{exception.Message}");
            UnityEngine.Debug.LogException(exception);
            if (!Debugger.IsLogging())
                Main.Main.ExceptionLogManager
                    .UnityLogMessageReceivedThreadedEvent($"{@enum}:{exception.Message}", exception.StackTrace, LogType.Exception);
        }

    }
}