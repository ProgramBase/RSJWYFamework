using System;
using System.IO;
using System.Text;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Senseshield;
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
        
#if UNITY_EDITOR
        /// <summary>
        /// 加密资源包-原生资源
        /// </summary>
        public class EncryptRF : IEncryptionServices
        {
            public EncryptResult Encrypt(EncryptFileInfo fileInfo)
            {
                // 注意：针对特定规则加密
                if (fileInfo.BundleName.Contains("_assets_hotupdateassets_hotcode_"))
                {
                    RSJWYLogger.Log($"加密文件{fileInfo.BundleName}");
                    byte[] fileData = File.ReadAllBytes(fileInfo.FileLoadPath);

                    var edata = Utility.Utility.AESTool.AESEncrypt(fileData,"");
                    
                    EncryptResult result = new EncryptResult();
                    result.Encrypted = true;
                    result.EncryptedData = edata;
                    return result;
                }
                else
                {
                    EncryptResult result = new EncryptResult();
                    result.Encrypted = false;
                    return result;
                }
            }
        }
        /// <summary>
        /// 加密资源包-资源文件
        /// </summary>
        public class EncryptPF : IEncryptionServices
        {
            public EncryptResult Encrypt(EncryptFileInfo fileInfo)
            {
                // 注意：针对特定规则加密
                RSJWYLogger.Log($"加密文件{fileInfo.BundleName}");
                byte[] fileData = File.ReadAllBytes(fileInfo.FileLoadPath);

                var edata = Utility.Utility.AESTool.AESEncrypt(fileData,"");
                    
                EncryptResult result = new EncryptResult();
                result.Encrypted = true;
                result.EncryptedData = edata;
                return result;
            }
        }
#endif
        /// <summary>
        /// 资源文件流加载解密类
        /// </summary>
        public class FileStreamDecryption : IDecryptionServices
        {
            /// <summary>
            /// 同步方式获取解密的资源包对象
            /// 注意：加载流对象在资源包对象释放的时候会自动释放
            /// </summary>
            AssetBundle IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
            {
                BundleStream bundleStream = new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read,
                    FileShare.Read);
                managedStream = bundleStream;
                return AssetBundle.LoadFromStream(bundleStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            }

            /// <summary>
            /// 异步方式获取解密的资源包对象
            /// 注意：加载流对象在资源包对象释放的时候会自动释放
            /// </summary>
            AssetBundleCreateRequest IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo,
                out Stream managedStream)
            {
                BundleStream bundleStream = new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read,
                    FileShare.Read);
                managedStream = bundleStream;
                return AssetBundle.LoadFromStreamAsync(bundleStream, fileInfo.FileLoadCRC,
                    GetManagedReadBufferSize());
            }
            /// <summary>
            /// 获取加密过的Data
            /// </summary>
            /// <param name="fileInfo"></param>
            /// <returns></returns>
            public byte[] ReadFileData(DecryptFileInfo fileInfo)
            {
                RSJWYLogger.Log($"解密文件{fileInfo.BundleName}");
                byte[] fileData = File.ReadAllBytes(fileInfo.FileLoadPath);
                return Utility.Utility.AESTool.AESEncrypt(fileData,"");
            }
            /// <summary>
            /// 获取加密过的Text
            /// </summary>
            /// <param name="fileInfo"></param>
            /// <returns></returns>
            public string ReadFileText(DecryptFileInfo fileInfo)
            {
                byte[] fileData = File.ReadAllBytes(fileInfo.FileLoadPath);
                var DData= Utility.Utility.AESTool.AESEncrypt(fileData,"");
                return File.ReadAllText(fileInfo.FileLoadPath, Encoding.UTF8);
            }

            private static uint GetManagedReadBufferSize()
            {
                return 1024;
            }
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        /// <summary>
        /// 资源文件偏移加载解密类
        /// </summary>
        public class FileOffsetDecryption : IDecryptionServices
        {
            /// <summary>
            /// 同步方式获取解密的资源包对象
            /// 注意：加载流对象在资源包对象释放的时候会自动释放
            /// </summary>
            AssetBundle IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
            {
                managedStream = null;
                return AssetBundle.LoadFromFile(fileInfo.FileLoadPath, fileInfo.FileLoadCRC, GetFileOffset());
            }

            /// <summary>
            /// 异步方式获取解密的资源包对象
            /// 注意：加载流对象在资源包对象释放的时候会自动释放
            /// </summary>
            AssetBundleCreateRequest IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo,
                out Stream managedStream)
            {
                managedStream = null;
                return AssetBundle.LoadFromFileAsync(fileInfo.FileLoadPath, fileInfo.FileLoadCRC, GetFileOffset());
            }

            public byte[] ReadFileData(DecryptFileInfo fileInfo)
            {
                throw new System.NotImplementedException();
            }

            public string ReadFileText(DecryptFileInfo fileInfo)
            {
                throw new System.NotImplementedException();
            }

            private static ulong GetFileOffset()
            {
                return 32;
            }
        }

        /// <summary>
        /// 资源文件解密流
        /// </summary>
        public class BundleStream : FileStream
        {
            public const byte KEY = 64;

            public BundleStream(string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode,
                access, share)
            {
            }

            public BundleStream(string path, FileMode mode) : base(path, mode)
            {
            }

            public override int Read(byte[] array, int offset, int count)
            {
                var index = base.Read(array, offset, count);
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] ^= KEY;
                }

                return index;
            }
        }
    }
}