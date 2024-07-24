using RSJWYFamework.Runtime.ReferencePool;
using UnityEngine;

namespace RSJWYFamework.Runtime.ExceptionLogUp
{
    /// <summary>
    /// 日志信息-来自HTFamework
    /// </summary>
    public sealed class ExceptionLogInfo: IReference
    {
        /// <summary>
        /// 异常类型
        /// </summary>
        public LogType Type;
        /// <summary>
        /// 异常日志
        /// </summary>
        public string LogString;
        /// <summary>
        /// 异常堆栈信息
        /// </summary>
        public string StackTrace;

        public ExceptionLogInfo Fill(string logString, string stackTrace, LogType type)
        {
            Type = type;
            LogString = logString;
            StackTrace = stackTrace;
            return this;
        }
        public void Reset()
        {
            Type = LogType.Error;
            LogString = null;
            StackTrace = null;
        }
    }
}