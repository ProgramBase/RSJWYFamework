using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Procedure;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.Procedure
{
    /// <summary>
    /// 更新资源清单
    /// </summary>
    public class UpdatePackageManifestProcedur:ProcedureBase
    {
        public override void OnInit()
        {
           
        }

        public  override void OnClose()
        {
        }

        public  override void OnEnter(ProcedureBase lastProcedureBase)
        {
            UpdateManifest().Forget();
        }
        private async UniTask UpdateManifest() 
        {
            var packageName = (string)pc.GetBlackboardValue("PackageName");
            var packageVersion = (string)pc.GetBlackboardValue("PackageVersion");
            var package = YooAssets.GetPackage(packageName);
            RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"更新包{packageName}资源清单");
            var operation = package.UpdatePackageManifestAsync(packageVersion);
            await operation.ToUniTask();

            if (operation.Status != EOperationStatus.Succeed)
            {
                pc.User.Exception(new ProcedureException($"更新包{packageName}清单失败！Error：{operation.Error}，请检查资源是否存在，或者网络是否正常"));
            }
            else
            {
                RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"更新包{packageName}清单成功");
                pc.SwitchProcedure<CreatePackageDownloaderProcedure>();
            }
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