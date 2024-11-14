using RSJWYFamework.Runtime.AsyncOperation.Procedure;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.YooAssetModule.Procedure;

namespace RSJWYFamework.Runtime.YooAssetModule.Procedure
{
    /// <summary>
    /// 下载更新文件完成
    /// </summary>
    public class DownloadPackageOverProcedure:ProcedureBase
    {
        public override void OnInit()
        {
        }

        public override void OnClose()
        {
        }

        public override void OnEnter(ProcedureBase lastProcedureBase)
        {
            RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"下载新资源完成");
            pc.SwitchProcedure(typeof(ClearPackageCacheProcedureBase));
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