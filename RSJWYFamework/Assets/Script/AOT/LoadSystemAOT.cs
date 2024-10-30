using Cysharp.Threading.Tasks;
using MyFamework.Runtime.Base;
using RSJWYFamework.Runtime.Base;
using RSJWYFamework.Runtime.Config;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.YooAssetModule;
using UnityEngine;


namespace Script.AOT
{
    /// <summary>
    /// 框架的管理器，unity挂载
    /// </summary>
    public class LoadSystemAOT:SingletonBaseMono<LoadSystemAOT>
    {
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);
            Main.Initialize();
        }

        private async void Start()
        {
            StartApp().Forget();
        }

        async UniTaskVoid StartApp()
        {
            RSJWYLogger.Log("加载项目配置文件，并加载到数据管理中···");
            var projectset =  Resources.Load<ProjectConfig>("ProjectConfig");
            Main.DataManagerataManager.AddDataSet(projectset);
            RSJWYLogger.Log($"日志等级：{projectset.Loglevel}");
            RSJWYLogger.Loglevel = projectset.Loglevel;
            RSJWYLogger.Log("等待包初始化");
            await Main.YooAssetManager.LoadPackage();
            RSJWYLogger.Log("包初始化完成，加载热更代码");
            await Main.HybridClrManager.LoadHotCodeDLL();
            
            RSJWYLogger.Log("加载入口");
            var prefab = Main.YooAssetManager.PrefabPackage.LoadAssetAsync("Prefab_GameObject");
            await prefab.ToUniTask();
            var Io = prefab.InstantiateAsync();
            await Io.ToUniTask();
        }
        protected void OnApplicationQuit()
        {
            
        }
    }
}