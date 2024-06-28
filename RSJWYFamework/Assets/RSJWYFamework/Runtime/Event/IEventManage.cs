using System;
using System.Collections.Concurrent;

namespace RSJWYFamework.Runtime.Event
{
    /// <summary>
    /// 事件系统接口
    /// </summary>
    public interface IEventManage
    {
        public void BindEvent<T>(EventHandler<EventArgsBase> callback)where T:EventArgsBase;
        public void UnBindEvent<T>( EventHandler<EventArgsBase> callback)where T:EventArgsBase;
        public void SendEvent(object sender, EventArgsBase eventArgs);
    }
}