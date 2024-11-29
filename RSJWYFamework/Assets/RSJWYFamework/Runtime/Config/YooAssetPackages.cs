using System;
using System.Collections.Generic;
using RSJWYFamework.Runtime.Data;
using Sirenix.OdinInspector;
using YooAsset;

namespace RSJWYFamework.Runtime.Config
{
    [Serializable]
    public class  YooAssetPackages:DataBaseSB
    {
        public List<YooAssetModulePackageData> packages;
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
        [LabelText("包说明")]
        public string PackageTips;
        [LabelText("构建管线")]
        [Required("必须选择构建管线，程序不做检测")] 
        public EDefaultBuildPipeline BuildPipeline;
    }
    
}

