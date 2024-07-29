using System;
using RSJWYFamework.Editor.Windows.Config;
using RSJWYFamework.Runtime.YooAssetModule;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

namespace RSJWYFamework.Editor.Windows.YooAsset
{
    public class YooAssetBuildWindow : OdinEditorWindow
    {
        [InlineEditor(InlineEditorModes.FullEditor)]
        [LabelText("配置文件")][ReadOnly]
        public YooAssetModuleSettingData SettingData;
        
        [LabelText("构建目标")]
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
        
        [FolderPath(AbsolutePath = true,RequireExistingPath = true)]
        [LabelText("构建目标根路径")]
        public string buildoutputRoot;
        
        [FolderPath(AbsolutePath = true,RequireExistingPath = true)]
        [LabelText("streamingAssets根路径")]
        public string streamingAssetsRoot;
        
        [LabelText("构建模式")]
        public EBuildMode BuildMode=EBuildMode.ForceRebuild;

        [LabelText("包版本")]
        public string PackageVersion ;

        [LabelText("文件名样式")]
        public EFileNameStyle FileNameStyle=EFileNameStyle.BundleName_HashName;

        [LabelText("构建后文件拷贝模式")]
        public EBuildinFileCopyOption BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll;

        [LabelText("压缩选项")]
        public ECompressOption CompressOption = ECompressOption.LZ4;
        
        [Button("构建Prefab包",ButtonSizes.Gigantic)]
        [ButtonGroup("构建按钮")]
        void BuildPrefabPackage()
        {
            Build(SettingData.Prefab.PackageName,SettingData.Prefab.BuildPipeline);
        }
        [Button("构建RawFile包",ButtonSizes.Gigantic)]
        [ButtonGroup("构建按钮")]
        void BuildRawFilePackage()
        {
            Build(SettingData.RawFile.PackageName,SettingData.Prefab.BuildPipeline);
        }
        
        
        
        private void Build(string PackageName,EDefaultBuildPipeline BuildPipeline)
        {
            Debug.Log($"开始构建 ，包名：{PackageName} ———— 目标平台: {buildTarget} ———— 构建管线：{BuildPipeline}");

            buildoutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
    
            // 构建参数
            BuiltinBuildParameters buildParameters = new BuiltinBuildParameters();
            buildParameters.BuildOutputRoot = buildoutputRoot;
            buildParameters.BuildinFileRoot = streamingAssetsRoot;
            buildParameters.BuildPipeline = BuildPipeline.ToString();
            buildParameters.BuildTarget = buildTarget;
            buildParameters.BuildMode = BuildMode;
            buildParameters.PackageName = PackageName;
            buildParameters.PackageVersion = PackageVersion;
            buildParameters.VerifyBuildingResult = true;
            buildParameters.EnableSharePackRule = true; //启用共享资源构建模式，兼容1.5x版本
            buildParameters.FileNameStyle = FileNameStyle;
            buildParameters.BuildinFileCopyOption = BuildinFileCopyOption;
            buildParameters.BuildinFileCopyParams = string.Empty;
            //buildParameters.EncryptionServices = CreateEncryptionInstance();
            buildParameters.CompressOption = CompressOption;
    
            // 执行构建
            BuiltinBuildPipeline pipeline = new BuiltinBuildPipeline();
            var buildResult = pipeline.Run(buildParameters, true);
            if (buildResult.Success)
            {
                Debug.Log($"构建成功 : {buildResult.OutputPackageDirectory}");
            }
            else
            {
                Debug.LogError($"构建失败 : {buildResult.ErrorInfo}");
            }
        }
        
        
        
        
        protected override void OnEnable()
        {
            base.OnEnable();
            if (SettingData == null)
            {
                SettingData = AssetDatabase.LoadAssetAtPath<YooAssetModuleSettingData>("Assets/Resources/YooAssetModuleSetting.asset");
            }
            buildoutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();

        }
    }
}