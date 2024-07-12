using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Mono;
using RSJWYFamework.Runtime.Procedure;
using UnityEngine;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.Procedure
{
    /// <summary>
    /// 初始化包
    /// </summary>
    public class InitPackageProcedure:IProcedure
    {
        public IProcedureController pc { get; set; }
        
        public void OnInit()
        {
           
        }
        
        public void OnEnter(IProcedure lastProcedure)
        {
            InitPackage().Forget();
        }

        public void OnLeave(IProcedure nextProcedure)
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
            // 创建资源包裹类
            var package = YooAssets.TryGetPackage(packageName);
            if (package == null)
                package = YooAssets.CreatePackage(packageName);

            // 编辑器下的模拟模式
            InitializationOperation initializationOperation = null;
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var simulateBuildResult = EditorSimulateModeHelper.SimulateBuild(buildPipeline, packageName);
                var createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters =
                    FileSystemParameters.CreateDefaultEditorFileSystemParameters(simulateBuildResult);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 单机运行模式
            if (playMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.BuildinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 联机运行模式
            if (playMode == EPlayMode.HostPlayMode)
            {
                string defaultHostServer = YooAssetManagerLoadTool.GetHostServerURL();
                string fallbackHostServer = YooAssetManagerLoadTool.GetHostServerURL();
                IRemoteServices remoteServices = new YooAssetManagerLoadTool.RemoteServices(defaultHostServer, fallbackHostServer);
                var createParameters = new HostPlayModeParameters();
                createParameters.BuildinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                createParameters.CacheFileSystemParameters =
                    FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // WebGL运行模式
            if (playMode == EPlayMode.WebPlayMode)
            {
                var createParameters = new WebPlayModeParameters();
                createParameters.WebFileSystemParameters = FileSystemParameters.CreateDefaultWebFileSystemParameters();
                initializationOperation = package.InitializeAsync(createParameters);
            }

            await initializationOperation.ToUniTask();
            if (initializationOperation.Status!=EOperationStatus.Succeed)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.YooAssets,$"初始化包：{packageName}失败！Error：{initializationOperation.Error}");
            }
            pc.SwitchProcedure(typeof(UpdatePackageVersionProcedure));
        }

        public void OnClose()
        {
            
        }

        public void OnUpdate()
        {
        }

        public void OnUpdateSecond()
        {
            
        }
    }
}