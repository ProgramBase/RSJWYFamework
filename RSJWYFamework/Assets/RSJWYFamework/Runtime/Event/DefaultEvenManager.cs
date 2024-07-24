﻿using System;
using System.Collections.Concurrent;
using RSJWYFamework.Runtime.Module;
using UnityEngine.Assertions;

namespace RSJWYFamework.Runtime.Event
{
    /// <summary>
    /// 默认事件系统
    /// </summary>
    public class DefaultEvenManager:IModule
    {
        /// <summary>
        /// 订阅者列表
        /// </summary>
        private readonly ConcurrentDictionary<Type, EventHandler<EventArgsBase>> _callBackDic = new();
        /// <summary>
        /// 多线程下的消息队列
        /// </summary>
        private readonly ConcurrentQueue<EventArgsBase> _callQueue = new();
        /// <summary>
        /// 绑定
        /// </summary> 
        /// <param name="callback">事件回调</param>
        public void BindEvent<T>( EventHandler<EventArgsBase> callback)where T:EventArgsBase
        {
            var type = typeof(T);
            //检测值是否为null
            Assert.IsNotNull(callback);
            //寻找本ID是否绑定过事件
            if (_callBackDic.ContainsKey(type))
            {
                //CallBackDic[eventID] +=CallBackDic;
                _callBackDic[type] += callback;
            }
            else
            {
                //没绑定过则创建
                EventHandler<EventArgsBase> temp = callback;
                _callBackDic.TryAdd(type, temp);
            }
        }
        /// <summary>
        /// 解除绑定
        /// </summary>
        /// <param name="callback">事件回调</param>
        public void UnBindEvent<T>(EventHandler<EventArgsBase> callback)where T:EventArgsBase
        {
            var type = typeof(T);
            Assert.IsNotNull(callback);
            var remove = false;
            if (_callBackDic.ContainsKey(type))
            {
                _callBackDic[type] -= callback;
                //检查handle是否为空，获得true/false
                remove = _callBackDic[type] == null;
            }
            else
            {
                throw new Exception($"{this.GetType().Name} 解绑失败");
            }
            
            if (remove)
                _callBackDic.TryRemove(type,out _);
        }
        /// <summary>
        /// 广播事件，不进入队列直接广播
        /// 禁止多线程调用，请调用Fire
        /// </summary>
        /// <param name="eventArgs">消息载体</param>
        public void FireNow(EventArgsBase eventArgs)
        {
            if (_callBackDic.TryGetValue(eventArgs.GetType(), out var handler))
            {
                handler?.Invoke(eventArgs.Sender, eventArgs);
            }
        }
        /// <summary>
        /// 广播事件，进入队列进行广播，每帧调用一次
        /// 适合多线程下的广播
        /// </summary>
        /// <param name="eventArgs"></param>
        public void Fire(EventArgsBase eventArgs)
        {
            _callQueue.Enqueue(eventArgs);
        }

        public void Init()
        {
            
        }

        public void Close()
        {
            _callBackDic.Clear();
            _callQueue.Clear();
        }

        public void Update(float time, float deltaTime)
        {
            if (_callQueue.IsEmpty)
                return;
            _callQueue.TryDequeue(out var _call);
            Fire(_call);
        }

        public void UpdatePerSecond(float time)
        {
            
        }
    }
    
    
}