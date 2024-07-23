using System;
using System.Collections.Concurrent;
using MyFamework.Runtime.Base;
using RSJWYFamework.Runtime.AsyncOperation;
using RSJWYFamework.Runtime.Data;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Driver;
using RSJWYFamework.Runtime.Event;
using RSJWYFamework.Runtime.HybridCLR;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Module;
using RSJWYFamework.Runtime.YooAssetModule;
using UnityEngine;

namespace RSJWYFamework.Runtime.Main
{
    public static partial class Main
    {
        public static bool IsInitialize { get; private set; } = false;
        private static GameObject _driver = null;
        private static ConcurrentDictionary<RSJWYFameworkEnum, IModule> _modules = new();
        
        public static DefaultEvenManager EventModle { get; private set; }
        public static DefaultYooAssetManager YooAssetManager{get; private set; }
        public static DataManager DataManagerataManager{get; private set; }
        public static DefaultHybirdCLRManager HybridClrManager { get; private set; }
        
        public static DefaultExceptionLogManager ExceptionLogManager { get; private set; }
        
        public static RAsyncOperationSystem RAsyncOperationSystem{get; private set; }
        

        /// <summary>
        /// 初始化框架服务
        /// </summary>
        public static void Initialize()
        {
            if (IsInitialize)
            {
                UnityEngine.Debug.LogWarning($"{nameof(Main)} is initialized !");
                return;
            }
            else
            {
                // 创建驱动器
                _driver = new GameObject($"[RSJWYFameworkServiceDriver]");
                _driver.AddComponent<RSJWYFameworkDriverService>();
                UnityEngine.Object.DontDestroyOnLoad(_driver);
                IsInitialize = true;
                RSJWYLogger.Log(RSJWYFameworkEnum.Main,"初始化完成");
                //添加基础的模块
                EventModle = (DefaultEvenManager)AddModule<DefaultEvenManager>(RSJWYFameworkEnum.Event);
                DataManagerataManager = (DataManager)AddModule<DataManager>(RSJWYFameworkEnum.Data);
                YooAssetManager = (DefaultYooAssetManager)AddModule<DefaultYooAssetManager>(RSJWYFameworkEnum.YooAssets);
                HybridClrManager = (DefaultHybirdCLRManager)AddModule<DefaultHybirdCLRManager>(RSJWYFameworkEnum.HybridCLR);
                RAsyncOperationSystem =
                    (RAsyncOperationSystem)AddModule<RAsyncOperationSystem>(RSJWYFameworkEnum.RAsyncOperationSystem);
            }
            
        }

        /// <summary>
        /// 每帧更新
        /// 勿在此执行高耗时应用
        /// </summary>
        /// <param name="time">自启动后已经经过的时间</param>
        /// <param name="deltaTime">自上一帧完成以来经过的时间量</param>
        internal static void Update(float time,float deltaTime)
        {
            if (IsInitialize)
            {
                foreach (var module in _modules)
                {
                    module.Value.Update(time,deltaTime);
                }
            }
        }
        /// <summary>
        /// 每秒更新时间
        /// </summary>
        /// <param name="time">自启动后已经经过的时间</param>
        internal static void UpdatePerSecond(float time)
        {
            if (IsInitialize)
            {
                foreach (var module in _modules)
                {
                    module.Value.UpdatePerSecond(time);
                }
            }
        }
        
        /// <summary>
        /// 获取模块
        /// </summary>
        /// <typeparam name="T">模块类，从IModule继承</typeparam>
        /// <returns></returns>
        public static IModule GetModule(RSJWYFameworkEnum emodule)
        {
            if (_modules.TryGetValue(emodule, out var module))
            {
                return module;
            }
            else
            {
                RSJWYLogger.Error($"{RSJWYFameworkEnum.Main}:{emodule}模块不存在");
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
        public static IModule AddModule<T>(RSJWYFameworkEnum emodule) where T : class,IModule,new()
        {
            IModule module = new T();
            if (_modules.TryAdd(emodule, module))
            {
                module.Init();
                RSJWYLogger.Log(RSJWYFameworkEnum.Main,$"添加模块{emodule}成功！为{module}");
            }
            else
            {
                module = _modules[emodule]; 
                RSJWYLogger.Warning($"{RSJWYFameworkEnum.Main}:添加{emodule}模块失败，似乎已添加相同的模块");
            }
            return module;
        }
        /// <summary>
        /// 关闭所有模块
        /// </summary>
        public static void CloseAllModule()
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
        SenseShield,
        Procedure,
        EditorTool,
        Data,
        YooAssets,
        ReferencePool,
        HybridCLR,
        Event,
        RAsyncOperationSystem,
        GameObjectPool,
    }
}