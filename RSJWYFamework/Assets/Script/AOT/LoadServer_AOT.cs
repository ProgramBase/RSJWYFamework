using MyFamework.Runtime.Base;
using RSJWYFamework.Runtime.Data;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Event;
using RSJWYFamework.Runtime.HybridCLR;
using RSJWYFamework.Runtime.YooAssetModule;
using RSJWYFamework.Runtime.Main;


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
            Main.Initialize();
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