using System;
using System.Collections.Generic;
using System.IO;
using RSJWYFamework.Editor.Windows.Config;
using RSJWYFamework.Editor.YooAssetModule;
using RSJWYFamework.Runtime.Config;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Utility;
using RSJWYFamework.Runtime.YooAssetModule;
using RSJWYFamework.Runtime.YooAssetModule.Tool;
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
        [InlineEditor(InlineEditorModes.FullEditor)] [LabelText("配置文件")] [ReadOnly]
        public YooAssetPackages SettingData;

        [LabelText("构建目标")] public BuildTarget buildTarget = BuildTarget.StandaloneWindows64;

        [BoxGroup("构建目标根路径",centerLabel:true)][HideLabel][LabelText("目录")]
        [FolderPath(AbsolutePath = true, RequireExistingPath = true)] 
        public string buildoutputRoot;
        [BoxGroup("构建目标根路径")]
        [Button("打开文件夹")]
        public void OpenRootFolder()
        {
            Utility.FileAndFolder.OpenFolder(buildoutputRoot);
        }
        [BoxGroup("构建目标根路径")]
        [Button("清空文件夹")]
        public void ClearRootFolder()
        {
            Utility.FileAndFolder.ClearDirectory(buildoutputRoot);
        }

        [FolderPath(AbsolutePath = true, RequireExistingPath = true)] [LabelText("目录")]
        [BoxGroup("streamingAssets根路径",centerLabel:true)][HideLabel]
        public string streamingAssetsRoot;
        
        [BoxGroup("streamingAssets根路径",centerLabel:true)]
        [Button("打开文件夹")]
        public void OpenStreamingAssetFolder()
        {
            Utility.FileAndFolder.OpenFolder(streamingAssetsRoot);
        }
        [BoxGroup("streamingAssets根路径",centerLabel:true)]
        [Button("清空文件夹")]
        public void ClearStreamingAssetFolder()
        {
            Utility.FileAndFolder.ClearDirectory(streamingAssetsRoot);
        }
        
        

        [LabelText("包版本")] public string PackageVersion = "test";

        [LabelText("文件名样式")] public EFileNameStyle FileNameStyle = EFileNameStyle.BundleName_HashName;

        [LabelText("构建后文件拷贝模式")]
        public EBuildinFileCopyOption BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll;

        [LabelText("压缩选项")]
        [DetailedInfoBox("仅在非RawFileBuildPipeline下有效",
            "RawFileBuildPipeline不会打包为AB包，故不支持压缩",
            InfoMessageType.Warning)]
        public ECompressOption CompressOption = ECompressOption.LZ4;

        public List<YooAssetPackageData> YooAssetPackages = new List<YooAssetPackageData>();
       
       
        
        [ButtonGroup("构建")]
        [Button("更新构建配置", ButtonSizes.Large)]
        void UpdateAllPackageSetting()
        {
            YooAssetPackages.Clear();
            foreach (var package in SettingData.packages)
            {
                var _packageData = new YooAssetPackageData()
                {
                    PackageName = package.PackageName,
                    PackageTips = package.PackageTips,
                    BuildPipeline = package.BuildPipeline,
                };
                if (package.BuildPipeline==EDefaultBuildPipeline.BuiltinBuildPipeline)
                {
                    _packageData.BuildMode = EBuildMode.ForceRebuild.ToString();
                }
                else if (package.BuildPipeline==EDefaultBuildPipeline.ScriptableBuildPipeline)
                {
                    _packageData.BuildMode = EBuildMode.IncrementalBuild.ToString();
                }
                else if (package.BuildPipeline == EDefaultBuildPipeline.RawFileBuildPipeline)
                {
                    _packageData.BuildMode = EBuildMode.ForceRebuild.ToString();
                }

               
                YooAssetPackages.Add(_packageData);
            }
        }

        [ButtonGroup("构建")]
        [Button("构建所有包", ButtonSizes.Gigantic)]
        void BuildAllPackage()
        {
            if (YooAssetPackages.Count <= 0)
            {
                throw new RSJWYException("请先更新构建配置");
            }

            foreach (var package in YooAssetPackages)
            {
                EBuildMode buildMode;

                if (!Enum.TryParse(package.BuildMode, out buildMode))
                {
                    throw new RSJWYException($"不支持的构建模式 : {buildMode}");
                }
                Build(package.PackageName, package.BuildPipeline, buildMode);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (SettingData == null)
            {
                SettingData =
                    AssetDatabase.LoadAssetAtPath<YooAssetPackages>("Assets/Resources/YooAssetPackages.asset");
            }

            buildoutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot(); 
            UpdateAllPackageSetting();
        }
        
         private void Build(string packageName, EDefaultBuildPipeline buildPipeline,EBuildMode buildMode)
        {
            Debug.Log($"开始构建 ，包名：{packageName} ———— 目标平台: {buildTarget} ———— 构建管线：{buildPipeline}");

            buildoutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();

            BuildResult buildResult;
            
            if (buildPipeline == EDefaultBuildPipeline.BuiltinBuildPipeline)
            {
                BuiltinBuildParameters buildParameters = new BuiltinBuildParameters();
                buildParameters.BuildOutputRoot = buildoutputRoot;
                buildParameters.BuildinFileRoot = streamingAssetsRoot;
                buildParameters.BuildPipeline = buildPipeline.ToString();
                buildParameters.BuildTarget = buildTarget;
                buildParameters.BuildMode = buildMode; 
                buildParameters.PackageName = packageName;
                buildParameters.PackageVersion = PackageVersion;
                buildParameters.VerifyBuildingResult = true;
                buildParameters.EnableSharePackRule = false; //启用共享资源构建模式，兼容1.5x版本
                buildParameters.FileNameStyle = FileNameStyle;
                buildParameters. BuildinFileCopyOption = BuildinFileCopyOption;
                buildParameters.BuildinFileCopyParams = string.Empty;
                buildParameters.CompressOption = CompressOption; 
                //加密服务
                buildParameters.EncryptionServices = new EncryptAssets();

                BuiltinBuildPipeline pipeline = new BuiltinBuildPipeline();
                buildResult = pipeline.Run(buildParameters, false);
            }
            else if (buildPipeline == EDefaultBuildPipeline.ScriptableBuildPipeline)
            {
                ScriptableBuildParameters buildParameters = new ScriptableBuildParameters();
                
                buildParameters.BuildOutputRoot = buildoutputRoot;
                buildParameters.BuildinFileRoot = streamingAssetsRoot;
                buildParameters.BuildPipeline = buildPipeline.ToString();
                buildParameters.BuildTarget = buildTarget;
                buildParameters.BuildMode = buildMode;
                buildParameters.PackageName = packageName;
                buildParameters.PackageVersion = PackageVersion;
                buildParameters.VerifyBuildingResult = true;
                buildParameters.EnableSharePackRule = false; //启用共享资源构建模式，兼容1.5x版本
                buildParameters.FileNameStyle = FileNameStyle;
                buildParameters.BuildinFileCopyOption = BuildinFileCopyOption;
                buildParameters.BuildinFileCopyParams = string.Empty;
                buildParameters.CompressOption = CompressOption;
                    //加密服务
                    buildParameters.EncryptionServices = new EncryptAssets();
                
                string packageOutputDirectory = buildParameters.GetPackageOutputDirectory();
                Utility.FileAndFolder.DeleteDirectory(packageOutputDirectory);
                
                ScriptableBuildPipeline pipeline = new ScriptableBuildPipeline();
                buildResult = pipeline.Run(buildParameters, true);
            }
            else if (buildPipeline == EDefaultBuildPipeline.RawFileBuildPipeline)
            {
                RawFileBuildParameters buildParameters = new RawFileBuildParameters();
                buildParameters.BuildOutputRoot = buildoutputRoot;
                buildParameters.BuildinFileRoot = streamingAssetsRoot;
                buildParameters.BuildPipeline = buildPipeline.ToString();
                buildParameters.BuildTarget = buildTarget;
                buildParameters.BuildMode = buildMode;
                buildParameters.PackageName = packageName;
                buildParameters.PackageVersion = PackageVersion;
                buildParameters.VerifyBuildingResult = true;
                buildParameters.EnableSharePackRule = false; //启用共享资源构建模式，兼容1.5x版本
                buildParameters.FileNameStyle = FileNameStyle;
                buildParameters.BuildinFileCopyOption = BuildinFileCopyOption;
                buildParameters.BuildinFileCopyParams = string.Empty;

                RawFileBuildPipeline pipeline = new RawFileBuildPipeline();
                buildResult = pipeline.Run(buildParameters, true);
            }
            else
            {
                throw new RSJWYException($"不支持的构建管线 : {buildPipeline}");
            }

            if (buildResult.Success)
            {
                Debug.Log($"构建成功 : {buildResult.OutputPackageDirectory}");
                
            }
            else
            {
                Debug.LogError($"构建失败 : {buildResult.ErrorInfo}");
            }
        }

        /// <summary>
        ///包信息参数
        /// </summary>
        public class YooAssetPackageData
        {
            [LabelText("包名")] [Required("必须输入包名称，程序不做检测")]
            public string PackageName;

            [LabelText("包说明")] 
            public string PackageTips;

            [LabelText("构建管线")] [Required("必须选择构建管线，程序不做检测")]
            [ReadOnly]
            public EDefaultBuildPipeline BuildPipeline;

            [ValueDropdown("BuildModeSelect")] 
            public string BuildMode;
            
            
            private IEnumerable<string> BuildModeSelect()
            {
                var list = new List<string>();
                if (BuildPipeline==EDefaultBuildPipeline.BuiltinBuildPipeline)
                {
                    list.Add(EBuildMode.ForceRebuild.ToString());
                    list.Add(EBuildMode.IncrementalBuild.ToString());
                    list.Add(EBuildMode.DryRunBuild.ToString());
                    list.Add(EBuildMode.SimulateBuild.ToString());
                    return list.ToArray();
                }
                else if (BuildPipeline == EDefaultBuildPipeline.ScriptableBuildPipeline)
                {
                    list.Add(EBuildMode.IncrementalBuild.ToString());
                    list.Add(EBuildMode.SimulateBuild.ToString());
                    return list.ToArray();
                }
                if (BuildPipeline==EDefaultBuildPipeline.RawFileBuildPipeline)
                {
                    list.Add(EBuildMode.ForceRebuild.ToString());
                    list.Add(EBuildMode.SimulateBuild.ToString());
                    return list.ToArray();
                }
                else
                {
                    throw new RSJWYException("不支持的构建管线");
                }
            }
        }
    }
}