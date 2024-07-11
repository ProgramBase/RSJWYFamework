using RSJWYFamework.Runtime.Procedure;

namespace RSJWYFamework.Runtime.YooAssetModule
{
    /// <summary>
    /// yooasset管理器接口
    /// </summary>
    public interface IYooAssetManager
    {
        /// <summary>
        /// 流程化切换回调
        /// </summary>
        /// <param name="lastProcedure"></param>
        /// <param name="nextProcedure"></param>
        void ProcedureSwitchEven(IProcedure lastProcedure, IProcedure nextProcedure);
    }
}