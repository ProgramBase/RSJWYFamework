using System;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Module;
using RSJWYFamework.Runtime.Procedure;
using RSJWYFamework.Runtime.YooAssetModule;
using Script.AOT.YooAssetModule.Procedure;
using UnityEngine;
using YooAsset;

namespace Script.AOT.YooAssetModule
{
    public class DefaultYooAssetManager:IYooAssetManager,IModule
    {
        private IProcedureController pc;
        
        private YooAssetModuleSettingData _assetModuleSettingData;
        
        public ResourcePackage RawPackage { get; private set; }
        public ResourcePackage PrefabPackage { get; private set; }

        /// <summary>
        /// 加载完成事件
        /// </summary>
        public event Action InitOverEvent;


        public void Init()
        {
            pc = new DefaultProcedureController(this);
            pc.ProcedureSwitchEvent += ProcedureSwitchEven;
            //获取数据并存入数据
            _assetModuleSettingData = Resources.Load<YooAssetModuleSettingData>("YooAssetModuleSetting");
            LoadServer_AOT.Instance.DataManagerataManager.AddDataSet(_assetModuleSettingData);
           
        }
        
        public void InitPackage()
        {
            YooAssets.Initialize();
            //创建流程
            //2.2.1版本offlinePlayModeEditorSimulateMode需要依次调用
            //init,requestversion,updatemanifest三部曲
            pc.AddProcedure(new InitPackageProcedure());
            pc.AddProcedure(new UpdatePackageVersionProcedure());
            pc.AddProcedure(new UpdatePackageManifestProcedure());
            pc.AddProcedure(new CreatePackageDownloaderProcedure());
            pc.AddProcedure(new DownloadPackageFilesProcedure());
            pc.AddProcedure(new DownloadPackageOverProcedure());
            pc.AddProcedure(new ClearPackageCacheProcedure());
            pc.AddProcedure(new UpdaterDoneProcedure());
            //写入当前初始化的内容
            SetInitPackageInfo();
        }
        /// <summary>
        /// 设置初始化包参数
        /// </summary>
        void SetInitPackageInfo()
        {
            pc.SetBlackboardValue("PlayMode",_assetModuleSettingData.PlayMode);
            int _n = 0;
            foreach (var t in _assetModuleSettingData.package)
            {
                if (!t.InitOk)
                {
                    pc.SetBlackboardValue("PackageName",t.PackageName);
                    pc.SetBlackboardValue("BuildPipeline",t.BuildPipeline.ToString());
                    pc.StartProcedure(typeof(InitPackageProcedure));
                    return;
                }
                _n++;
            }
            if (_assetModuleSettingData.package.Count==_n)
            {
                //加载完成
                RawPackage = YooAssets.GetPackage(_assetModuleSettingData.package[0].PackageName);
                PrefabPackage = YooAssets.GetPackage(_assetModuleSettingData.package[0].PackageName);
                InitOverEvent?.Invoke();
            }
        }


        public void Close()
        {
           
        }

        public void ProcedureSwitchEven(IProcedure lastProcedure, IProcedure nextProcedure)
        {
            if (lastProcedure is null&&nextProcedure is null)
                return;
            if (nextProcedure is UpdaterDoneProcedure)
            {
                SetInitPackageInfo();
            }
            else if (lastProcedure is InitPackageProcedure)
            {
                var packageName = (string)pc.GetBlackboardValue("PackageName");
                foreach (var t in _assetModuleSettingData.package)
                {
                    if (t.PackageName==packageName&&!t.InitOk)
                    {
                        t.InitOk = true;
                    }
                }
            }
            
        }

    }
}