using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Settings;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.YooAssetModule;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RSJWYFamework.Editor.UtilityEditor
{
    public static partial class UtilityEditor
    {
        /// <summary>
        /// 获取项目根路径
        /// </summary>
        /// <returns></returns>
        public static string GetProjectPath()
        {
            return  Application.dataPath.Substring(0, Application.dataPath.Length - 6); // 去除"Assets"
        }
        /// <summary>
        /// 创建ScriptableObject
        /// </summary>
        /// <param name="path">创建路径</param>
        /// <typeparam name="TScriptableObject">类型，必须继承自ScriptableObject</typeparam>
        public static void CreateScriptableObject<TScriptableObject>(string path)where TScriptableObject:ScriptableObject
        {
            if (File.Exists(path))
            {
                RSJWYLogger.Error(RSJWYFameworkEnum.EditorTool,$"路径：{path} 已存在");
                return;
            }
            //创建数据资源文件
            //泛型是继承自ScriptableObject的类
            var asset = ScriptableObject.CreateInstance<TScriptableObject>();
            //前一步创建的资源只是存在内存中，现在要把它保存到本地
            //通过编辑器API，创建一个数据资源文件，第二个参数为资源文件在Assets目录下的路径
            AssetDatabase.CreateAsset(asset, path);
            //保存创建的资源
            AssetDatabase.SaveAssets();
            //刷新界面
            AssetDatabase.Refresh();
            RSJWYLogger.Log(RSJWYFameworkEnum.EditorTool,$"创建成功！！路径：{path}");
        }
        
        /// <summary>
        /// 保存场景
        /// </summary>
        public static bool AutoSaveScence()
        {
            // if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            // {
            //     Debug.Log("Scene not saved");
            // }
            // else
            // {
            //     Debug.Log("Scene saved");
            // }
            RSJWYLogger.Log("保存场景");
            bool issave = EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            return issave;
        }
       
    }
}