using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            /// <param name="BuildMetadataForAOTAssembliesDllPatch">AOTDLL构建目标目录</param>
            public static void BuildMetadataForAOTAssemblies(string BuildMetadataForAOTAssembliesDllPatch,BuildTarget buildTarget)
            {
                Debug.Log($"构建生成补充元数据程序集DLL，目标平台：{buildTarget}");
                //构建补充元数据
                StripAOTDllCommand.GenerateStripedAOTDlls();
                //拷贝到资源包
                Debug.Log($"拷贝补充元数据到资源包，构建模式为{buildTarget.ToString()}");
                //获取构建DLL的路径
                var aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(buildTarget);
                Utility.FileAndFoder.CheckDirectoryExistsAndCreate(BuildMetadataForAOTAssembliesDllPatch);
                Parallel.ForEach(SettingsUtil.AOTAssemblyNames, aotDll =>
                {
                    string srcDllPath = $"{aotAssembliesSrcDir}/{aotDll}.dll";
                    if (!File.Exists(srcDllPath))
                    {
                        Debug.LogError(
                            $"添加AOT补充元数据dll:{srcDllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先运行一次ALL后再打包。");
                    }
                    string dllBytesPath = $"{BuildMetadataForAOTAssembliesDllPatch}/{aotDll}.dll.bytes";
                    File.Copy(srcDllPath, dllBytesPath, true);
                    Debug.Log($"[拷贝补充元数据到热更包] 拷贝 {srcDllPath} -> 到{dllBytesPath}");
                });
                AssetDatabase.Refresh();
            }

            /// <summary>
            /// 构建热更代码
            /// </summary>
            /// <param name="BuildHotCodeDllPatch">构建后DLL路径</param>
            public static void BuildHotCode(string BuildHotCodeDllPatch,BuildTarget buildTarget)
            {
                if (!AutoSaveScence())
                {
                    RSJWYLogger.Error("场景保存失败");
                    return;
                }
                Debug.Log($"构建生成热更新程序集DLL，目标平台：{buildTarget}");
                //构建热更新代码
                CompileDllCommand.CompileDll(buildTarget);
                //拷贝到资源包
                Debug.Log($"拷贝热更新代码到资源包，构建模式为{buildTarget.ToString()}");
                //获取构建DLL的路径
                var hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget);
                Utility.FileAndFoder.CheckDirectoryExistsAndCreate(BuildHotCodeDllPatch);
                //拷贝到资源-并行拷贝
                Parallel.ForEach(SettingsUtil.HotUpdateAssemblyNamesExcludePreserved, hotDll =>
                {
                    //拷贝热更代码
                    string startDllPath = $"{hotfixDllSrcDir}/{hotDll}.dll";
                    string endDllBytePath = $"{BuildHotCodeDllPatch}/{hotDll}.dll.bytes";
                    File.Copy(startDllPath, endDllBytePath, true);
                    Debug.Log($"[拷贝热更代码到热更包] 拷贝 {startDllPath} -> 到{endDllBytePath}");
                });
                Parallel.ForEach(SettingsUtil.HotUpdateAssemblyNamesExcludePreserved, hotDll =>
                {
                    //拷贝PDB
                    string startPdbPath = $"{hotfixDllSrcDir}/{hotDll}.pdb";
                    string endPdbBytePath = $"{BuildHotCodeDllPatch}/{hotDll}.pdb.bytes"; 
                    File.Copy(startPdbPath, endPdbBytePath, true);
                    Debug.Log($"[拷贝热更代码PDB到热更包] 拷贝 {startPdbPath} -> 到{endPdbBytePath}");
                });
                AssetDatabase.Refresh();
            }
            
            /// <summary>
            /// 构建补充元数据列表到HybridCLR设置
            /// </summary>
            public static void AddMetadataForAOTAssembliesToHCLRSetArr()
            {
                //生成补充元数据表
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
                Utility.FileAndFoder.CheckDirectoryAndFileCreate(GeneratedHotUpdateDLLJsonPath);
                File.WriteAllText(GeneratedHotUpdateDLLJsonPath, hclrLoadDLLJsonFile.ToString());
                AssetDatabase.Refresh();
            }
            
            
        }
    }
}