using System;
using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Config;
using RSJWYFamework.Runtime.Module;
using RSJWYFamework.Runtime.YooAssetModule.AsyncOperation;
using RSJWYFamework.Runtime.YooAssetModule.Tool;
using YooAsset;

namespace RSJWYFamework.Runtime.Default.Manager
{
    public class DefaultYooAssetManager :  IModule
    {
        public ResourcePackage RawPackage { get; private set; }
        public ResourcePackage PrefabPackage { get; private set; }


        public void Start()
        {
        }


        public void Init()
        {
            YooAssets.Initialize();
        }

        public async UniTask LoadPackage()
        {
            //获取数据并存入数据
            var projectConfig = RSJWYFamework.Runtime.Main.Main.DataManagerataManager.GetDataSetSB<ProjectConfig>();
            YooAssetManagerTool.Setting(projectConfig.YooAssets.hostServerIP, projectConfig.ProjectName, projectConfig.APPName, projectConfig.Version);
            //创建异步任务
            LoadPackages operationR = new LoadPackages("RawFilePackage", EDefaultBuildPipeline.BuiltinBuildPipeline.ToString(), projectConfig.YooAssets.PlayMode);
            LoadPackages operationP = new LoadPackages("PrefabPackage",  EDefaultBuildPipeline.RawFileBuildPipeline.ToString(), projectConfig.YooAssets.PlayMode);
            //开始异步任务
            RSJWYFamework.Runtime.Main.Main.RAsyncOperationSystem.StartOperation(string.Empty, operationR);
            RSJWYFamework.Runtime.Main.Main.RAsyncOperationSystem.StartOperation(string.Empty, operationP);
            //等待完成
            await UniTask.WhenAll(operationR.UniTask, operationP.UniTask);
            //获取包
            RawPackage = YooAssets.GetPackage("RawFilePackage");
            PrefabPackage = YooAssets.GetPackage("PrefabPackage");
        }

        public event Action InitOverEvent;

        public void Close()
        {
        }
    }
}