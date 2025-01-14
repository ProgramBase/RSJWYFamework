using System;
using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.StateMachine;
using RSJWYFamework.Runtime.YooAssetModule.Tool;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.StateNode
{
    /// <summary>
    /// 初始化包
    /// </summary>
    public class InitPackageStateNode : StateNodeBase
    {
        public override void OnInit()
        {
        }

        public override void OnEnter(StateNodeBase lastStateNodeBase)
        {
            InitPackage().Forget();
        }

        public override void OnLeave(StateNodeBase nextStateNodeBase)
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

            RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets, $"初始化包{packageName} 运行模式{playMode}");
            // 创建资源包裹类
            var package = YooAssets.TryGetPackage(packageName);
            if (package == null)
                package = YooAssets.CreatePackage(packageName);

            InitializationOperation initializationOperation = null;
            switch (playMode)
            {
                case EPlayMode.EditorSimulateMode:
                    var simulateBuildParam = new EditorSimulateBuildParam(packageName);
                    var simulateBuildResult = EditorSimulateModeHelper.SimulateBuild(simulateBuildParam);
                    var packageRoot = simulateBuildResult.PackageRootDirectory;
                    var EcreateParameters = new EditorSimulateModeParameters();
                    EcreateParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                    initializationOperation = package.InitializeAsync(EcreateParameters);
                    break;
                case EPlayMode.OfflinePlayMode:
                    var Ofs = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(new YooAssetManagerTool.FileDecryption());
                    var OcreateParameters = new OfflinePlayModeParameters
                    {
                        BuildinFileSystemParameters = Ofs
                    };
                    initializationOperation = package.InitializeAsync(OcreateParameters);
                    break;
                case EPlayMode.HostPlayMode:
                    string defaultHostServer = YooAssetManagerTool.GetHostServerURL(packageName);
                    string fallbackHostServer = YooAssetManagerTool.GetHostServerURL(packageName);
                    IRemoteServices remoteServices = new YooAssetManagerTool.RemoteServices(defaultHostServer, fallbackHostServer);

                    var HcreateParameters = new HostPlayModeParameters();
                    HcreateParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                    HcreateParameters.CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
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
            pc.SwitchProcedure<UpdatePackageVersionStateNode>();
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