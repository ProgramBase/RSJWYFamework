using System;
using MyFamework.Runtime.Base;
using RSJWYFamework.Runtime.Data;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Event;
using RSJWYFamework.Runtime.HybridCLR;
using RSJWYFamework.Runtime.Senseshield;
using RSJWYFamework.Runtime.YooAssetModule;
using UnityEngine;
namespace RSJWYFamework.Runtime.Mono
{
    /// <summary>
    /// 框架的管理器，unity挂载
    /// </summary>
    public class RFWMonoManager:SingletonBaseMono<RFWMonoManager>
    {
        public IEventManage EventModle { get; private set; }
        public IYooAssetManager YooAssetManager{get; private set; }
        public IDataManager DataManagerataManager{get; private set; }
        
        public IHybridCLRManager HybridClrManager { get; private set; }
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);
            Main.Main.Instance.Init();
            
            EventModle = Main.Main.Instance.GetModule<IEventManage>();
            DataManagerataManager = Main.Main.Instance.AddModule<IDataManager>(new DefaultDataManager());
            YooAssetManager= Main.Main.Instance.AddModule<IYooAssetManager>(new DefaultYooAssetManager());
            HybridClrManager = Main.Main.Instance.AddModule<IHybridCLRManager>(new DefaultHybirdCLRManager());
        }

        private void Start()
        {
            YooAssetManager.InitPackage();
        }

        protected void OnApplicationQuit()
        {
            
        }
    }
}