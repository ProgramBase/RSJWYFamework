using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RSJWYFamework.Runtime.Module;
using UnityEngine;

namespace RSJWYFamework.Runtime.ReferencePool
{
    public class ReferencePoolManager:IModule
    {
        /// <summary>
        /// 数量上限
        /// </summary>
        [SerializeField]
        private int _limit = 100;

        /// <summary>
        /// 所有引用池
        /// </summary>
        private ConcurrentDictionary<Type, ReferencePool> SpawnPools=new ();

        /// <summary>
        /// 获取一个引用
        /// </summary>
        /// <typeparam name="T">继承自IReference的必须包含一个无参构造函数</typeparam>
        /// <returns></returns>
        public T Get<T>() where T : class, IReference, new()
        {
            var type = typeof(T);
            if (!SpawnPools.ContainsKey(type))
            {
                SpawnPools.TryAdd(type, new ReferencePool(_limit));
            }
            return SpawnPools[type].Get<T>();
        }

        /// <summary>
        /// 回收一个引用
        /// </summary>
        /// <param name="refe"></param>
        public void Release(IReference refe)
        {
            if (refe == null)
                return;

            Type type = refe.GetType();
            if (!SpawnPools.ContainsKey(type))
            {
                SpawnPools.TryAdd(type, new ReferencePool(_limit));
            }

            SpawnPools[type].Release(refe);
        }
        
        
        
        
        public void Init()
        {
        }

        public void Close()
        {
        }

        public void Update(float time, float deltaTime)
        {
        }

        public void UpdatePerSecond(float time)
        {
        }
    }
}