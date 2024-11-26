using System.IO;
using RSJWYFamework.Runtime.Config;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Utility;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

namespace RSJWYFamework.Editor.YooAssetModule
{
    public class EncryptAssets:IEncryptionServices
    {
        private string aeskey;
        public EncryptAssets()
        {
            aeskey =  Resources.Load<ProjectConfig>("ProjectConfig").AESKey;
        }
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            // 注意：针对特定规则加密
            var extension = Path.GetExtension(fileInfo.FileLoadPath);
            switch (extension)
            {
                case ".hotcode":
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
                case ".bundle":
                {
                    RSJWYLogger.Log($"加密bundle文件{fileInfo.BundleName}");
                    byte[] fileData = File.ReadAllBytes(fileInfo.FileLoadPath);
                    var edata = Utility.AESTool.AESEncrypt(fileData,aeskey);
                    return new EncryptResult
                    {
                        Encrypted = true,
                        EncryptedData = edata
                    };
                }
                default:
                    RSJWYLogger.Log($"不满足和加密后缀要求，不进行加密：{fileInfo.BundleName}");
                    return new EncryptResult
                    {
                        Encrypted = false,
                    };
            }
        }
    }
}