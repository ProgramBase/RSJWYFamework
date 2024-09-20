using System;
using System.Collections.Concurrent;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Main;

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
        private ConcurrentStack<IReference> _referenceQueue = new ();
        /// <summary>
        /// 池子里当前未使用引用数量
        /// </summary>
        public int Count => _referenceQueue.Count;

        /// <summary>
        /// 记录当前池存储的类型
        /// </summary>
        private Type _ReferenceType;
        
        public ReferencePool(int limit,Type ReType)
        {
            _limit = limit;
            _ReferenceType = ReType;
        }

        /// <summary>
        /// 获取一个引用
        /// </summary>
        /// <typeparam name="T">继承自IReference的无参构造类</typeparam>
        /// <returns></returns>
        public IReference Get<T>()where T : class, IReference, new()
        {
            if (typeof(T) != _ReferenceType)
                throw new RSJWYException(RSJWYFameworkEnum.ReferencePool,$"栈存储的是{_ReferenceType}，而要获取的是{typeof(T)}");
            if (_referenceQueue.TryPop(out var popRef))
            {
                return popRef as T;
            }
            else
            {
                return Activator.CreateInstance<T>();
            }
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
                refe.Release();
                _referenceQueue.Push(refe);
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