using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Procedure;
using RSJWYFamework.Runtime.YooAssetModule.Tool;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.Procedure
{
    /// <summary>
    /// 初始化包
    /// </summary>
    public class InitPackageProcedure:ProcedureBase
    {
        
        public override void OnInit()
        {
           
        }
        
        public override void OnEnter(ProcedureBase lastProcedureBase)
        {
            InitPackage().Forget();
        }

        public override void OnLeave(ProcedureBase nextProcedureBase)
        {
            
        }
        /// <summary>
        /// 创建初始化包
        /// </summary>
        /// <returns></returns>
        async UniTask InitPackage()
        {
            var playMode = (EPlayMode)pc.GetBlackboardValue("PlayMode");
            var packageName = (string)pc.GetBlackboardValue("PackageName");
            var buildPipeline = (string)pc.GetBlackboardValue("BuildPipeline");
            
            RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"初始化包{packageName} 运行模式{playMode}  构建管线{buildPipeline}");
            // 创建资源包裹类
            var package = YooAssets.TryGetPackage(packageName);
            if (package == null)
                package = YooAssets.CreatePackage(packageName);

            // 编辑器下的模拟模式
            InitializationOperation initializationOperation = null;
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var simulateBuildResult = EditorSimulateModeHelper.SimulateBuild(buildPipeline, packageName);
                var createParameters = new EditorSimulateModeParameters
                {
                    EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(simulateBuildResult)
                };
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 单机运行模式
            if (playMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters
                {
                    BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters()
                    
                };
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 联机运行模式
            if (playMode == EPlayMode.HostPlayMode)
            {
                string defaultHostServer = YooAssetManagerTool.GetHostServerURL(packageName);
                string fallbackHostServer = YooAssetManagerTool.GetHostServerURL(packageName);
                IRemoteServices remoteServices = new YooAssetManagerTool.RemoteServices(defaultHostServer, fallbackHostServer);
                var createParameters = new HostPlayModeParameters
                {
                    BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(),
                    CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices)
                };
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // WebGL运行模式
            if (playMode == EPlayMode.WebPlayMode)
            {
                /*var createParameters = new WebPlayModeParameters
                {
                    WebFileSystemParameters = FileSystemParameters.CreateDefaultWebFileSystemParameters()
                };
                initializationOperation = package.InitializeAsync(createParameters);*/
                pc.modle.Exception(new ProcedureException($"初始化包：{packageName}失败！Error：本框架不支持WebGL运行模式"));
            }

            await initializationOperation.ToUniTask();
            if (initializationOperation.Status!=EOperationStatus.Succeed)
            {
                pc.modle.Exception(new ProcedureException($"初始化包：{packageName}失败！Error：{initializationOperation.Error}"));
            }
            pc.SwitchProcedure(typeof(UpdatePackageVersionProcedure));
        }

        public override void OnClose()
        {
            
        }

        public override void OnUpdate()
        {
        }

        public override void OnUpdateSecond()
        {
            
        }
    }
}