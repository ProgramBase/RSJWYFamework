using System;
using System.Collections.Generic;
using RSJWYFamework.Runtime.Data.Base;
using Sirenix.OdinInspector;
using UnityEngine;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule
{
    [CreateAssetMenu(fileName = "YooAssetModuleSettingData", menuName = "RSJWYFamework/创建Yoo资源管理参数", order = 0)]
    public class YooAssetModuleSettingData:DataBaseSB
    {
        [Required]
        public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;
        [ListDrawerSettings(ShowIndexLabels =true)]
        public List<YooAssetModulePackageData> package;
        
       
    }
    
    
    
    /// <summary>
    ///包信息参数
    /// </summary>
    [Serializable]
    public class YooAssetModulePackageData:DataBase
    {
        [Required("必须输入包名称，程序不做检测")] public string PackageName;

        [Required("必须选择构建管线，程序不做检测")] public EDefaultBuildPipeline BuildPipeline;

        /// <summary>
        /// 是否初始化
        /// </summary>
        [HideInInspector]
        public bool InitOk = false;

        /// <summary>
        /// 初始化后的包类
        /// </summary>
        [HideInInspector] public ResourcePackage Package;
        
        /// <summary>
        /// 初始化后的包类
        /// </summary>
        [HideInInspector] public string PackageVersion;
        
        
    }
    
}

