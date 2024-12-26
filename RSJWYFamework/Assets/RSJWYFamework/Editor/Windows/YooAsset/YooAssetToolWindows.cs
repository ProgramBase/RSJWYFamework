using System.Collections.Generic;
using RSJWYFamework.Editor.YooAssetModule;
using RSJWYFamework.Runtime.Config;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Utility;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;
using EBuildPipeline = RSJWYFamework.Runtime.Config.EBuildPipeline;
using YooAsset;

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

        [LabelText("是否清理构建缓存文件")]
        public bool ClearBuildCacheFiles;

        [LabelText("使用资源依赖关系数据库")]
        public bool UseAssetDependencyDB ;
            

        [LabelText("包版本")] public string PackageVersion = "test";

        [LabelText("文件名样式")] public EFileNameStyle FileNameStyle = EFileNameStyle.BundleName_HashName;

        [LabelText("构建后文件拷贝模式")]
        public EBuildinFileCopyOption BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll;

        [LabelText("压缩选项")]
        [DetailedInfoBox("仅在非RawFileBuildPipeline下有效",
            "RawFileBuildPipeline不会打包为AB包，故不支持压缩",
            InfoMessageType.Warning)]
        public ECompressOption CompressOption = ECompressOption.LZ4;
        [LabelText("整理后的打包信息")]
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
                Build(package.PackageName, package.BuildPipeline);
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
        
         private void Build(string packageName, EBuildPipeline buildPipeline)
        {
            Debug.Log($"开始构建 ，包名：{packageName} ———— 目标平台: {buildTarget} ———— 构建管线：{buildPipeline}");

            buildoutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();

            BuildResult buildResult;
            
            if (buildPipeline == EBuildPipeline.BuiltinBuildPipeline)
            {
                BuiltinBuildParameters buildParameters = new BuiltinBuildParameters();
                buildParameters.BuildOutputRoot = buildoutputRoot;
                buildParameters.BuildinFileRoot = streamingAssetsRoot;
                buildParameters.BuildPipeline = buildPipeline.ToString();
                buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle; //必须指定资源包类型
                buildParameters.BuildTarget = buildTarget;
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
                buildParameters.ClearBuildCacheFiles = ClearBuildCacheFiles; //不清理构建缓存，启用增量构建，可以提高打包速度！
                buildParameters.UseAssetDependencyDB = UseAssetDependencyDB; //使用资源依赖关系数据库，可以提高打包速度！

                BuiltinBuildPipeline pipeline = new BuiltinBuildPipeline();
                buildResult = pipeline.Run(buildParameters, false);
            }
            else if (buildPipeline == EBuildPipeline.ScriptableBuildPipeline)
            {
                ScriptableBuildParameters buildParameters = new ScriptableBuildParameters();
                
                buildParameters.BuildOutputRoot = buildoutputRoot;
                buildParameters.BuildinFileRoot = streamingAssetsRoot;
                buildParameters.BuildPipeline = buildPipeline.ToString();
                buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle; //必须指定资源包类型
                buildParameters.BuildTarget = buildTarget;
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
                buildParameters.ClearBuildCacheFiles = ClearBuildCacheFiles; //不清理构建缓存，启用增量构建，可以提高打包速度！
                buildParameters.UseAssetDependencyDB = UseAssetDependencyDB; //使用资源依赖关系数据库，可以提高打包速度！
                
                //删除旧的包，等待修复这个问题
                /*string packageOutputDirectory = buildParameters.GetPackageOutputDirectory();
                Utility.FileAndFolder.DeleteDirectory(packageOutputDirectory);*/
                
                ScriptableBuildPipeline pipeline = new ScriptableBuildPipeline();
                buildResult = pipeline.Run(buildParameters, true);
            }
            else if (buildPipeline == EBuildPipeline.RawFileBuildPipeline)
            {
                RawFileBuildParameters buildParameters = new RawFileBuildParameters();
                buildParameters.BuildOutputRoot = buildoutputRoot;
                buildParameters.BuildinFileRoot = streamingAssetsRoot;
                buildParameters.BuildPipeline = buildPipeline.ToString();
                buildParameters.BuildBundleType = (int)EBuildBundleType.RawBundle; //必须指定资源包类型
                buildParameters.BuildTarget = buildTarget;
                buildParameters.PackageName = packageName;
                buildParameters.PackageVersion = PackageVersion;
                buildParameters.VerifyBuildingResult = true;
                buildParameters.EnableSharePackRule = false; //启用共享资源构建模式，兼容1.5x版本
                buildParameters.FileNameStyle = FileNameStyle;
                buildParameters.BuildinFileCopyOption = BuildinFileCopyOption;
                buildParameters.BuildinFileCopyParams = string.Empty;
                //加密服务
                buildParameters.EncryptionServices = new EncryptAssets();
                buildParameters.ClearBuildCacheFiles = ClearBuildCacheFiles; //不清理构建缓存，启用增量构建，可以提高打包速度！
                buildParameters.UseAssetDependencyDB = UseAssetDependencyDB; //使用资源依赖关系数据库，可以提高打包速度！

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
            public EBuildPipeline BuildPipeline;
            
            
            
            /*2.2.5版本废弃
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
            }*/
        }
    }
    internal enum EBuildBundleType
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 虚拟资源包
        /// </summary>
        VirtualBundle = 1,

        /// <summary>
        /// AssetBundle
        /// </summary>
        AssetBundle = 2,

        /// <summary>
        /// 原生文件
        /// </summary>
        RawBundle = 3,
    }
}