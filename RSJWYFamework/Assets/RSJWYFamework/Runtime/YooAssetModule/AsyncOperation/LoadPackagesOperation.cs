using RSJWYFamework.Runtime.AsyncOperation;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.StateMachine;
using RSJWYFamework.Runtime.YooAssetModule.StateNode;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.AsyncOperation
{
    
    /// <summary>
    /// 加载资源包
    /// </summary>
    public class LoadPackages:GameRAsyncOperation,IStateMachineUser
    {
        enum RSteps
        {
            None,
            Update,
            Done
        }
        private readonly StateMachineController pc;
        private RSteps _steps = RSteps.None;
        
        
        public LoadPackages(string packageName, EPlayMode playMode)
        {
            pc = new StateMachineController(this,"初始化资源管理");
            // 创建状态机
            //2.2.1版本 offlinePlayMode EditorSimulateMode 需要依次调用init, request version, update manifest 三部曲
            pc.AddProcedure(new InitPackageStateNode());
            pc.AddProcedure(new UpdatePackageVersionStateNode());
            pc.AddProcedure(new UpdatePackageManifestProcedur());
            pc.AddProcedure(new CreatePackageDownloaderStateNode());
            pc.AddProcedure(new DownloadPackageFilesStateNode());
            pc.AddProcedure(new DownloadPackageOverStateNode());
            pc.AddProcedure(new ClearPackageCacheStateNodeBase());
            pc.AddProcedure(new UpdaterDoneStateNode());
            //弱联网
            pc.AddProcedure(new CheckUpdatePackageManifestStateNode());
            pc.AddProcedure(new CheckResourceIntegrityStateNodeBase());
            //写入数据
            pc.SetBlackboardValue("PlayMode",playMode);
            pc.SetBlackboardValue("PackageName",packageName);
            //开始异步任务
            Main.Main.RAsyncOperationSystem.StartOperation(typeof(LoadPackages).FullName, this);
        }

        protected override void OnStart()
        {
            _steps = RSteps.Update;
            pc.StartProcedure(typeof(InitPackageStateNode));
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
                    if(pc.GetNowProcedure() == typeof(UpdaterDoneStateNode))
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
            SetException(exception);
        }

        public void Abort(string reason)
        {
            SetAbort(reason);
        }
    }
}