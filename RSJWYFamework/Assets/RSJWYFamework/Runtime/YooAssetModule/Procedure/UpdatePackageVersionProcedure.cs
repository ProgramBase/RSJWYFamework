using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Procedure;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.Procedure
{
    /// <summary>
    /// 更新包版本
    /// </summary>
    public class UpdatePackageVersionProcedure:IProcedure
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
            UpdatePackageVersion().Forget();
        }
        /// <summary>
        /// 更新包版本
        /// </summary>
        private async UniTask UpdatePackageVersion()
        {
            await UniTask.WaitForSeconds(0.5f);

            var packageName = (string)pc.GetBlackboardValue("PackageName");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.RequestPackageVersionAsync();
            await operation.ToUniTask();

            if (operation.Status != EOperationStatus.Succeed)
            {
                RSJWYLogger.Error(RSJWYFameworkEnum.YooAssets,$"更新包{packageName}版本失败！Error：{operation.Error}");
            }
            else
            {
                pc.SetBlackboardValue("PackageVersion", operation.PackageVersion);
                RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"包{packageName}请求到包版本为：{operation.PackageVersion}");
                pc.SwitchProcedure(typeof(UpdatePackageManifestProcedure));
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