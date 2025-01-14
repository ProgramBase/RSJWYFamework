using System;
using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.StateMachine;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.StateNode
{
    /// <summary>
    /// 下载需要更新的文件
    /// </summary>
    public class DownloadPackageFilesStateNode:StateNodeBase
    {
        public override void OnInit()
        {
        }

        public override void OnClose()
        {
        }

        public override void OnEnter(StateNodeBase lastStateNodeBase)
        {
            BeginDownload().Forget();
        }
        private async UniTask  BeginDownload()
        {
            var packageName=(string)pc.GetBlackboardValue("PackageName");
            var downloader = (ResourceDownloaderOperation)pc.GetBlackboardValue("Downloader");
            downloader.DownloadErrorCallback = OnDownloadErrorFunction;
            downloader.DownloadUpdateCallback = OnDownloadProgressUpdateFunction;
            downloader.DownloadFinishCallback = DownloadFinishCallback;
            downloader.DownloadFileBeginCallback = OnStartDownloadFileFunction;
            downloader.BeginDownload();
            await downloader;
            

            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
            {
                //新清单文件新版本数据，不全无法正常启动
                pc.User.Exception(new ProcedureException($"包{packageName}下载失败：{downloader.Error}"));
            }
            else
            {
                RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"包{packageName}下载新资源完成");
                pc.SwitchProcedure(typeof(DownloadPackageOverStateNode));
            }
        }
        
        /// <summary>
        /// 开始下载
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnStartDownloadFileFunction(DownloadFileData data)
        {
            RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"包{data.PackageName}开始下载：文件名：{data.FileName}, 文件大小：{data.FileSize}");
        }

        /// <summary>
        /// 当下载器结束（无论成功或失败）
        /// </summary>
        /// <param name="data"></param>
        private void DownloadFinishCallback(DownloaderFinishData data)
        {
            RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"包{data.PackageName}下载：{ (data.Succeed ? "成功" : "失败")}");
        }

        /// <summary>
        /// 更新中
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnDownloadProgressUpdateFunction(DownloadUpdateData data)
        {
            RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"包{data.PackageName}文件总数：{data.TotalDownloadCount}, 已下载文件数：{data.CurrentDownloadCount}, 下载总大小：{data.TotalDownloadBytes}, 已下载大小：{data.CurrentDownloadBytes}");
        }

        /// <summary>
        /// 下载出错
        /// </summary>
        /// <param name="data"></param>
        private void OnDownloadErrorFunction(DownloadErrorData data)
        {
            RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"包{data.PackageName}下载出错：文件名：{data.FileName}, 错误信息：{data.ErrorInfo}");
        }

        static int GetBaiFenBi(int now, int sizeBytes)
        {
            return (int)sizeBytes / now * 100;
        }

        public override void OnLeave(StateNodeBase nextStateNodeBase)
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