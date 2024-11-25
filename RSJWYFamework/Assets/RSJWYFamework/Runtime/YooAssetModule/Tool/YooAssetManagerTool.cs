using System;
using System.IO;
using System.Text;
using RSJWYFamework.Runtime.Config;
using RSJWYFamework.Runtime.Logger;
using UnityEngine;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule.Tool
{
    public static class YooAssetManagerTool
    {
        static string ProjectName = "测试工程";
        private static string AppName = "测试软件";
        static string HostServerIP = "http://127.0.0.1";
        static string AppVersion = "v1.0";

        public static void Setting(
            string hostServerIP = "测试工程", string projectName = "测试软件",
            string appName = "http://127.0.0.1", string appVersion = "v1.0")
        {
            ProjectName = projectName;
            AppName = appName;
            HostServerIP = hostServerIP;
            AppVersion = appVersion;
        }
        
        /// <summary>
        /// 获取资源服务器地址
        /// </summary>
        /// <param name="packageName">包名</param>
        /// <returns></returns>
        public static string GetHostServerURL(string packageName)
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
                return $"{HostServerIP}/{ProjectName}/{AppName}/{AppVersion}/Android/{packageName}";
            else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
                return $"{HostServerIP}/{ProjectName}/{AppName}/{AppVersion}/IPhone/{packageName}";
            else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
                return $"{HostServerIP}/{ProjectName}/{AppName}/{AppVersion}/WebGL/{packageName}";
            else
                return $"{HostServerIP}/{ProjectName}/{AppName}/{AppVersion}/PC/{packageName}";
#else
        if (Application.platform == RuntimePlatform.Android)
            return $"{HostServerIP}/{ProjectName}/{AppName}/{AppVersion}/Android/{packageName}";
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
            return $"{HostServerIP}/{ProjectName}/{AppName}/{AppVersion}/IPhone/{packageName}";
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
            return $"{HostServerIP}/{ProjectName}/{AppName}/{AppVersion}/WebGL/{packageName}";
        else
            return $"{HostServerIP}/{ProjectName}/{AppName}/{AppVersion}/PC/{packageName}";
#endif
        }

        /// <summary>
        /// 远端资源地址查询服务类
        /// </summary>
        public class RemoteServices : IRemoteServices
        {
            private readonly string _defaultHostServer;
            private readonly string _fallbackHostServer;

            public RemoteServices(string defaultHostServer, string fallbackHostServer)
            {
                _defaultHostServer = defaultHostServer;
                _fallbackHostServer = fallbackHostServer;
            }

            string IRemoteServices.GetRemoteMainURL(string fileName)
            {
                return $"{_defaultHostServer}/{fileName}";
            }

            string IRemoteServices.GetRemoteFallbackURL(string fileName)
            {
                return $"{_fallbackHostServer}/{fileName}";
            }
        }
        
        /// <summary>
        /// 资源文件流加载解密类
        /// </summary>
        public class FileDecryption : IDecryptionServices
        {
            /// <summary>
            /// 同步方式获取解密的资源包对象
            /// 注意：加载流对象在资源包对象释放的时候会自动释放
            /// </summary>
            AssetBundle IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
            {
                RSJWYLogger.Log($"解密文件：{fileInfo.BundleName}");
                managedStream = null;
                byte[] AESFileData = File.ReadAllBytes(fileInfo.FileLoadPath);
                byte[] fileData = Utility.Utility.AESTool.AESDecrypt(AESFileData, Main.Main.DataManagerataManager.GetDataSetSB<ProjectConfig>().AESKey);
                return AssetBundle.LoadFromMemory(fileData);
            }

            /// <summary>
            /// 异步方式获取解密的资源包对象
            /// 注意：加载流对象在资源包对象释放的时候会自动释放
            /// </summary>
            AssetBundleCreateRequest IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream)
            {
                RSJWYLogger.Log($"解密文件：{fileInfo.BundleName}");
                managedStream = null;
                byte[] AESFileData = File.ReadAllBytes(fileInfo.FileLoadPath);
                byte[] fileData = Utility.Utility.AESTool.AESDecrypt(AESFileData, Main.Main.DataManagerataManager.GetDataSetSB<ProjectConfig>().AESKey);
                return AssetBundle.LoadFromMemoryAsync(fileData);
            }
            /// <summary>
            /// 获取加密过的Data
            /// </summary>
            public byte[] ReadFileData(DecryptFileInfo fileInfo)
            {
                RSJWYLogger.Log($"解密文件{fileInfo.BundleName}，路径：{fileInfo.FileLoadPath}");
                byte[] fileData = File.ReadAllBytes(fileInfo.FileLoadPath);
                return Utility.Utility.AESTool.AESDecrypt(fileData,Main.Main.DataManagerataManager.GetDataSetSB<ProjectConfig>().AESKey);
            }
            /// <summary>
            /// 获取加密过的Text
            /// </summary>
            public string ReadFileText(DecryptFileInfo fileInfo)
            {
                byte[] fileData = File.ReadAllBytes(fileInfo.FileLoadPath);
                var DData= Utility.Utility.AESTool.AESDecrypt(fileData,Main.Main.DataManagerataManager.GetDataSetSB<ProjectConfig>().AESKey);
                return Encoding.UTF8.GetString(DData);
            }
        }
    }
}