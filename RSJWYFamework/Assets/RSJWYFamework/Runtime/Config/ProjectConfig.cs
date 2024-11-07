﻿using RSJWYFamework.Runtime.Data;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.YooAssetModule;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine.Serialization;
using YooAsset;

#if UNITY_EDITOR
using UnityEngine;
#endif

namespace RSJWYFamework.Runtime.Config
{
    /// <summary>
    /// 项目配置文件
    /// </summary>
    public class ProjectConfig:DataBaseSB
    {
        [LabelText("项目名称")]
        public string ProjectName = "测试工程";
        [LabelText("软件名")]
        public string APPName = "测试软件";
        [LabelText("软件版本")]
        public string Version = "TestV0.1";
        
        [FoldoutGroup("YooAssetInfo")]
        [InlineEditor(InlineEditorModes.FullEditor)] 
        public YooAssetPackages YooAssets;
        
        [FoldoutGroup("YooAssetInfo")]
        [Required][LabelText("运行模式")]
        public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;
        [FoldoutGroup("VirBoxInfo")]
        
        [FoldoutGroup("YooAssetInfo")]
        [BoxGroup("Host配置")]
        [LabelText("资源根地址")]
        [ShowIf("PlayMode", EPlayMode.HostPlayMode)]
        public string hostServerIP="http://127.0.0.1";

        /// <summary>
        /// 绑定的许可ID
        /// </summary>
        [FoldoutGroup("VirBoxInfo")]
        [Required("请输入授权绑定ID")] 
        [InfoBox("默认使用0号许可，发行时请更改为其他许可")]
        public int licenseID;

        /// <summary>
        /// 开发者密钥
        /// 注意，每个开发者SDK和密钥一一绑定
        /// 请使用正确的SDK和开发者密钥
        /// </summary>
        [FoldoutGroup("VirBoxInfo")]
        [LabelText("开发者密钥")]
        [Required("请输入开发者密钥")] 
        [DetailedInfoBox("输入平台给的开发者密钥",
            "注意，每个开发者SDK和密钥一一绑定,请使用正确的SDK和开发者密钥，否则程序初始化不通过\n请登录 Virbox 开发者中心(https://developer.lm.virbox.com), 获取 API 密码",
            InfoMessageType.Warning)]
        public string developerPW;

        [LabelText("日志等级")]
        public Loglevel Loglevel=Loglevel.LOG;
        
        [Required("请输入加密密钥")] 
        [LabelText("AES加密密钥")]
        public string AESKey;

#if UNITY_EDITOR
        [Button("设置项目参数")]
        public void SetSetting()
        {
            PlayerSettings.companyName = ProjectName;
            PlayerSettings.productName = APPName;
            PlayerSettings.bundleVersion = Version;
            Debug.Log("参数设置完成");
        }
#endif
    }
}