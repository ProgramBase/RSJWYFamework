using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Procedure;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.Procedure
{
    /// <summary>
    /// 创建文件下载器
    /// </summary>
    public class CreatePackageDownloaderProcedure:ProcedureBase
    {
        public override  void OnInit()
        {
        }

        public override  void OnClose()
        {
        }

        public override void OnEnter(ProcedureBase lastProcedureBase)
        {
            CreateDownloader().Forget();
        }
        async UniTask CreateDownloader()
        {
            await UniTask.WaitForSeconds(0.5f);
            var packageName = (string)pc.GetBlackboardValue("PackageName");
            var package = YooAssets.GetPackage(packageName);
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
            pc.SetBlackboardValue("Downloader", downloader);

            if (downloader.TotalDownloadCount == 0)
            {
                RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"包{packageName}没找到任何下载文件！");
                pc.SwitchProcedure(typeof(UpdaterDoneProcedure));
            }
            else
            {
                // 发现新更新文件后，挂起流程系统
                // 注意：开发者需要在下载前检测磁盘空间不足
                RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"包{packageName}发现新文件！下载的文件总量：{downloader.TotalDownloadCount}，总大小：{downloader.TotalDownloadBytes}");
                pc.SwitchProcedure(typeof(DownloadPackageFilesProcedure));
            }
        }

        public  override void OnLeave(ProcedureBase nextProcedureBase)
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