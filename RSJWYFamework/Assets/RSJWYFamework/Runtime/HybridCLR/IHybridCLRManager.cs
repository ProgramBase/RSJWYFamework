using System;
using Cysharp.Threading.Tasks;
using MyFamework.Runtime.Base;
using RSJWYFamework.Runtime.Procedure;

namespace RSJWYFamework.Runtime.HybridCLR
{
    /// <summary>
    /// 热更模块接口
    /// </summary>
    public interface IHybridCLRManager:ModleInterface
    {

        /// <summary>
        /// 初始化加载流程
        /// </summary>
        UniTask LoadHotCodeDLL();

    }
}