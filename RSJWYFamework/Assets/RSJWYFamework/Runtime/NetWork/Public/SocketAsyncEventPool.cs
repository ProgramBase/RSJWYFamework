using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Pool;

namespace RSJWYFamework.Runtime.NetWork.Public
{
    /// <summary>
    /// 针对于SocketAsyncEventArgs特殊的对象池
    /// </summary>
    public sealed class SocketAsyncEventPool: ObjectPool<SocketAsyncEventArgs>
    {
        public SocketAsyncEventPool(Action<SocketAsyncEventArgs> onCreate, Action<SocketAsyncEventArgs> onDestroy, 
            Action<SocketAsyncEventArgs> onGet, Action<SocketAsyncEventArgs> onRelease, int limit, int initCount) 
            : base(onCreate, onDestroy, onGet, onRelease, limit, initCount)
        {
            
        }

        /// <summary>
        /// 获取时配置完成要操作的数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        public SocketAsyncEventArgs Get(byte[] bytes,int offset,int Length)
        {
            if (_objectQueue.TryPop(out var popitem))
            {
                popitem.SetBuffer(bytes,offset,Length);
                return popitem;
            }
            else
            {
                var item= new SocketAsyncEventArgs();
                _onCreate?.Invoke(item);
                item.SetBuffer(bytes,offset,Length);
                return item;
            }
        }

        
    }
}