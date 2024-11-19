using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.StateMachine;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.StateNode
{
    /// <summary>
    /// 清理未使用的缓存文件
    /// </summary>
    public class ClearPackageCacheStateNodeBase:StateNodeBase
    {
        public override void OnInit()
        {
        }

        public override  void OnClose()
        {
        }

        public override  void OnEnter(StateNodeBase lastStateNodeBase)
        {
            UniTask.Create(async () =>
            {
                RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"清理未使用的缓存文件");
                var packageName = (string)pc.GetBlackboardValue("PackageName");
                var package = YooAssets.GetPackage(packageName);
                var operation = package.ClearUnusedBundleFilesAsync();
                operation.Completed += Operation_Completed;
                await operation.ToUniTask();
                pc.SwitchProcedure(typeof(UpdaterDoneStateNode));
            });
        }
        private void Operation_Completed(YooAsset.AsyncOperationBase obj)
        {
           
        }
        public override  void OnLeave(StateNodeBase nextStateNodeBase)
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