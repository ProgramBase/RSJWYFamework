using RSJWYFamework.Runtime.Data;
using RSJWYFamework.Runtime.YooAssetModule;
using Sirenix.OdinInspector;
using UnityEditor;

#if UNITY_EDITOR
using UnityEngine;
#endif

namespace RSJWYFamework.Runtime.Config
{
    /// <summary>
    /// 项目配置文件
    /// </summary>
    public class ProjectConfig:DataBaseSB
    {
        [LabelText("项目名称")]
        public string ProjectName = "测试工程";
        [LabelText("软件名")]
        public string APPName = "测试软件";
        [LabelText("软件版本")]
        public string Version = "TestV0.1";
        
        [InlineEditor(InlineEditorModes.FullEditor)] 
        public YooAssetModuleSettingData YooAssets;
        
        [Required("请输入加密密钥")] 
        public string AESKey;
        
        [Required("请输入授权绑定ID")] 
        public int VirboxID;


#if UNITY_EDITOR
        [Button("设置项目参数")]
        public void SetSetting()
        {
            PlayerSettings.companyName = ProjectName;
            PlayerSettings.productName = APPName;
            PlayerSettings.bundleVersion = Version;
            Debug.Log("参数设置完成");
        }
#endif
    }
}