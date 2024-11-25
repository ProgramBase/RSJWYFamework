using System.IO;
using RSJWYFamework.Runtime.Config;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Utility;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

namespace RSJWYFamework.Editor.YooAssetModule
{
    public class EncryptAssets
    {
         /// <summary>
        /// 加密资源包-原生资源
        /// </summary>
        public class EncryptRawFile : IEncryptionServices
        {
            private string aeskey;
            public EncryptRawFile() : base()
            {
                aeskey =  Resources.Load<ProjectConfig>("ProjectConfig").AESKey;
            }
            public EncryptResult Encrypt(EncryptFileInfo fileInfo)
            {
                // 注意：针对特定规则加密
                var extension = Path.GetExtension(fileInfo.FileLoadPath);
                if (extension==".hotcode")
                {
                    RSJWYLogger.Log($"加密文件{fileInfo.BundleName}");
                    byte[] fileData = File.ReadAllBytes(fileInfo.FileLoadPath);
                    var edata = Utility.AESTool.AESEncrypt(fileData,aeskey);
                    return new EncryptResult
                    {
                        Encrypted = true,
                        EncryptedData = edata
                    };
                }
                else
                {
                    return new EncryptResult
                    {
                        Encrypted = false,
                    };
                }
                
            }
           
        }
        /// <summary>
        /// 加密资源包-资源文件
        /// </summary>
        public class EncryptPrefabFile : IEncryptionServices
        {
            private string aeskey;
            public EncryptPrefabFile() : base()
            {
                aeskey =  Resources.Load<ProjectConfig>("ProjectConfig").AESKey;
            }

            public EncryptResult Encrypt(EncryptFileInfo fileInfo)
            {
                RSJWYLogger.Log($"加密文件{fileInfo.BundleName}，路径：{fileInfo.FileLoadPath}");
                byte[] fileData = File.ReadAllBytes(fileInfo.FileLoadPath);
                var edata = Utility.AESTool.AESEncrypt(fileData,aeskey);
                EncryptResult result = new EncryptResult
                {
                    Encrypted = true,
                    EncryptedData = edata
                };
                return result;
            }
        }
    }
}