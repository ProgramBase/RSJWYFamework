using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Procedure;
using UnityEngine;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.Procedure
{
    /// <summary>
    /// 更新包版本
    /// </summary>
    public class UpdatePackageVersionProcedure:ProcedureBase
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

            var packageName = (string)pc.GetBlackboardValue("PackageName");
            var modle = (EPlayMode)pc.GetBlackboardValue("PlayMode");
            var package = YooAssets.GetPackage(packageName);
            
            RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"更新包{packageName}版本");
            
            var operation = package.RequestPackageVersionAsync(false);
            await operation.ToUniTask();
            if (operation.Status != EOperationStatus.Succeed)
            {
                RSJWYLogger.Warning(RSJWYFameworkEnum.YooAssets,$"更新包{packageName}版本失败！Error：{operation.Error}");
                pc.SetBlackboardValue("NetworkNormal", false);
                if (modle == EPlayMode.OfflinePlayMode)
                {
                    var version= PlayerPrefs.GetString($"YooAssets_{packageName}_Version");
                    if (string.IsNullOrEmpty(version))
                    {
                        pc.User.Abort($"更新包{packageName}版本失败！Error：{operation.Error}，没有找到本地版本记录，无法启动，请检查网络，进行在线更新");
                    }
                    else
                    {
                        RSJWYLogger.Warning(RSJWYFameworkEnum.YooAssets,$"网络不可达，将使用上一次获取到的完整包版本：包{packageName}请求到包版本为：{version}");
                        pc.SetBlackboardValue("PackageVersion", version);
                        pc.SwitchProcedure<UpdatePackageManifestProcedur>();
                    }
                }
                else
                {
                    pc.User.Exception(new ProcedureException($"更新包{packageName}版本失败！Error：{operation.Error}，运行模式为：{modle}，本地不存在内部资源包，无法启动"));
                }
            }
            else
            {
                pc.SetBlackboardValue("PackageVersion", operation.PackageVersion);
                pc.SetBlackboardValue("NetworkNormal", true);
                RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"包{packageName}请求到包版本为：{operation.PackageVersion}");
                pc.SwitchProcedure<UpdatePackageManifestProcedur>();
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