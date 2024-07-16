using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Procedure;
using YooAsset;

namespace Script.AOT.YooAssetModule.Procedure
{
    /// <summary>
    /// 更新资源清单
    /// </summary>
    public class UpdatePackageManifestProcedure:IProcedure
    {
        public IProcedureController pc { get; set; }
        public void OnInit()
        {
           
        }

        public void OnClose()
        {
        }

        public void OnEnter(IProcedure lastProcedure)
        {
            UpdateManifest().Forget();
        }
        private async UniTask UpdateManifest() 
        {
            await UniTask.WaitForSeconds(0.5f);
            var packageName = (string)pc.GetBlackboardValue("PackageName");
            var packageVersion = (string)pc.GetBlackboardValue("PackageVersion");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.UpdatePackageManifestAsync(packageVersion);
            await operation.ToUniTask();

            if (operation.Status != EOperationStatus.Succeed)
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.YooAssets,$"更新包{packageName}清单失败！Error：{operation.Error}");
            }
            else
            {
                RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"更新包{packageName}清单成功");
                pc.SwitchProcedure(typeof(CreatePackageDownloaderProcedure));
            }
        }

        public void OnLeave(IProcedure nextProcedure)
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