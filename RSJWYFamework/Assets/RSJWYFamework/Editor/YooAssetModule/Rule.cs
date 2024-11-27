using System.IO;
using UnityEditor;
using YooAsset.Editor;
namespace RSJWYFamework.Editor.YooAssetModule
{
    
    [DisplayName("收集热更代码")]
    public class HotCodeFilterRule:IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            string extension = Path.GetExtension(data.AssetPath);
            return extension == ".bytes";
        }
    }
    [DisplayName("打包为加密热更代码名")]
    public class HotCodePackRule : IPackRule
    {
        public PackRuleResult GetPackRuleResult(PackRuleData data)
        {
            //提取文件名，增加标记
            var filename=Path.GetFileName(data.AssetPath);
            return new PackRuleResult($"{filename}_HotCodeEncrypt", "hotcode");
        }
    }

    public class PrefabIgnoreRule : IIgnoreRule
    {
        /// <summary>
        /// 查询是否为忽略文件
        /// </summary>
        public bool IsIgnore(AssetInfo assetInfo)
        {
            if (assetInfo.AssetPath.StartsWith("Assets/") == false && assetInfo.AssetPath.StartsWith("Packages/") == false)
            {
                UnityEngine.Debug.LogError($"Invalid asset path : {assetInfo.AssetPath}");
                return true;
            }
            // 忽略Editor文件夹下的资源
            if (assetInfo.AssetPath.Contains("/Editor/")||assetInfo.AssetPath.Contains("Packages/"))
                return true;

            // 忽略文件夹
            if (AssetDatabase.IsValidFolder(assetInfo.AssetPath))
                return true;

            // 忽略编辑器下的类型资源
            if (assetInfo.AssetType == typeof(LightingDataAsset))
                return true;
            if (assetInfo.AssetType == typeof(LightmapParameters))
                return true;

            // 忽略Unity引擎无法识别的文件
            if (assetInfo.AssetType == typeof(UnityEditor.DefaultAsset))
            {
                UnityEngine.Debug.LogWarning($"Cannot pack default asset : {assetInfo.AssetPath}");
                return true;
            }

            return DefaultIgnoreRule.IgnoreFileExtensions.Contains(assetInfo.FileExtension);
        }
    }
}