using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Procedure;
using UnityEngine;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.Procedure
{
    /// <summary>
    /// 完成更新流程
    /// </summary>
    public class UpdaterDoneProcedure:ProcedureBase
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
            var packageVersion = (string)pc.GetBlackboardValue("PackageVersion");
            var playMode = (EPlayMode)pc.GetBlackboardValue("PlayMode");
            RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"完成包{packageName}更新流程，版本：{packageVersion}");
            if (playMode == EPlayMode.OfflinePlayMode)
            {
                //弱联网环境配置
                // 注意：下载完成之后再保存本地版本
                PlayerPrefs.SetString($"YooAssets_{packageName}_Version", packageVersion);
                RSJWYLogger.Log($"运行模式--OfflinePlayMode--保存包：{packageName}--版本：{packageVersion}");
                var networkNormal = (bool)pc.GetBlackboardValue("NetworkNormal");
                if (networkNormal)
                {
                    RSJWYLogger.Warning("检测到网络不可达，将以上一次获取到的完整包版本启动本程序");
                }
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