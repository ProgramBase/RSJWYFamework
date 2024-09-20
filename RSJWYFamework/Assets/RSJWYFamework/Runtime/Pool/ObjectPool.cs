using System;
using System.Collections.Concurrent;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Main;

namespace RSJWYFamework.Runtime.Pool
{
    /// <summary>
    /// 引用池
    /// </summary>
    public sealed class ObjectPool<T>where T:class,new()
    {
        /// <summary>
        /// 数量上限
        /// </summary>
        private int _limit = 100;
        
        /// <summary>
        /// 池子
        /// </summary>
        private ConcurrentStack<T> _objectQueue = new ();

        /// <summary>
        /// 池子里当前未使用物体数量
        /// </summary>
        public int Count => _objectQueue.Count;
        /// <summary>
        /// 创建物体时回调
        /// </summary>
        private Action<T> _onCreate;
        /// <summary>
        /// 销毁物体时回调
        /// </summary>
        private Action<T> _onDestroy;
        /// <summary>
        /// 获取物体时回调
        /// </summary>
        private Action<T> _onGet;
        /// <summary>
        /// 回收物体时回调
        /// </summary>
        private Action<T> _onRelease;
        
        /// <summary>
        /// 创建池
        /// 创建时，如果初始数量大于最大数量，将以最大数量来初始化（取这两值最小值）
        /// </summary>
        /// <param name="limit">最大数量限制</param>
        /// <param name="initCount">初始化数量</param>
        /// <param name="onCreate">创建时执行的事件</param>
        /// <param name="onDestroy">销毁时执行的事件</param>
        /// <param name="onGet">获取时执行的事件</param>
        /// <param name="onRelease">回收物体时执行的回调</param>
        public ObjectPool(Action<T> onCreate,Action<T> onDestroy,Action<T> onGet,Action<T> onRelease,
            int limit,int initCount)
        {
            _limit = limit;
           _onCreate = onCreate;
           _onDestroy = onDestroy;
           _onGet = onGet;
           _onRelease = onRelease;

           var max = Math.Min(limit,initCount);
           for (int i = 0; i < max; i++)
           {
               var _obj= new T();
               _onCreate?.Invoke(_obj);
               _onRelease?.Invoke(_obj);
           }
        }
        /// <summary>
        /// 获取一个对象池内的对象
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            if (_objectQueue.TryPop(out var popObj))
            {
                _onGet?.Invoke(popObj);
                return popObj;
            }
            else
            {
                var _obj= new T();
                _onCreate?.Invoke(_obj);
                _onGet?.Invoke(_obj);
                return _obj;
            }
        }

        /// <summary>
        /// 回收一个对象
        /// </summary>
        public void Release(T Obj)
        {
            if (Obj == null)
                throw new RSJWYException(RSJWYFameworkEnum.Pool,"试图放入一个空对象到对象池中");
            if (_objectQueue.Count<_limit)
            {
                _onRelease?.Invoke(Obj);
                _objectQueue.Push(Obj);
            }
            else
            {
                _onDestroy?.Invoke(Obj);
                Obj = null;
            }
        }
        
        /// <summary>
        /// 清空所有对象
        /// </summary>
        public void Clear()
        {
            while (_objectQueue.TryPop(out var _obj))
            {
                _obj = null;
            }
            _objectQueue.Clear();
        }
    }
}