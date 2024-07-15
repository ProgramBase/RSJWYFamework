using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace RSJWYFamework.Editor.Windows.Config
{
    /// <summary>
    /// 配置文件处
    /// </summary>
    public class SettingConfigWindows:OdinMenuEditorWindow
    {
        /// <summary>
        /// 加载侧边列表
        /// </summary>
        /// <returns></returns>
        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            //这里的第一个参数为窗口名字，第二个参数为指定目录，第三个参数为需要什么类型，第四个参数为是否在家该文件夹下的子文件夹
            tree.AddAllAssetsAtPath("编辑器下配置设置", "Assets/RSJWYFamework/Editor/Setting", typeof(ScriptableObject), true);
            tree.AddAllAssetsAtPath("运行时配置", "Assets/Resources", typeof(ScriptableObject), true);
            return tree;
        }
    }
}