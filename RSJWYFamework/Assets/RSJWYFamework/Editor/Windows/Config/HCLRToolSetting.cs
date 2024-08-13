using Sirenix.OdinInspector;
using UnityEngine;

namespace RSJWYFamework.Editor.Windows.Config
{
    [CreateAssetMenu(fileName = "HCLRToolSettingData", menuName = "RSJWYFamework/创建热更设置参数", order = 0)]
    public class HCLRToolSetting : ScriptableObject
    {
        [FolderPath][LabelText("构建热更代码DLL到：")]
        public string BuildHotCodeDllPatch="Assets/HotUpdateAssets/HotCode/HotCode";
        [FolderPath][LabelText("构建补充选数据DLL到：")]
        public string BuildMetadataForAOTAssembliesDllPatch="Assets/HotUpdateAssets/HotCode/MetadataForAOTAssemblies";
        [FilePath][LabelText("构建热更列表文件路径：")]
        public string GeneratedHotUpdateDLLJson="Assets/HotUpdateAssets/HotCode/List/HotCodeDLL.json"; 
    }
}
