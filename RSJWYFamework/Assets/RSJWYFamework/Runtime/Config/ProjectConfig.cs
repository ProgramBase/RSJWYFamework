using RSJWYFamework.Runtime.Data;
using RSJWYFamework.Runtime.YooAssetModule;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine.Serialization;

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
        [LabelText("AES加密密钥")]
        public string AESKey;
        
        /// <summary>
        /// 绑定的许可ID
        /// </summary>
        [Required("请输入授权绑定ID")] 
        [InfoBox("默认使用0号许可，发行时请更改为其他许可")]
        public int licenseID;

        /// <summary>
        /// 开发者密钥
        /// 注意，每个开发者SDK和密钥一一绑定
        /// 请使用正确的SDK和开发者密钥
        /// </summary>
        [Required("请输入开发者密钥")] 
        [DetailedInfoBox("输入平台给的开发者密钥",
            "注意，每个开发者SDK和密钥一一绑定,请使用正确的SDK和开发者密钥，否则程序初始化不通过\n请登录 Virbox 开发者中心(https://developer.lm.virbox.com), 获取 API 密码",
            InfoMessageType.Warning)]
        public string developerPW;
        
        
        


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