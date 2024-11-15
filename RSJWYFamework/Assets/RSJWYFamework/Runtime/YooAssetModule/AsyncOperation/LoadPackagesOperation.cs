using RSJWYFamework.Runtime.AsyncOperation;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Procedure;
using RSJWYFamework.Runtime.YooAssetModule.Procedure;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.AsyncOperation
{
    
    /// <summary>
    /// 加载资源包
    /// </summary>
    public class LoadPackages:GameRAsyncOperation,IProcedureUser
    {
        enum RSteps
        {
            None,
            Update,
            Done
        }
        private readonly ProcedureController pc;
        private RSteps _steps = RSteps.None;
        
        
        public LoadPackages(string packageName, string buildPipeline, EPlayMode playMode)
        {
            pc = new ProcedureController(this,"初始化资源管理");
            // 创建状态机
            //2.2.1版本 offlinePlayMode EditorSimulateMode 需要依次调用init, request version, update manifest 三部曲
            pc.AddProcedure(new InitPackageProcedure());
            pc.AddProcedure(new UpdatePackageVersionProcedure());
            pc.AddProcedure(new UpdatePackageManifestProcedur());
            pc.AddProcedure(new CreatePackageDownloaderProcedure());
            pc.AddProcedure(new DownloadPackageFilesProcedure());
            pc.AddProcedure(new DownloadPackageOverProcedure());
            pc.AddProcedure(new ClearPackageCacheProcedureBase());
            pc.AddProcedure(new UpdaterDoneProcedure());
            //写入数据
            pc.SetBlackboardValue("PlayMode",playMode);
            pc.SetBlackboardValue("PackageName",packageName);
            pc.SetBlackboardValue("BuildPipeline",buildPipeline);
            //开始异步任务
            Main.Main.RAsyncOperationSystem.StartOperation(typeof(LoadPackages).FullName, this);
        }

        protected override void OnStart()
        {
            _steps = RSteps.Update;
            pc.StartProcedure(typeof(InitPackageProcedure));
        }

        protected override void OnUpdate(float time, float deltaTime)
        {
            switch (_steps)
            {
                case RSteps.None:
                case RSteps.Done:
                    return;
                case RSteps.Update:
                {
                    pc.OnUpdate(time,deltaTime);
                    if(pc.GetNowProcedure() == typeof(UpdaterDoneProcedure))
                    {
                        Status = RAsyncOperationStatus.Succeed;
                        _steps = RSteps.Done;
                    }

                    break;
                }
            }
        }

        protected override void OnUpdatePerSecond(float time)
        {
            
        }


        protected override void OnAbort()
        {
            
        }

        public void Exception(ProcedureException exception)
        {
            
        }
    }
}