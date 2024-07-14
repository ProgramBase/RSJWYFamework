using System.IO;
using RSJWYFamework.Editor.Windows;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.YooAssetModule;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace RSJWYFamework.Editor.Tool
{
    public class EditorMenu
    {
        [MenuItem("RSJWYFamework/创建Yoo资源管理参数")]
        public static void CreateMyData()
        {
            UtilityEditor.UtilityEditor.CreateScriptableObject<YooAssetModuleSettingData>("Assets/RSJWYFamework/Prefab/YooAssetModuleSetting.asset");
        }
        [MenuItem("RSJWYFamework/打开热更新系统工具")]
        public static void OpenHybridCLRToolWindows()
        {
            var _windows = OdinEditorWindow.GetWindow<HybridCLRTool>();
            _windows.Show();
            _windows.titleContent = new GUIContent("热更新系统工具");
        }
    }
}