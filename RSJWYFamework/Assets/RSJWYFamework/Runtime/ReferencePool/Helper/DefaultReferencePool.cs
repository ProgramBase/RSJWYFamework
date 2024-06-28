using System;
using System.Collections.Generic;
using HT.Framework;
using MyFamework.Runtime.Base;
using RSJWYFamework.Runtime.Base;
using RSJWYFamework.Runtime.ReferencePool.Base;

namespace RSJWYFamework.Runtime.ReferencePool.Helper
{
    /// <summary>
    /// 默认的引用池管理器助手
    /// </summary>
    internal sealed class DefaultReferencePool : SingletonBase<DefaultReferencePool>,IReferencePool
    {
        /// <summary>
        /// 对象池默认上限
        /// </summary>
        private int _limit;
        /// <summary>
        /// 所有引用池
        /// </summary>
        public Dictionary<Type, ReferenceSpawnPool> SpawnPools { get; private set; } = new Dictionary<Type, ReferenceSpawnPool>();
        
        /// <summary>
        /// 获取引用池中引用数量
        /// </summary>
        /// <param name="type">引用类型</param>
        /// <returns>引用数量</returns>
        public int GetPoolCount(Type type)
        {
            if (SpawnPools.ContainsKey(type))
            {
                return SpawnPools[type].Count;
            }
            else
            {
                throw new RSJWYFameworkException($"获取引用数量失败：不存在引用池 {type.FullName} ！");
            }
        }
        /// <summary>
        /// 生成引用
        /// </summary>
        /// <typeparam name="T">引用类型</typeparam>
        /// <returns>对象</returns>
        public T Spawn<T>() where T : class, IReference, new()
        {
            Type type = typeof(T);
            if (!SpawnPools.ContainsKey(type))
            {
                SpawnPools.Add(type, new ReferenceSpawnPool(_limit));
            }
            return SpawnPools[type].Spawn<T>();
        }
        /// <summary>
        /// 生成引用
        /// </summary>
        /// <param name="type">引用类型</param>
        /// <returns>对象</returns>
        public IReference Spawn(Type type)
        {
            if (!SpawnPools.ContainsKey(type))
            {
                SpawnPools.Add(type, new ReferenceSpawnPool(_limit));
            }
            return SpawnPools[type].Spawn(type);
        }
        /// <summary>
        /// 回收引用
        /// </summary>
        /// <param name="refe">对象</param>
        public void Despawn(IReference refe)
        {
            if (refe == null)
                return;

            Type type = refe.GetType();
            if (!SpawnPools.ContainsKey(type))
            {
                SpawnPools.Add(type, new ReferenceSpawnPool(_limit));
            }

            SpawnPools[type].Despawn(refe);
        }
        /// <summary>
        /// 批量回收引用
        /// </summary>
        /// <typeparam name="T">引用类型</typeparam>
        /// <param name="refes">对象集合</param>
        public void Despawns<T>(List<T> refes) where T : class, IReference, new()
        {
            if (refes == null)
                return;

            Type type = typeof(T);
            if (!SpawnPools.ContainsKey(type))
            {
                SpawnPools.Add(type, new ReferenceSpawnPool(_limit));
            }

            for (int i = 0; i < refes.Count; i++)
            {
                SpawnPools[type].Despawn(refes[i]);
            }
            refes.Clear();
        }
        /// <summary>
        /// 批量回收引用
        /// </summary>
        /// <typeparam name="T">引用类型</typeparam>
        /// <param name="refes">对象数组</param>
        public void Despawns<T>(T[] refes) where T : class, IReference, new()
        {
            if (refes == null)
                return;

            Type type = typeof(T);
            if (!SpawnPools.ContainsKey(type))
            {
                SpawnPools.Add(type, new ReferenceSpawnPool(_limit));
            }

            for (int i = 0; i < refes.Length; i++)
            {
                SpawnPools[type].Despawn(refes[i]);
            }
        }
        /// <summary>
        /// 清空指定的引用池
        /// </summary>
        /// <param name="type">引用类型</param>
        public void Clear(Type type)
        {
            if (SpawnPools.ContainsKey(type))
            {
                SpawnPools[type].Clear();
            }
        }
        /// <summary>
        /// 清空所有引用池
        /// </summary>
        public void ClearAll()
        {
            foreach (var spawnPool in SpawnPools)
            {
                spawnPool.Value.Clear();
            }
        }
    }
}