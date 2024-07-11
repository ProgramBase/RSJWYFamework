using System;
using System.Collections.Generic;
using RSJWYFamework.Runtime.Data.Base;
using Sirenix.OdinInspector;
using UnityEngine;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule
{
    [CreateAssetMenu(fileName = "YooAssetModuleSettingData", menuName = "RSJWYFamework/创建Yoo资源管理参数", order = 0)]
    public class YooAssetModuleSetting:DataBaseSB
    {
        [Required]
        public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;
        [ListDrawerSettings(ShowIndexLabels =true)]
        public List<YooAssetModulePackageData> package;
        
        /// <summary>
        ///包信息参数
        /// </summary>
        [Serializable]
        public class YooAssetModulePackageData
        {
            [Required("必须输入包名称，程序不做检测")] public string PackagesName;

            [Required("必须选择构建管线，程序不做检测")] public EDefaultBuildPipeline BuildPipeline;

            /// <summary>
            /// 是否初始化
            /// </summary>
            [HideInInspector]
            public bool InitOk = false;

            /// <summary>
            /// 
            /// </summary>
            [HideInInspector] public ResourcePackage Package;
        }
    }
    
    
    
    
    
}

