using RSJWYFamework.Runtime.Module;
using RSJWYFamework.Runtime.Mono;
using RSJWYFamework.Runtime.Procedure;
using RSJWYFamework.Runtime.YooAssetModule;
using UnityEngine;
using YooAsset;

namespace RSJWYFamework.Runtime.Default.Manager
{
    public class DefaultYooAssetManager:IYooAssetManager,IModule
    {
        private IProcedureController pc;
        
        private YooAssetModuleSetting AssetModuleSetting;
        
        public ResourcePackage RawPackage;
        public ResourcePackage PrefabPackage;


        public void Init()
        {
            pc = new DefaultProcedureController(this);
            AssetModuleSetting = Resources.Load<YooAssetModuleSetting>("YooAssetModuleSetting");
            pc.ProcedureSwitchEvent += ProcedureSwitchEven;
            RFWMonoManager.Instance.DataManagerataManager.AddDataSet(AssetModuleSetting);
        }


        public void Close()
        {
            throw new System.NotImplementedException();
        }

        public void ProcedureSwitchEven(IProcedure lastProcedure, IProcedure nextProcedure)
        {
            
        }
    }
}