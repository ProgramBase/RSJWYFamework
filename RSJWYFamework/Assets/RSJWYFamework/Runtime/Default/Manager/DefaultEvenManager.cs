using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MyFamework.Runtime.Base;
using RSJWYFamework.Runtime.Event;
using RSJWYFamework.Runtime.Module;
using UnityEngine.Assertions;

namespace RSJWYFamework.Runtime.Default.Manager
{
    /// <summary>
    /// 默认事件系统
    /// </summary>
    public class DefaultEvenManager :IEventManage,IModule
    {
        ConcurrentDictionary<Type, EventHandler<EventArgsBase>> CallBackDic = new();
        /// <summary>
        /// 绑定
        /// </summary>
        /// <param name="eventID">事件类（ID）</param>
        /// <param name="callback">事件回调</param>
        public void BindEvent<T>( EventHandler<EventArgsBase> callback)where T:EventArgsBase
        {
            var type = typeof(T);
            //检测值是否为null
            Assert.IsNotNull(callback);
            //寻找本ID是否绑定过事件
            if (CallBackDic.ContainsKey(type))
            {
                //CallBackDic[eventID] +=CallBackDic;
                CallBackDic[type] += callback;
            }
            else
            {
                //没绑定过则创建
                EventHandler<EventArgsBase> temp = callback;
                CallBackDic.TryAdd(type, temp);
            }
        }
        /// <summary>
        /// 解除绑定
        /// </summary>
        /// <param name="eventID">事件类（ID）</param>
        /// <param name="callback">事件回调</param>
        public void UnBindEvent<T>(EventHandler<EventArgsBase> callback)where T:EventArgsBase
        {
            var type = typeof(T);
            Assert.IsNotNull(callback);
            var remove = false;
            if (CallBackDic.ContainsKey(type))
            {
                CallBackDic[type] -= callback;
                //检查handle是否为空，获得true/false
                remove = CallBackDic[type] == null;
            }
            else
            {
                throw new Exception($"{this.GetType().Name} 解绑失败");
            }
            
            if (remove)
                CallBackDic.TryRemove(type,out _);
        }
        /// <summary>
        /// 调用事件
        /// </summary>
        /// <param name="sender">消息来源类</param>
        /// <param name="eventArgs">消息载体</param>
        public void SendEvent(object sender, EventArgsBase eventArgs)
        {
            if (CallBackDic.TryGetValue(eventArgs.GetType(), out var handler))
            {
                handler?.Invoke(sender, eventArgs);
            }
        }

        public void Init()
        {
            
        }

        public void Close()
        {
            CallBackDic.Clear();
        }
    }
}