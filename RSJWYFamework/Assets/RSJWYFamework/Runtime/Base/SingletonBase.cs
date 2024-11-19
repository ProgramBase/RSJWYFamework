using System;
using UnityEngine;

namespace RSJWYFamework.Runtime.Base
{
    /// <summary>
    /// 单例模式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonBase<T> where T : class, new()
    {
        private static T _instance;
        private static readonly object _lockObject = new object();
 
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                        }
                    }
                }
                return _instance;
            }
        }
 
        // 防止子类直接调用构造函数
        protected SingletonBase()
        {
            if (_instance != null)
            {
                throw new Exception("单例类不应该直接被实例化。请使用Singleton.Instance访问实例。");
            }
        }
    }
    public abstract class SingletonBaseMono<T>:MonoBehaviour where T : SingletonBaseMono<T>
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (Singleton)";
                    }
                }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = (T)this;
            }
        }

        protected virtual void OnDestroy()
        {
            _instance = null;
        }
    }
}