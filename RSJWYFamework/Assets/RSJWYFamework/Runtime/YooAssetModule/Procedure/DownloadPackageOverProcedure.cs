using RSJWYFamework.Runtime.Procedure;
using RSJWYFamework.Runtime.YooAssetModule.Procedure;

namespace RSJWYFamework.Runtime.YooAssetModule.Procedure
{
    /// <summary>
    /// 下载更新文件完成
    /// </summary>
    public class DownloadPackageOverProcedure:IProcedure
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
            pc.SwitchProcedure(typeof(ClearPackageCacheProcedure));
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