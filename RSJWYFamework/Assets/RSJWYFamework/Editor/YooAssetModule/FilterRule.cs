using System.IO;
using YooAsset.Editor;
namespace RSJWYFamework.Editor.YooAssetModule
{
    
    [DisplayName("收集热更代码")]
    public class HotCode:IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            string extension = Path.GetExtension(data.AssetPath);
            return extension == ".bytes";
        }
    }
}