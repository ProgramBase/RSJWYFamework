using System;
using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.AsyncOperation;
using RSJWYFamework.Runtime.Config;
using RSJWYFamework.Runtime.Module;
using RSJWYFamework.Runtime.Procedure;
using RSJWYFamework.Runtime.YooAssetModule;
using RSJWYFamework.Runtime.YooAssetModule.AsyncOperation;
using RSJWYFamework.Runtime.YooAssetModule.Procedure;
using UnityEngine;
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
            var projectConfig = Main.Main.DataManagerataManager.GetDataSetSB<ProjectConfig>();
            YooAssetManagerLoadTool.Setting(projectConfig.YooAssets.hostServerIP, projectConfig.ProjectName, projectConfig.APPName, projectConfig.Version);
            //创建异步任务
            LoadPackages operationR = new LoadPackages(projectConfig.YooAssets.RawFile.PackageName, projectConfig.YooAssets.RawFile.BuildPipeline.ToString(), projectConfig.YooAssets.PlayMode);
            LoadPackages operationP = new LoadPackages(projectConfig.YooAssets.Prefab.PackageName, projectConfig.YooAssets.Prefab.BuildPipeline.ToString(), projectConfig.YooAssets.PlayMode);
            //开始异步任务
            Main.Main.RAsyncOperationSystem.StartOperation(string.Empty, operationR);
            Main.Main.RAsyncOperationSystem.StartOperation(string.Empty, operationP);
            //等待完成
            await UniTask.WhenAll(operationR.UniTask, operationP.UniTask);
            //获取包
            RawPackage = YooAssets.GetPackage(projectConfig.YooAssets.RawFile.PackageName);
            PrefabPackage = YooAssets.GetPackage(projectConfig.YooAssets.Prefab.PackageName);
        }

        public event Action InitOverEvent;

        public void Close()
        {
        }

        public void Update(float time, float deltaTime)
        {
        }

        public void UpdatePerSecond(float time)
        {
        }

        public void ProcedureSwitchEven(ProcedureBase lastProcedureBase, ProcedureBase nextProcedureBase)
        {
        }
    }
}