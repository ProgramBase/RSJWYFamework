using Sirenix.OdinInspector;
using UnityEngine;

namespace RSJWYFamework.Editor.Windows.Config
{
    [CreateAssetMenu(fileName = "HCLRToolSettingData", menuName = "RSJWYFamework/创建热更设置参数", order = 0)]
    public class HCLRToolSetting : ScriptableObject
    {
        [FolderPath][LabelText("构建热更代码DLL到：")]
        public string BuildHotCodeDllPatch="Assets/HotUpdateAssets/HotCode";
        [FolderPath][LabelText("构建补充选数据DLL到：")]
        public string BuildMetadataForAOTAssembliesDllPatch="Assets/HotUpdateAssets/MetadataForAOTAssemblies";
        [FilePath]
        public string GeneratedHotUpdateDLLJson="Assets/HotUpdateAssets/Config/HotCodeDLL.json"; 
    }
}
