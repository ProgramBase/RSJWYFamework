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
        /// 流程切换时的回调
        /// </summary>
        /// <param name="last"></param>
        /// <param name="next"></param>
        void ProcedureSwitchEven(IProcedure last, IProcedure next);

        /// <summary>
        /// 初始化加载流程
        /// </summary>
        void InitProcedure();
    }
}