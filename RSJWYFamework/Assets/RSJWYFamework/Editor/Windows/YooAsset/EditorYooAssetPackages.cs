using RSJWYFamework.Runtime.Config;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;

namespace RSJWYFamework.Editor.Windows.YooAsset
{
    [CustomEditor(typeof(YooAssetPackages))]
    public class EditorYooAssetPackages : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // 绘制默认的Inspector
            DrawDefaultInspector();

            // 绘制一个按钮
            if (GUILayout.Button("从YooAsset加载包信息【自行配置管线相关信息】"))
            {
                // 执行按钮点击后的操作
                var settingData = target as YooAssetPackages;
                if (settingData)
                {
                    settingData.packages.Clear();
                    var yooData = SettingLoader.LoadSettingData<AssetBundleCollectorSetting>();
                    var packages = yooData.Packages;
                    foreach (var package in packages)
                    {
                        var yooAssetPackageData = new YooAssetModulePackageData();
                        yooAssetPackageData.PackageName = package.PackageName;
                        yooAssetPackageData.PackageTips = package.PackageDesc;
                        settingData.packages.Add(yooAssetPackageData);
                    }
                    Debug.Log($"包数量为{settingData.packages.Count}");
                }
                else
                {
                    Debug.LogError($"找不到包列表");
                }
            }
        }
    }
}