using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Procedure;
using YooAsset;

namespace Script.AOT.YooAssetModule.Procedure
{
    /// <summary>
    /// 清理未使用的缓存文件
    /// </summary>
    public class ClearPackageCacheProcedure:IProcedure
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
            RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"清理未使用的缓存文件");
            var packageName = (string)pc.GetBlackboardValue("PackageName");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.ClearUnusedBundleFilesAsync();
            operation.Completed += Operation_Completed;
            operation.ToUniTask().Forget();
        }
        private void Operation_Completed(YooAsset.AsyncOperationBase obj)
        {
            pc.SwitchProcedure(typeof(UpdaterDoneProcedure));
        }
        public void OnLeave(IProcedure nextProcedure)
        {;
        }

        public void OnUpdate()
        {
        }

        public void OnUpdateSecond()
        {
        }
    }
}