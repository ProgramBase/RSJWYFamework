using System.IO;
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
}