using System.Diagnostics;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Main;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace RSJWYFamework.Runtime.Logger
{
    /// <summary>
    /// 自定义日志处理
    /// </summary>
    public interface ILogger
    {
        void Log(string message);
        void Warning(string message);
        void Error(string message);
        void Exception(System.Exception exception);
        
        void Exception(string e);
    }

    public static class RSJWYLogger
    {

        /// <summary>
        /// 日志
        /// </summary>
        [Conditional("DEBUG")]
        public static void Log(string info)
        {
            UnityEngine.Debug.Log(info);
            if (!Debugger.IsLogging())
                Main.Main.Instance.GetModule<DefaultExceptionLogManager>()
                    .UnityLogMessageReceivedThreadedEvent(info, null, LogType.Log);
        }
        /// <summary>
        /// 日志
        /// </summary>
        [Conditional("DEBUG")]
        public static void Log(RSJWYFameworkEnum @enum,string info)
        {
            UnityEngine.Debug.Log($"{@enum}:\n{info}");
            if (!Debugger.IsLogging())
                Main.Main.Instance.GetModule<DefaultExceptionLogManager>()
                    .UnityLogMessageReceivedThreadedEvent($"{@enum}:\n{info}", null, LogType.Log);
        }

        /// <summary>
        /// 警告
        /// </summary>
        public static void LogWarning(string info)
        {
            UnityEngine.Debug.LogWarning(info);
            if (!Debugger.IsLogging())
                Main.Main.Instance.GetModule<DefaultExceptionLogManager>()
                    .UnityLogMessageReceivedThreadedEvent(info, null, LogType.Warning);
        }
        /// <summary>
        /// 警告
        /// </summary>
        public static void LogWarning(RSJWYFameworkEnum @enum,string info)
        {
            UnityEngine.Debug.LogWarning($"{@enum}:\n{info}");;
            if (!Debugger.IsLogging())
                Main.Main.Instance.GetModule<DefaultExceptionLogManager>()
                    .UnityLogMessageReceivedThreadedEvent($"{@enum}:\n{info}", null, LogType.Warning);
        }

        /// <summary>
        /// 错误
        /// </summary>
        public static void LogError(string info)
        {
            UnityEngine.Debug.LogError(info);
            if (!Debugger.IsLogging())
                Main.Main.Instance.GetModule<DefaultExceptionLogManager>()
                    .UnityLogMessageReceivedThreadedEvent(info, null, LogType.Error);
        }
        /// <summary>
        /// 错误
        /// </summary>
        public static void LogError(RSJWYFameworkEnum @enum,string info)
        {
            UnityEngine.Debug.LogError($"{@enum}:\n{info}");;
            if (!Debugger.IsLogging())
                Main.Main.Instance.GetModule<DefaultExceptionLogManager>()
                    .UnityLogMessageReceivedThreadedEvent($"{@enum}:\n{info}", null, LogType.Error);
        }

        /// <summary>
        /// 异常
        /// </summary>
        public static void Exception(System.Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
            if (!Debugger.IsLogging())
                Main.Main.Instance.GetModule<DefaultExceptionLogManager>()
                    .UnityLogMessageReceivedThreadedEvent(exception.Message, exception.StackTrace, LogType.Exception);
        }
        /// <summary>
        /// 异常
        /// </summary>
        public static void Exception(RSJWYFameworkEnum @enum,System.Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
            if (!Debugger.IsLogging())
                Main.Main.Instance.GetModule<DefaultExceptionLogManager>()
                    .UnityLogMessageReceivedThreadedEvent($"{@enum}:\n{exception.Message}", exception.StackTrace, LogType.Exception);
        }

    }
}