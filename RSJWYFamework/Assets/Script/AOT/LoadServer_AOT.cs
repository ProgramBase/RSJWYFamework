using MyFamework.Runtime.Base;
using RSJWYFamework.Runtime.Data;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Event;
using RSJWYFamework.Runtime.HybridCLR;
using RSJWYFamework.Runtime.YooAssetModule;
using Script.AOT.HybridCLR;
using Script.AOT.YooAssetModule;

namespace Script.AOT
{
    /// <summary>
    /// 框架的管理器，unity挂载
    /// </summary>
    public class LoadServer_AOT:SingletonBaseMono<LoadServer_AOT>
    {
        public IEventManage EventModle { get; private set; }
        public DefaultYooAssetManager YooAssetManager{get; private set; }
        public DefaultDataManager DataManagerataManager{get; private set; }
        
        public DefaultHybirdCLRManager HybridClrManager { get; private set; }
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);
            RSJWYFamework.Runtime.Main.Main.Instance.Init();
            
            EventModle = RSJWYFamework.Runtime.Main.Main.Instance.GetModule<IEventManage>();
            DataManagerataManager = RSJWYFamework.Runtime.Main.Main.Instance.AddModule<IDataManager>(new DefaultDataManager()) as DefaultDataManager;
            YooAssetManager= RSJWYFamework.Runtime.Main.Main.Instance.AddModule<IYooAssetManager>(new DefaultYooAssetManager())as DefaultYooAssetManager;
            HybridClrManager = RSJWYFamework.Runtime.Main.Main.Instance.AddModule<IHybridCLRManager>(new DefaultHybirdCLRManager()) as DefaultHybirdCLRManager;
        }

        private void Start()
        {
            YooAssetManager.InitOverEvent += () =>
            {
                HybridClrManager.InitProcedure();
            };
            YooAssetManager.InitPackage();
        }

        protected void OnApplicationQuit()
        {
            
        }
    }
}