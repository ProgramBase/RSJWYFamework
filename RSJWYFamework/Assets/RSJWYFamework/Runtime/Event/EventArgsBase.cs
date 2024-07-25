using System;
using RSJWYFamework.Runtime.ReferencePool;

namespace RSJWYFamework.Runtime.Event
{
    /// <summary>
    /// 事件内容载体
    /// </summary>
    public abstract class EventArgsBase:IReference
    {
        public abstract void Reset();
        /// <summary>
        /// 消息发送者
        /// </summary>
        public object Sender;

    }
}