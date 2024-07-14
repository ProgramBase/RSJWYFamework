using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Settings;
using Newtonsoft.Json.Linq;
using RSJWYFamework.Editor.Tool;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Utility;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RSJWYFamework.Editor.UtilityEditor
{
    public static partial class UtilityEditor
    {
        public static class HybrildCLR
        {
            /// <summary>
            /// 构建补充元数据到资源文件夹
            /// </summary>
            /// <param name="BuildMetadataForAOTAssembliesDllPatch"></param>
            public static void BuildMetadataForAOTAssemblies(string BuildMetadataForAOTAssembliesDllPatch)
            {
                var buildTarget = EditorUserBuildSettings.activeBuildTarget;
                Debug.Log($"构建生成补充元数据程序集DLL，目标平台：{buildTarget}");
                //构建补充元数据
                StripAOTDllCommand.GenerateStripedAOTDlls();
                //拷贝到资源包
                Debug.Log($"拷贝补充元数据到资源包，构建模式为{buildTarget.ToString()}");
                //获取构建DLL的路径
                var aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(buildTarget);
                var yoodllPath = $"{GetProjectPath()}/{BuildMetadataForAOTAssembliesDllPatch}";
                Utility.FileAndFoder.CheckDirectoryExistsAndCreate(yoodllPath);
                //迭代资源目录
                foreach (var dll in SettingsUtil.AOTAssemblyNames)
                {
                    string srcDllPath = $"{aotAssembliesSrcDir}/{dll}.dll";
                    if (!File.Exists(srcDllPath))
                    {
                        Debug.LogError(
                            $"添加AOT补充元数据dll:{srcDllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先运行一次ALL后再打包。");
                        continue;
                    }

                    string dllBytesPath = $"{yoodllPath}/{dll}.dll.bytes";
                    //File.Copy(srcDllPath, dllBytesPath, true);
                    // byte[] _rawByte=File.ReadAllBytes(srcDllPath);
                    // byte[] _aesByte= MyTool_AOT.AESEncrypt(_rawByte,MyTool_AOT.AESkey);
                    // File.WriteAllBytes(dllBytesPath,_aesByte);
                    File.Copy(srcDllPath, dllBytesPath, true);
                    Debug.Log($"[拷贝补充元数据到热更包] 拷贝 {srcDllPath} -> 到{dllBytesPath}");
                }

                AssetDatabase.Refresh();
            }

            /// <summary>
            /// 构建热更代码
            /// </summary>
            /// <param name="BuildHotCodeDllPatch">构建后DLL路径</param>
            public static void BuildHotCode(string BuildHotCodeDllPatch)
            {
                if (!AutoSaveScence())
                {
                    RSJWYLogger.LogError("场景保存失败");
                    return;
                }
                BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
                Debug.Log($"构建生成热更新程序集DLL，目标平台：{target}");
                //构建热更新代码
                CompileDllCommand.CompileDll(target);
                //拷贝到资源包
                Debug.Log($"拷贝热更新代码到资源包，构建模式为{target.ToString()}");
                //获取构建DLL的路径
                var hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
                var yoodllPath = $"{GetProjectPath()}/{BuildHotCodeDllPatch}";
                Utility.FileAndFoder.CheckDirectoryExistsAndCreate(yoodllPath);
                //拷贝到资源
                foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
                {
                    string dllPath = $"{hotfixDllSrcDir}/{dll}";
                    string dllBytesPath = $"{yoodllPath}/{dll}.bytes";//File.Copy(srcDllPath, dllBytesPath, true);
                    File.Copy(dllPath, dllBytesPath, true);
                    Debug.Log($"[拷贝热更代码到热更包] 拷贝 {dllPath} -> 到{dllBytesPath}");
                }
                AssetDatabase.Refresh();
            }
            
            /// <summary>
            /// 构建补充元数据列表到HybridCLR设置
            /// </summary>
            public static void AddMetadataForAOTAssembliesToHCLRSetArr()
            {
                //生成补充元数据表
                AOTReferenceGeneratorCommand.CompileAndGenerateAOTGenericReference();
                AssetDatabase.Refresh();
                var aotdlls = AOTGenericReferences.PatchedAOTAssemblyList.ToList();
                //处理信息
                var temp = new List<string>();
                foreach (string str in aotdlls)
                {
                    var _s = str.Replace(".dll", "");
                    temp.Add(_s);
                }
                //保存处理的数据
                HybridCLRSettings.Instance.patchAOTAssemblies = temp.ToArray();
                HybridCLRSettings.Save();
                AssetDatabase.Refresh();
                //输出结果方便复制
                StringBuilder listStr = new();
                listStr.Append($"\n");
                foreach (string str in aotdlls)
                {
                    listStr.Append($"\n{str}.bytes");
                }
                listStr.Append($"\n");
                AssetDatabase.Refresh();
                Debug.Log($"补充元数据表如下{listStr.ToString()}");
                AssetDatabase.Refresh();
            }
            
            /// <summary>
            /// 根据列表生成Json文件
            /// </summary>
            public static void BuildDLLJson(string GeneratedHotUpdateDLLJsonPath)
            {
                //读取列表
                var aotAssemblies = HybridCLRSettings.Instance.patchAOTAssemblies.ToList();
                var hotDllDef = HybridCLRSettings.Instance.hotUpdateAssemblyDefinitions.ToList();
                //List<string> _HotdllName = HybridCLRSettings.Instance.hotUpdateAssemblies.ToList();
                List<string> hotdlls = new();
                //整理
                foreach (AssemblyDefinitionAsset dlldef in hotDllDef)
                {
                    hotdlls.Add(dlldef.name);
                }
                JObject hclrLoadDLLJsonFile = new();
                hclrLoadDLLJsonFile.Add("MetadataForAOTAssemblies", JArray.FromObject(aotAssemblies));
                hclrLoadDLLJsonFile.Add("HotCode", JArray.FromObject(hotdlls));
                string path =$"{UtilityEditor.GetProjectPath()}/{GeneratedHotUpdateDLLJsonPath}/HotCodeDLL.json";
                Utility.FileAndFoder.CheckDirectoryAndFileCreate($"{UtilityEditor.GetProjectPath()}/{GeneratedHotUpdateDLLJsonPath}", "HotCodeDLL.json");
                AssetDatabase.Refresh();
                File.WriteAllText(path, hclrLoadDLLJsonFile.ToString());
                AssetDatabase.Refresh();
            }
            
            
        }
    }
}