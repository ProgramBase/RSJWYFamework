using System;
using System.Collections.Generic;
using RSJWYFamework.Runtime.AsyncOperation;
using RSJWYFamework.Runtime.Data;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Driver;
using RSJWYFamework.Runtime.Event;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Module;
using RSJWYFamework.Runtime.ReferencePool;
using RSJWYFamework.Runtime.StateMachine;
using RSJWYFamework.Runtime.YooAssetModule;
using UnityEngine;
using UnityEngine.Serialization;

namespace RSJWYFamework.Runtime.Main
{
    public  class Main:MonoBehaviour
    {
        
        public static bool IsInitialize { get; private set; } = false;
        /// <summary>
        /// 模块字典
        /// </summary>
        private static Dictionary<Type, IModule> _modules = new();
        
        /// <summary>
        /// 生命周期
        /// </summary>
        public static List<ILife> _life = new();
        /// <summary>
        /// 事件管理器
        /// </summary>
        public static EvenManager EventModle { get; private set; }
        /// <summary>
        /// 资源管理器
        /// </summary>
        public static YooAssetManager YooAssetManager{get; private set; }
        /// <summary>
        /// 数据管理器
        /// </summary>
        public static DataManager DataManagerataManager{get; private set; }
        /// <summary>
        /// 热更新管理器
        /// </summary>
        public static HybirdCLRManager HybridClrManager { get; private set; }
        /// <summary>
        /// 异步系统
        /// </summary>
        public static RAsyncOperationSystem RAsyncOperationSystem{get; private set; }
        /// <summary>
        /// 引用池
        /// </summary>
        public static ReferencePoolManager ReferencePoolManager {get; private set; }
        /// <summary>
        /// 流程管理器
        /// </summary>
        public static StateMachineControllerExecuteQueue StateMachineControllerExecuteQueue { get; private set; }

        [SerializeField]
        RSJWYFameworkDriverService service;
        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (IsInitialize)
            {
                UnityEngine.Debug.LogWarning($"{nameof(Main)} is initialized !");
            }
            else
            {
                //添加基础的模块
                EventModle = (EvenManager)AddModule<EvenManager>();
                DataManagerataManager = (DataManager)AddModule<DataManager>();
                YooAssetManager = (YooAssetManager)AddModule<YooAssetManager>();
                HybridClrManager = (HybirdCLRManager)AddModule<HybirdCLRManager>();
                RAsyncOperationSystem = (RAsyncOperationSystem)AddModule<RAsyncOperationSystem>();
                ReferencePoolManager = (ReferencePoolManager)AddModule<ReferencePoolManager>();
                StateMachineControllerExecuteQueue= (StateMachineControllerExecuteQueue)AddModule<StateMachineControllerExecuteQueue>();
                IsInitialize = true;
                RSJWYLogger.Log(RSJWYFameworkEnum.Main,"初始化完成");
            }
        }

        #region 模块

        /// <summary>
        /// 获取模块
        /// </summary>
        /// <typeparam name="T">模块类，从IModule继承</typeparam>
        /// <returns></returns>
        public static IModule GetModule<T>()where T:class,IModule
        {
            var t = typeof(T);
            if (_modules.TryGetValue(t, out var module))
            {
                return module;
            }
            else
            {
                RSJWYLogger.Error($"{RSJWYFameworkEnum.Main} GetModule:{t}模块不存在!");
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
        /// 添加模块时将检查是否继承了ILife，将会自动添加
        /// 如果模块重复添加，将会出发warn，并返回已添加的相同类型模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IModule AddModule<T>() where T : class,IModule,new()
        {
            var t = typeof(T);
            IModule module = new T();
            if (_modules.TryAdd(t, module))
            {
                module.Init();
                RSJWYLogger.Log(RSJWYFameworkEnum.Main,$"添加模块{t}成功！");
                if (module is ILife)
                    AddLife(module as ILife);
            }
            else
            {
                module = _modules[t]; 
                RSJWYLogger.Warning($"{RSJWYFameworkEnum.Main}:添加{t}模块失败，似乎已添加相同的模块");
            }
            return module;
        }
        /// <summary>
        /// 移除模块
        /// </summary>
        /// <typeparam name="T">模块类，从IModule继承</typeparam>
        /// <returns></returns>
        public static void RemoveModule<T>()where T:class,IModule
        {
            var t = typeof(T);
            if (_modules.ContainsKey(t))
            {
                var module = _modules[t];
                module.Close();
                if (module is ILife)
                    RemoveLife(module as ILife);
                _modules.Remove(t);
                RSJWYLogger.Log(RSJWYFameworkEnum.Main,$"RemoveModule 移除模块{t}成功！");
            }
        }
        #endregion

        #region 生命周期

        /// <summary>
        /// 添加生命周期
        /// </summary>
        /// <returns></returns>
        public static void AddLife(ILife life)
        {
            if (_life.Contains(life))
            {
                RSJWYLogger.Warning(RSJWYFameworkEnum.Main,$"AddLife 已经添加了一个相同的实例");
            }
            else
            {
                // 获取新添加实例的优先级
                uint newLifePriority = life.Priority();

                // 找到新实例应该插入的位置
                int insertIndex = 0;
                while (insertIndex < _life.Count && _life[insertIndex].Priority() < newLifePriority)
                {
                    insertIndex++;
                }
                // 在找到的位置插入新实例
                _life.Insert(insertIndex, life);
                _life.Add(life);
                RSJWYLogger.Log(RSJWYFameworkEnum.Main,$"AddLife 添加{life}完成");
            }
        }
        /// <summary>
        /// 移除
        /// </summary>
        /// <returns></returns>
        public static void RemoveLife(ILife life)
        {
            if (_life.Contains(life))
            {
                _life.Remove(life);
                RSJWYLogger.Log(RSJWYFameworkEnum.Main,$"RemoveLife 移除{life}完成");
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
                foreach (var life in _life)
                {
                    life.Update(time,deltaTime);
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
                foreach (var life in _life)
                {
                    life.UpdatePerSecond(time);
                }
            }
        }
        /// <summary>
        /// 每0.02s更新
        /// </summary>
        internal static void FixedUpdate()
        {
            if (IsInitialize)
            {
                foreach (var life in _life)
                {
                    life.FixedUpdate();
                }
            }
        }
        
        internal static void LateUpdate()
        {
            if (IsInitialize)
            {
                foreach (var life in _life)
                {
                    life.LateUpdate();
                }
            }
        }
        #endregion
        /// <summary>
        /// 关闭所有模块
        /// </summary>
        public static void CloseAllModule()
        {
            foreach (var VARIABLE in _modules)
            {
                VARIABLE.Value.Close();
            }
            _modules.Clear();
            _life.Clear();
        }
    }
    
    

    public enum RSJWYFameworkEnum
    {
        None,
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
        Pool,
    }
}