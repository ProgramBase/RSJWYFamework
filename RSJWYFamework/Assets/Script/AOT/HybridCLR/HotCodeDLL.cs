using System.Collections.Generic;
using Newtonsoft.Json;

namespace Script.AOT.HybridCLR.Procedure
{
    /// <summary>
    /// 热更代码json加载
    /// </summary>
    public class HotCodeDLL
    {
        /// <summary>
        /// 补充元数据
        /// </summary>
        [JsonProperty("MetadataForAOTAssemblies")]
        public List<string> MetadataForAOTAssemblies;
        /// <summary>
        /// 热更代码
        /// </summary>
        [JsonProperty("HotCode")]
        public List<string> HotCode;
    }
}