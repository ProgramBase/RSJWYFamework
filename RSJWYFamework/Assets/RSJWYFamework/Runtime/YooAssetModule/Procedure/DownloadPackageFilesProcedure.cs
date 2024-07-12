using System;
using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Procedure;
using UnityEngine;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.Procedure
{
    /// <summary>
    /// 下载需要更新的文件
    /// </summary>
    public class DownloadPackageFilesProcedure:IProcedure
    {
        public IProcedureController pc { get; set; }
        private string packageName;
        public void OnInit()
        {
        }

        public void OnClose()
        {
        }

        public void OnEnter(IProcedure lastProcedure)
        {
            BeginDownload().Forget();
        }
        private async UniTask  BeginDownload()
        {
            packageName=(string)pc.GetBlackboardValue("PackageName");
            var downloader = (ResourceDownloaderOperation)pc.GetBlackboardValue("Downloader");
            downloader.OnDownloadErrorCallback = OnDownloadErrorFunction;
            downloader.OnDownloadProgressCallback = OnDownloadProgressUpdateFunction;
            downloader.OnDownloadOverCallback = OnDownloadOverFunction;
            downloader.OnStartDownloadFileCallback = OnStartDownloadFileFunction;
            downloader.BeginDownload();
            await downloader;

            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
            {
                RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"包{packageName}下载失败：{downloader.Error}");
            }
            else
            {
                RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"包{packageName}下载新资源完成");
                pc.SwitchProcedure(typeof(DownloadPackageOverProcedure));
            }
        }
        
        /// <summary>
        /// 开始下载
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sizeBytes"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnStartDownloadFileFunction(string fileName, long sizeBytes)
        {
            RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"包{packageName}开始下载：文件名：{fileName}, 文件大小：{sizeBytes}");
        }

        /// <summary>
        /// 下载完成
        /// </summary>
        /// <param name="isSucceed"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnDownloadOverFunction(bool isSucceed)
        {
            RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"包{packageName}下载：{ (isSucceed ? "成功" : "失败")}");
        }

        /// <summary>
        /// 更新中
        /// </summary>
        /// <param name="totalDownloadCount"></param>
        /// <param name="currentDownloadCount"></param>
        /// <param name="totalDownloadBytes"></param>
        /// <param name="currentDownloadBytes"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnDownloadProgressUpdateFunction(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
        {
            RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"包{packageName}文件总数：{totalDownloadCount}, 已下载文件数：{currentDownloadCount}, 下载总大小：{totalDownloadBytes}, 已下载大小：{currentDownloadBytes}");
        }

        /// <summary>
        /// 下载出错
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="error"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnDownloadErrorFunction(string fileName, string error)
        {
            RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"包{packageName}下载出错：文件名：{fileName}, 错误信息：{error}");
        }

        static int GetBaiFenBi(int now, int sizeBytes)
        {
            return (int)sizeBytes / now * 100;
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