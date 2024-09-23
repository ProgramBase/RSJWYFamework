using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.NetWork.TCP.Server;
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
        /// <returns></returns>
        public SocketAsyncEventArgs Get(ClientSocketToken token)
        {
            var item= base.Get();
            item.UserToken = token;
            return item;
        }

        
    }
}