using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Procedure;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.Procedure
{
    /// <summary>
    /// 弱联网获取资源清单
    /// </summary>
    public class CheckUpdatePackageManifestProcedur:ProcedureBase
    {
        public override void OnInit()
        {
        }

        public override void OnClose()
        {
        }

        public override void OnEnter(ProcedureBase lastProcedureBase)
        {
            UniTask.Create(async () =>
            {
                var packageName = (string)pc.GetBlackboardValue("PackageName");
                var packageVersion = (string)pc.GetBlackboardValue("PackageVersion");
                var package = YooAssets.GetPackage(packageName);
                RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"校验上次完整启动时包{packageName}本地资源清单");
                var operation = package.UpdatePackageManifestAsync(packageVersion);
                await operation.ToUniTask();

                if (operation.Status != EOperationStatus.Succeed)
                {
                    pc.User.Exception(new ProcedureException($"加载包{packageName}本地清单失败！Error：{operation.Error}，请联网更新"));
                }
                else
                {
                    RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"加载包{packageName}本地清单成功");
                    pc.SwitchProcedure<CreatePackageDownloaderProcedure>();
                }
            });
        }

        public override void OnLeave(ProcedureBase nextProcedureBase)
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