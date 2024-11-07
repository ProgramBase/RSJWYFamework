using System.IO;
using HybridCLR.Editor.Commands;
using Newtonsoft.Json;
using RSJWYFamework.Editor.Windows.Config;
using RSJWYFamework.Runtime.HybridCLR;
using RSJWYFamework.Runtime.Utility;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace RSJWYFamework.Editor.Windows.HybridCLR
{
    /// <summary>
    /// 
    /// </summary>
    public class HybridCLRToolWindows : OdinEditorWindow
    {
        [InlineEditor(InlineEditorModes.FullEditor)] [LabelText("配置文件")]
        public HCLRToolSetting SettingData;

        [ReadOnly] public HotCodeDLL HotUpdateDll;


        [Button("构建所有",ButtonSizes.Gigantic)]
        private void BuildAllDLL()
        {
            BuildMetadataForAOTAssemblies();
            BuildHotCode();
            BuildHotUpdateDllJson();
        }

        [Button("构建补充元数据",ButtonSizes.Gigantic)]
        [ButtonGroup("构建按钮")]
        private void BuildMetadataForAOTAssemblies()
        {
            Utility.FileAndFoder.ClearDirectory($"{UtilityEditor.UtilityEditor.GetProjectPath()}{SettingData.BuildMetadataForAOTAssembliesDllPatch}");
            AssetDatabase.Refresh();
            UtilityEditor.UtilityEditor.HybrildCLR.AddMetadataForAOTAssembliesToHCLRSetArr();
            UtilityEditor.UtilityEditor.HybrildCLR.BuildMetadataForAOTAssemblies($"{UtilityEditor.UtilityEditor.GetProjectPath()}{SettingData.BuildMetadataForAOTAssembliesDllPatch}",SettingData.BuildTarget);
        }

        [Button("构建热更代码",ButtonSizes.Gigantic)]
        [ButtonGroup("构建按钮")]
        private void BuildHotCode()
        {
            Utility.FileAndFoder.ClearDirectory($"{UtilityEditor.UtilityEditor.GetProjectPath()}{SettingData.BuildHotCodeDllPatch}");
            AssetDatabase.Refresh();
            UtilityEditor.UtilityEditor.HybrildCLR.BuildHotCode($"{UtilityEditor.UtilityEditor.GetProjectPath()}{SettingData.BuildHotCodeDllPatch}",SettingData.BuildTarget);
        }

        [Button("获取补充元数据列表（手动执行两次）",ButtonSizes.Gigantic)]
        [ButtonGroup("获取信息")]
        private void CompileAndGenerateAOTGenericReference()
        {
            //生成补充元数据表
            AOTReferenceGeneratorCommand.CompileAndGenerateAOTGenericReference();
            AssetDatabase.Refresh();
            UtilityEditor.UtilityEditor.HybrildCLR.AddMetadataForAOTAssembliesToHCLRSetArr();
        }
        [Button("创建热更dll列表",ButtonSizes.Gigantic)]
        [ButtonGroup("获取信息")]
        private void BuildHotUpdateDllJson()
        {
            UtilityEditor.UtilityEditor.HybrildCLR.AddMetadataForAOTAssembliesToHCLRSetArr();
            UtilityEditor.UtilityEditor.HybrildCLR.BuildDLLJson($"{UtilityEditor.UtilityEditor.GetProjectPath()}/{SettingData.GeneratedHotUpdateDLLJson}");
            UpdateHotDLLJson();
        }
       
        
        


        protected override void OnEnable()
        {
            base.OnEnable();
            if (SettingData == null)
            {
                SettingData =
                    AssetDatabase.LoadAssetAtPath<HCLRToolSetting>("Assets/RSJWYFamework/Editor/Setting/HCLRToolSetting.asset");
            }
            //加载热更dll列表
            UpdateHotDLLJson();
        }

        void UpdateHotDLLJson()
        {
            var hotcodedllJson = $"{UtilityEditor.UtilityEditor.GetProjectPath()}/{SettingData.GeneratedHotUpdateDLLJson}";
            HotUpdateDll = JsonConvert.DeserializeObject<HotCodeDLL>(File.ReadAllText(hotcodedllJson));
        }
    }
}