using RSJWYFamework.Runtime.AsyncOperation;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.YooAssetModule.Procedure;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.AsyncOperation
{
    
    /// <summary>
    /// 初始化包
    /// </summary>
    public class InitPackages:GameRAsyncOperation
    {
        enum RSteps
        {
            None,
            Update,
            Done
        }
        private readonly DefaultProcedureController pc;
        private RSteps _steps = RSteps.None;
        
        
        public InitPackages(string packageName, string buildPipeline, EPlayMode playMode)
        {
            // 创建状态机
            pc.AddProcedure(new InitPackageProcedure());
            pc.AddProcedure(new UpdatePackageVersionProcedure());
            pc.AddProcedure(new UpdatePackageManifestProcedure());
            pc.AddProcedure(new CreatePackageDownloaderProcedure());
            pc.AddProcedure(new DownloadPackageFilesProcedure());
            pc.AddProcedure(new DownloadPackageOverProcedure());
            pc.AddProcedure(new ClearPackageCacheProcedure());
            pc.AddProcedure(new UpdaterDoneProcedure());
            //写入数据
            pc.SetBlackboardValue("PlayMode",playMode);
            pc.SetBlackboardValue("PackageName",packageName);
            pc.SetBlackboardValue("BuildPipeline",buildPipeline);
        }

        internal override void InternalOnUpdate(float time, float deltaTime)
        {
            if (_steps == RSteps.None || _steps == RSteps.Done)
                return;

            if(_steps == RSteps.Update)
            {
                pc.OnUpdate(time,deltaTime);
                if(pc.CurrentNode == typeof(FsmUpdaterDone).FullName)
                {
                    _eventGroup.RemoveAllListener();
                    Status = EOperationStatus.Succeed;
                    _steps = RSteps.Done;
                }
            }
        }

        internal override void InternalOnUpdatePerSecond(float time)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnStart()
        {
            _steps = RSteps.Update;
            pc.StartProcedure(typeof(InitPackageProcedure));
        }

        protected override void OnUpdate(float time, float deltaTime)
        {
            throw new System.NotImplementedException();
        }


        protected override void OnAbort()
        {
            throw new System.NotImplementedException();
        }
    }
}