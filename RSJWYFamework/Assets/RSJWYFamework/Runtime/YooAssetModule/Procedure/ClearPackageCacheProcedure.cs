using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Procedure;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.Procedure
{
    /// <summary>
    /// 清理未使用的缓存文件
    /// </summary>
    public class ClearPackageCacheProcedureBase:ProcedureBase
    {
        public override void OnInit()
        {
        }

        public override  void OnClose()
        {
        }

        public override  void OnEnter(ProcedureBase lastProcedureBase)
        {
            UniTask.Create(async () =>
            {
                RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"清理未使用的缓存文件");
                var packageName = (string)pc.GetBlackboardValue("PackageName");
                var package = YooAssets.GetPackage(packageName);
                var operation = package.ClearUnusedBundleFilesAsync();
                operation.Completed += Operation_Completed;
                await operation.ToUniTask();
                pc.SwitchProcedure(typeof(UpdaterDoneProcedure));
            });
        }
        private void Operation_Completed(YooAsset.AsyncOperationBase obj)
        {
           
        }
        public override  void OnLeave(ProcedureBase nextProcedureBase)
        {;
        }

        public override  void OnUpdate()
        {
        }

        public override  void OnUpdateSecond()
        {
        }
        
    }
}