using System;
using RSJWYFamework.Runtime.Module;
using RSJWYFamework.Runtime.Procedure;
using RSJWYFamework.Runtime.YooAssetModule;
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


        public void Init()
        {
            /*pc = new DefaultProcedureController(this);
            pc.ProcedureSwitchEvent += ProcedureSwitchEven;*/
            //获取数据并存入数据
            _assetModuleSettingData = Resources.Load<YooAssetModuleSettingData>("YooAssetModuleSetting");
            //LoadServer_AOT.Instance.DataManagerataManager.AddDataSet(_assetModuleSettingData);
           
        }
        
        public void InitPackage()
        {
            YooAssets.Initialize();
            procedures = new IProcedureController[_assetModuleSettingData.package.Count];
            for (int i = 0; i < _assetModuleSettingData.package.Count; i++)
            {
                var data = _assetModuleSettingData.package;
                var pc = new DefaultProcedureController(this);
                //添加流程
                pc.AddProcedure(new InitPackageProcedure());
                pc.AddProcedure(new UpdatePackageVersionProcedure());
                pc.AddProcedure(new UpdatePackageManifestProcedure());
                pc.AddProcedure(new CreatePackageDownloaderProcedure());
                pc.AddProcedure(new DownloadPackageFilesProcedure());
                pc.AddProcedure(new DownloadPackageOverProcedure());
                pc.AddProcedure(new ClearPackageCacheProcedure());
                pc.AddProcedure(new UpdaterDoneProcedure());
                //
                pc.SetBlackboardValue("PlayMode",_assetModuleSettingData.PlayMode);
                pc.SetBlackboardValue("PackageName",data[i].PackageName);
                pc.SetBlackboardValue("BuildPipeline",data[i].BuildPipeline.ToString());
                pc.StartProcedure(typeof(InitPackageProcedure));
                procedures[i] = pc;
            }
            //创建流程
            //2.2.1版本offlinePlayModeEditorSimulateMode需要依次调用
            //init,requestversion,updatemanifest三部曲
           
            //写入当前初始化的内容
            SetInitPackageInfo();
        }
        /// <summary>
        /// 设置初始化包参数
        /// </summary>
        void SetInitPackageInfo()
        {
            /*pc.SetBlackboardValue("PlayMode",_assetModuleSettingData.PlayMode);
            int _n = 0;
            foreach (var t in _assetModuleSettingData.package)
            {
                pc.SetBlackboardValue("PackageName",t.PackageName);
                pc.SetBlackboardValue("BuildPipeline",t.BuildPipeline.ToString());
                pc.StartProcedure(typeof(InitPackageProcedure));
                return;
                _n++;
            }
            if (_assetModuleSettingData.package.Count==_n)
            {
                //加载完成
                RawPackage = YooAssets.GetPackage(_assetModuleSettingData.package[0].PackageName);
                PrefabPackage = YooAssets.GetPackage(_assetModuleSettingData.package[0].PackageName);
                InitOverEvent?.Invoke();
            }*/
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