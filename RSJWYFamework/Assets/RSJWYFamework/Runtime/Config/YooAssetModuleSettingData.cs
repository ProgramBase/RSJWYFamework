using System;
using System.Collections.Generic;
using RSJWYFamework.Runtime.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule
{
    public class YooAssetModuleSettingData:DataBaseSB
    {
        [Required][LabelText("运行模式")]
        public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;
        [LabelText("预制体包")]
        public YooAssetModulePackageData PrefabP;
         [LabelText("原生文件包")]
        public YooAssetModulePackageData RawFileP;

        [BoxGroup("Host配置")]
        [LabelText("资源根地址")]
        [ShowIf("PlayMode", EPlayMode.HostPlayMode)]
        public string hostServerIP="http://127.0.0.1";
        
    }
    
    
    /// <summary>
    ///包信息参数
    /// </summary>
    [Serializable]
    public class YooAssetModulePackageData:DataBase
    {
        [LabelText("包名")]
        [Required("必须输入包名称，程序不做检测")] 
        public string PackageName;
        [LabelText("构建管线")]
        [Required("必须选择构建管线，程序不做检测")] 
        public EDefaultBuildPipeline BuildPipeline;
        
    }
    
}

