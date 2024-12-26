using System;
using System.Collections.Generic;
using RSJWYFamework.Runtime.Data;
using Sirenix.OdinInspector;

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
        [Required("必须输入构建管线，程序不做检测--仅构建包时使用")]
        public EBuildPipeline BuildPipeline;
    }
    
    /// <summary>
    /// 构建管线类型
    /// </summary>
    public enum EBuildPipeline
    {
        /// <summary>
        /// 编辑器下的模拟构建管线（ESBP）
        /// </summary>
        EditorSimulateBuildPipeline,

        /// <summary>
        /// 传统内置构建管线 (BBP)
        /// </summary>
        BuiltinBuildPipeline,

        /// <summary>
        /// 可编程构建管线 (SBP)
        /// </summary>
        ScriptableBuildPipeline,

        /// <summary>
        /// 原生文件构建管线 (RFBP)
        /// </summary>
        RawFileBuildPipeline,
    }
}

