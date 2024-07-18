using System;
using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.AsyncOperation;
using RSJWYFamework.Runtime.Module;
using RSJWYFamework.Runtime.Procedure;
using RSJWYFamework.Runtime.YooAssetModule;
using RSJWYFamework.Runtime.YooAssetModule.AsyncOperation;
using RSJWYFamework.Runtime.YooAssetModule.Procedure;
using UnityEngine;
using YooAsset;

namespace RSJWYFamework.Runtime.Default.Manager
{
    public class DefaultYooAssetManager:IYooAssetManager,IModule
    {
        private YooAssetModuleSettingData _assetModuleSettingData;
        
        public ResourcePackage RawPackage { get; private set; }
        public ResourcePackage PrefabPackage { get; private set; }

        private IProcedureController[] procedures;

        /// <summary>
        /// 加载完成事件
        /// </summary>
        public event Action InitOverEvent;

        public void Start()
        {
        }


        public void Init()
        {
            //获取数据并存入数据
            _assetModuleSettingData = Resources.Load<YooAssetModuleSettingData>("YooAssetModuleSetting");
        }
        
        public async UniTask InitPackage()
        {
            YooAssets.Initialize();
            InitPackages operationR = new InitPackages(_assetModuleSettingData.RawFile.PackageName, _assetModuleSettingData.RawFile.BuildPipeline.ToString(), _assetModuleSettingData.PlayMode);
            InitPackages operationP = new InitPackages(_assetModuleSettingData.Prefab.PackageName, _assetModuleSettingData.Prefab.BuildPipeline.ToString(), _assetModuleSettingData.PlayMode);
            RAsyncOperationSystem.StartOperation(string.Empty,operationR);
            RAsyncOperationSystem.StartOperation(string.Empty,operationP);
            await UniTask.WhenAll(operationR.Task, operationP.Task);
        }

        public void Close()
        {
           
        }

        public void Update(float time, float deltaTime)
        {
            
        }

        public void UpdatePerSecond(float time)
        {
        }

        public void ProcedureSwitchEven(IProcedure lastProcedure, IProcedure nextProcedure)
        {
            
        }

    }
}