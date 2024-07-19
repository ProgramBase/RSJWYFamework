using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Procedure;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.Procedure
{
    /// <summary>
    /// 更新包版本
    /// </summary>
    public class UpdatePackageVersionProcedureBase:ProcedureBase
    {
        public override void OnInit()
        {
            
        }

        public override void OnClose()
        {
        }

        public override void OnEnter(ProcedureBase lastProcedureBase)
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
            var modle = (EPlayMode)pc.GetBlackboardValue("PlayMode");
            var package = YooAssets.GetPackage(packageName);
            
            RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"更新包{packageName}版本");
            
            var operation = package.RequestPackageVersionAsync(false);
            await operation.ToUniTask();

            if (operation.Status != EOperationStatus.Succeed)
            {
                RSJWYLogger.Error(RSJWYFameworkEnum.YooAssets,$"更新包{packageName}版本失败！Error：{operation.Error}");
                pc.SetBlackboardValue("NetworkNormal", false);
            }
            else
            {
                pc.SetBlackboardValue("PackageVersion", operation.PackageVersion);
                pc.SetBlackboardValue("NetworkNormal", true);
                RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"包{packageName}请求到包版本为：{operation.PackageVersion}");
                pc.SwitchProcedure(typeof(UpdatePackageManifestProcedureBase));
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