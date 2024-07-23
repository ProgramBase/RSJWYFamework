using System;
using System.Collections.Generic;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Main;
using UnityEngine;

namespace RSJWYFamework.Runtime.ObjectPool
{
    /// <summary>
    /// 对象池
    /// </summary>
    public sealed class ObjectSpawnPool<T>where T:MonoBehaviour,new ()
    {
        /// <summary>
        /// 模板
        /// </summary>
        private T _spawnTem;
        
        /// <summary>
        /// 数量上限
        /// </summary>
        private int _limit = 100;
        
        /// <summary>
        /// 池子
        /// </summary>
        private Queue<T> _objectQueue = new ();

        /// <summary>
        /// 池子里当前未使用物体数量
        /// </summary>
        public int Count => _objectQueue.Count;
        /// <summary>
        /// 创建物体时回调
        /// </summary>
        private Func<T> _onCreate;
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
        /// </summary>
        /// <param name="spawnTem">模板</param>
        /// <param name="limit">初始化数量</param>
        /// <param name="parent">池子实例化时的父类</param>
        public ObjectSpawnPool(T spawnTem, int limit,
            Func<T> onCreate,Action<T> onDestroy,Action<T> onGet,Action<T> onRelease)
        {
            _spawnTem = spawnTem;
            _limit = limit;
           _onCreate = onCreate;
           _onDestroy = onDestroy;
           _onGet = onGet;
           _onRelease = onRelease;
        }
        /// <summary>
        /// 获取一个对象池内的对象
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            GameObject obj;
            if (_objectQueue.Count > 0)
            {
                obj = _objectQueue.Dequeue().gameObject;
            }
            else
            {
                obj = _onCreate.Invoke().gameObject;
                obj.AddComponent<T>();
            }
            _onGet?.Invoke(obj.GetComponent<T>());
            return obj.GetComponent<T>();
        }

        /// <summary>
        /// 回收一个对象
        /// </summary>
        public void Release(T Object)
        {
            if (Object == null)
                throw new RSJWYException(RSJWYFameworkEnum.GameObjectPool,"试图放入一个空对象到对象池中");
            if (_objectQueue.Count<_limit)
            {
                _onRelease?.Invoke(Object);
                _objectQueue.Enqueue(Object);
            }
            else
            {
                _onDestroy?.Invoke(Object);
                Utility.Utility.GameObjectTool.Destory(Object.gameObject);
            }
        }
        
        /// <summary>
        /// 清空所有对象
        /// </summary>
        public void Clear()
        {
            while (_objectQueue.Count > 0)
            {
                GameObject obj = _objectQueue.Dequeue().gameObject;
                if (obj)
                {
                    Utility.Utility.GameObjectTool.Destory(obj);
                }
            }
        }
    }
}