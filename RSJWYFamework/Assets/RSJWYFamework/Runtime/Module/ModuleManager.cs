using System;
using System.Collections.Generic;
using MyFamework.Runtime.Base;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Main;

namespace RSJWYFamework.Runtime.Module
{
    /// <summary>
    /// 模块管理器
    /// </summary>
    public static class ModuleManager
    {
        /// <summary>
        /// 模块字典
        /// </summary>
        /// <typeparam name="Type">模块类型</typeparam>
        /// <typeparam name="IModule">模块的具体实现</typeparam>
        /// <returns></returns>
        public static Dictionary<Type, IModule> Modules = new Dictionary<Type, IModule>();
        /// <summary>
        /// 设置模块(添加)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void SetModule<T>(IModule module) where T : class
        {
            //检查模块是否已存在
            if (!Modules.ContainsKey(typeof(T)))
            {
                Modules.Add(typeof(T), module);
            }
            else
            {
                throw new RSJWYException(RSJWYFameworkEnum.Main,$"模块管理器:设置模块重复");
            }
        }
        /// <summary>
        /// 获取模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static T GetModule<T>() where T : class
        {
            //尝试获取，获取成功为true
            if (Modules.TryGetValue(typeof(T), out var module))
            {
                return (T)module;
            }

            return null;
        }
        /// <summary>
        /// 移除模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static bool RemoveModule<T>() where T : class
        {
            //获取类型
            var type = typeof(T);
            if (Modules.ContainsKey(type))
            {
                Modules.Remove(type);
                return true;
            }

            return false;
        }

        #region 生命周期

        public static void OnUpdate(float time, float realtime)
        {
            foreach (var keyValuePair in Modules)
            {
                if (keyValuePair.Value is ILife life)
                    life.OnUpdate(time, realtime);
            }
        }

        public static void OnClose()
        {
            foreach (var keyValuePair in Modules)
            {
                if (keyValuePair.Value is ILife life){}
                    //life.OnClose();
            }

            Modules.Clear();
        }

        #endregion
    }
}