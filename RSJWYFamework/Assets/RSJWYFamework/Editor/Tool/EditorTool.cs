using System.IO;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.YooAssetModule;
using UnityEditor;
using UnityEngine;

namespace RSJWYFamework.Editor.Tool
{
    public class EditorTool
    {
        [MenuItem("RSJWYFamework/创建Yoo资源管理参数")]
        public static void CreateMyData()
        {
            string path = "Assets/RSJWYFamework/Prefab/YooAssetModuleSetting.asset";
            if (File.Exists(path))
            {
                RSJWYLogger.LogError(RSJWYFameworkEnum.EditorTool,$"路径：{path} 已存在");
                return;
            }
            //创建数据资源文件
            //泛型是继承自ScriptableObject的类
            YooAssetModuleSetting asset = ScriptableObject.CreateInstance<YooAssetModuleSetting>();
            //前一步创建的资源只是存在内存中，现在要把它保存到本地
            //通过编辑器API，创建一个数据资源文件，第二个参数为资源文件在Assets目录下的路径
            AssetDatabase.CreateAsset(asset, "Assets/RSJWYFamework/Prefab/YooAssetModuleSetting.asset");
            //保存创建的资源
            AssetDatabase.SaveAssets();
            //刷新界面
            AssetDatabase.Refresh();
            RSJWYLogger.Log(RSJWYFameworkEnum.EditorTool,$"创建成功！！路径：{path}");
        }
    }
}