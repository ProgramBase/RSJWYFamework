using System;
using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.AsyncOperation.Procedure;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.YooAssetModule.Tool;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.Procedure
{
    /// <summary>
    /// 初始化包
    /// </summary>
    public class InitPackageProcedure : ProcedureBase
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

            RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets, $"初始化包{packageName} 运行模式{playMode}  构建管线{buildPipeline}");
            // 创建资源包裹类
            var package = YooAssets.TryGetPackage(packageName);
            if (package == null)
                package = YooAssets.CreatePackage(packageName);

            InitializationOperation initializationOperation = null;
            switch (playMode)
            {
                case EPlayMode.EditorSimulateMode:
                    var simulateBuildResult = EditorSimulateModeHelper.SimulateBuild(buildPipeline, packageName);
                    var EcreateParameters = new EditorSimulateModeParameters
                    {
                        EditorFileSystemParameters =
                            FileSystemParameters.CreateDefaultEditorFileSystemParameters(simulateBuildResult)
                    };
                    initializationOperation = package.InitializeAsync(EcreateParameters);
                    break;
                case EPlayMode.OfflinePlayMode:
                    var Ofs = buildPipeline == EDefaultBuildPipeline.RawFileBuildPipeline.ToString()
                        ? FileSystemParameters.CreateDefaultBuildinRawFileSystemParameters(
                            new YooAssetManagerTool.FileDecryption())
                        : FileSystemParameters.CreateDefaultBuildinFileSystemParameters(
                            new YooAssetManagerTool.FileDecryption());

                    var OcreateParameters = new OfflinePlayModeParameters
                    {
                        BuildinFileSystemParameters = Ofs
                    };
                    initializationOperation = package.InitializeAsync(OcreateParameters);
                    break;
                case EPlayMode.HostPlayMode:
                    string defaultHostServer = YooAssetManagerTool.GetHostServerURL(packageName);
                    string fallbackHostServer = YooAssetManagerTool.GetHostServerURL(packageName);
                    IRemoteServices remoteServices =
                        new YooAssetManagerTool.RemoteServices(defaultHostServer, fallbackHostServer);

                    FileSystemParameters bfsp;
                    FileSystemParameters cfsp;
                    if (buildPipeline == EDefaultBuildPipeline.RawFileBuildPipeline.ToString())
                    {
                        bfsp = FileSystemParameters.CreateDefaultBuildinRawFileSystemParameters(
                            new YooAssetManagerTool.FileDecryption());
                        cfsp = FileSystemParameters.CreateDefaultCacheRawFileSystemParameters(remoteServices,
                            new YooAssetManagerTool.FileDecryption());
                    }
                    else
                    {
                        bfsp = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(
                            new YooAssetManagerTool.FileDecryption());
                        cfsp = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices,
                            new YooAssetManagerTool.FileDecryption());
                    }

                    var HcreateParameters = new HostPlayModeParameters
                    {
                        BuildinFileSystemParameters = bfsp,
                        CacheFileSystemParameters = cfsp
                    };
                    initializationOperation = package.InitializeAsync(HcreateParameters);
                    break;
                case EPlayMode.WebPlayMode:
                    /*var createParameters = new WebPlayModeParameters
                {
                    WebFileSystemParameters = FileSystemParameters.CreateDefaultWebFileSystemParameters()
                };
                initializationOperation = package.InitializeAsync(createParameters);*/
                    pc.User.Exception(new ProcedureException($"InitPackageProcedure：初始化包：{packageName}失败！Error：本框架不支持WebGL运行模式，没用过，无法测试"));
                    break;
                default:
                    pc.User.Exception( new ProcedureException($"InitPackageProcedure：初始化包：{packageName}失败！Error：未知的运行模式"));
                    break;
            }
            await initializationOperation.ToUniTask();
            if (initializationOperation.Status != EOperationStatus.Succeed)
            {
                pc.User.Exception(
                    new ProcedureException($"InitPackageProcedure：初始化包：{packageName}失败！Error：{initializationOperation.Error}"));
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