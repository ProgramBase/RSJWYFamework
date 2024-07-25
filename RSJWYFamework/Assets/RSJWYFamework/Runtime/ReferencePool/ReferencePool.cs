using System;
using System.Collections.Concurrent;

namespace RSJWYFamework.Runtime.ReferencePool
{
    public class ReferencePool
    {
        /// <summary>
        /// 数量上限
        /// </summary>
        private int _limit = 100;
        /// <summary>
        /// 引用队列
        /// </summary>
        private ConcurrentQueue<IReference> _referenceQueue = new ();
        /// <summary>
        /// 池子里当前未使用引用数量
        /// </summary>
        public int Count => _referenceQueue.Count;
        
        public ReferencePool(int limit)
        {
            _limit = limit;
        }

        /// <summary>
        /// 获取一个引用
        /// </summary>
        /// <typeparam name="T">继承自IReference的无参构造类</typeparam>
        /// <returns></returns>
        public T Get<T>()where T : class, IReference, new()
        {
            T refe;
            if (_referenceQueue.Count > 0)
            {
                _referenceQueue.TryDequeue(out var _a);
                refe = _a as T;
            }
            else
            {
                refe = new T();
            }
            return refe;
        }

        /// <summary>
        /// 回收一个引用
        /// </summary>
        /// <param name="refe"></param>
        public void Release(IReference refe)
        {
            if (_referenceQueue.Count >= _limit)
            {
                refe = null;
            }
            else
            {
                refe.Reset();
                _referenceQueue.Enqueue(refe);
            }
        }
        /// <summary>
        /// 清空所有引用
        /// </summary>
        public void Clear()
        {
            _referenceQueue.Clear();
        }
    }
}