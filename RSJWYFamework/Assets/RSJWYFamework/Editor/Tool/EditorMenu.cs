using System.IO;
using RSJWYFamework.Editor.Windows;
using RSJWYFamework.Editor.Windows.Config;
using RSJWYFamework.Editor.Windows.HybridCLR;
using RSJWYFamework.Editor.Windows.YooAsset;
using RSJWYFamework.Runtime.Config;
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
        [MenuItem("RSJWYFamework/Config/创建Yoo资源管理参数")]
        public static void CreateYooAssetModuleSettingData()
        {
            UtilityEditor.UtilityEditor.CreateScriptableObject<YooAssetModuleSettingData>(
                "Assets/RSJWYFamework/Resources/YooAssetModuleSetting.asset");
        }

        [MenuItem("RSJWYFamework/Config/创建热更路径设置参数")]
        public static void CreateHCLRToolSetting()
        {
            UtilityEditor.UtilityEditor.CreateScriptableObject<HCLRToolSetting>(
                "Assets/RSJWYFamework/Editor/Setting/HCLRToolSetting.asset");
        }
        [MenuItem("RSJWYFamework/Config/创建项目配置文件")]
        public static void CreateProjectConfig()
        {
            UtilityEditor.UtilityEditor.CreateScriptableObject<ProjectConfig>(
                "Assets/Resources/ProjectConfig.asset");
        }

        [MenuItem("RSJWYFamework/打开热更新系统工具")]
        public static void OpenHybridCLRToolWindows()
        {
            var _windows = OdinEditorWindow.GetWindow<HybridCLRToolWindows>();
            _windows.Show();
            _windows.titleContent = new GUIContent("热更新系统工具");
        }

        [MenuItem("RSJWYFamework/打开配置文件设置工具")]
        public static void OpenSettingConfigWindows()
        {
            var _windows = OdinEditorWindow.GetWindow<SettingConfigWindows>();
            _windows.Show();
            _windows.titleContent = new GUIContent("配置文件设置工具");
        }
        [MenuItem("RSJWYFamework/打开YooAsset资源构建工具")]
        public static void OpenYooAssetBuildWindowsA()
        {
            var _windows = OdinEditorWindow.GetWindow<YooAssetBuildWindow>();
            _windows.titleContent = new GUIContent("YooAsset资源构建工具");
            _windows.Show();
        }
    }
}