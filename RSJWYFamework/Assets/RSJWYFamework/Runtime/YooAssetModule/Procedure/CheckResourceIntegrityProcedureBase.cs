using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Procedure;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.Procedure
{
    /// <summary>
    /// 弱联网检查资源完整性
    /// </summary>
    public class CheckResourceIntegrityProcedureBase:ProcedureBase
    {
        public override void OnInit()
        {
            
        }

        public override void OnClose()
        {
        }

        public override void OnEnter(ProcedureBase lastProcedureBase)
        {
            var packageName = (string)pc.GetBlackboardValue("PackageName");
            var package = YooAssets.GetPackage(packageName);
            RSJWYLogger.Log($"包：{packageName}校验资源完整性");
            var downloader = package.CreateResourceDownloader(1, 1, 60);
            if (downloader.TotalDownloadCount > 0)   
            {
                pc.User.Exception(new ProcedureException($"包：{packageName}校验资源完整性失败！需要下载{downloader.TotalDownloadCount}个更新内容，请检查网络，进行在线更新"));
            }
            else
            {
                pc.SwitchProcedure<UpdaterDoneProcedure>();
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