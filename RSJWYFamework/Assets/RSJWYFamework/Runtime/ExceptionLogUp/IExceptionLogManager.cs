using MyFamework.Runtime.Base;
using UnityEngine;

namespace RSJWYFamework.Runtime.ExceptionLogUp
{
    public interface IExceptionLogManager:ModleInterface
    {
        /// <summary>
        /// 绑定Unity日志监听
        /// </summary>
        void UnityLogMessageReceivedThreadedEvent(string log, string stacktrace, LogType type);
    }
}