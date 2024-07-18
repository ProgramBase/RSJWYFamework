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
    public sealed partial class Main
    {
        public static bool IsInitialize { get; private set; } = false;
        private static GameObject _driver = null;
        private static ConcurrentDictionary<Type, IModule> _modules = new();
        
        public static IEventManage EventModle { get; private set; }
        public static DefaultYooAssetManager YooAssetManager{get; private set; }
        public static DefaultDataManager DataManagerataManager{get; private set; }
        public static DefaultHybirdCLRManager HybridClrManager { get; private set; }
        
        public static RAsyncOperationSystem AsyncOperation{ get; private set; }

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
                EventModle = Main.AddModule<IEventManage>(new DefaultEvenManager()) as DefaultEvenManager;
                DataManagerataManager = Main.AddModule<IDataManager>(new DefaultDataManager()) as DefaultDataManager;
                YooAssetManager= Main.AddModule<IYooAssetManager>(new DefaultYooAssetManager())as DefaultYooAssetManager;
                HybridClrManager = Main.AddModule<IHybridCLRManager>(new DefaultHybirdCLRManager()) as DefaultHybirdCLRManager;
                AsyncOperation = AddModule<IRAsyncAsyncOperationSystem>(new RAsyncOperationSystem()) as RAsyncOperationSystem;
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
        public static T GetModule<T>()where T: class,ModleInterface
        {
            var type = typeof(T);
            if (_modules.TryGetValue(type, out var module))
            {
                return module as T;
            }
            else
            {
                RSJWYLogger.Error($"{RSJWYFameworkEnum.Main}:{type}模块不存在");
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
        public static T AddModule<T>(IModule module ) where T : ModleInterface
        {
            var type = typeof(T);
            if (_modules.TryAdd(type, module))
            {
                module.Init();
                RSJWYLogger.Log(RSJWYFameworkEnum.Main,$"添加模块{type}成功");
            }
            else
            {
                module = _modules[typeof(T)]; 
                RSJWYLogger.Warning($"{RSJWYFameworkEnum.Main}:添加{type}模块失败，似乎已添加相同的模块，将释放本次实例化内容");
            }
            return (T)module;
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
        ReferencePool
    }
}