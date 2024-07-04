using System;
using System.Collections.Concurrent;
using MyFamework.Runtime.Base;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Module;
using UnityEngine;

namespace RSJWYFamework.Runtime.Main
{
    public sealed partial class Main:SingletonBase<Main>
    {
        public Main()
        {
            AddModule<DefaultEvenManager>();
            RSJWYLogger.Log(RSJWYFameworkEnum.Main,"初始化完成");
        }

        public void Init()
        {
            
        }
        
        private ConcurrentDictionary<Type, IModule> _modules = new();

        /// <summary>
        /// 获取模块
        /// </summary>
        /// <typeparam name="T">模块类，从IModule继承</typeparam>
        /// <returns></returns>
        public T GetModule<T>()where T: class, IModule
        {
            var type = typeof(T);
            if (_modules.TryGetValue(type, out var module))
            {
                return module as T;
            }
            else
            {
                RSJWYLogger.LogError($"{RSJWYFameworkEnum.Main}:{type}模块不存在");
                return null;
            }
            /*
            else
            {
                RSJWYLogger.LogWarning($"{RSJWYFameworkEnum.Main}:{type}没有在字典内，将进行实例化添加");
                AddModule<T>
            }*/
        }
        /// <summary>
        /// 添加模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddModule<T>( ) where T : IModule, new()
        {
            var type = typeof(T);
            var _t = new T();
            if (_modules.TryAdd(type, _t))
            {
                _t.Init();
                RSJWYLogger.Log(RSJWYFameworkEnum.Main,$"添加模块{type}成功");
            }
            else
            {
                _t = default(T); 
                RSJWYLogger.LogWarning($"{RSJWYFameworkEnum.Main}:添加{type}模块失败，似乎已添加相同的模块，将释放本次实例化内容");
            }
            return _t;
        }
        /// <summary>
        /// 关闭所有模块
        /// </summary>
        public void CloseAllModule()
        {
            foreach (var VARIABLE in _modules)
            {
                VARIABLE.Value.Close();
            }
        }
    }
    
    

    public enum RSJWYFameworkEnum
    {
        NetworkTcpServer,
        NetworkTcpClient,
        NetworkUDP,
        NetworkTool,
        Main,
        ExceptionLogManager,
        Utility,
        SenseShield
    }
}