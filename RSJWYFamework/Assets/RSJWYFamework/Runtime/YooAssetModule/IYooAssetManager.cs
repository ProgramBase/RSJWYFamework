using System;
using MyFamework.Runtime.Base;
using RSJWYFamework.Runtime.Procedure;

namespace RSJWYFamework.Runtime.YooAssetModule
{
    /// <summary>
    /// yooasset管理器接口
    /// </summary>
    public interface IYooAssetManager:ModleInterface
    {
        /// <summary>
        /// 流程化切换回调
        /// </summary>
        /// <param name="lastProcedure"></param>
        /// <param name="nextProcedure"></param>
        void ProcedureSwitchEven(IProcedure lastProcedure, IProcedure nextProcedure);
        /// <summary>
        /// /初始化包
        /// </summary>
        void InitPackage();
        /// <summary>
        /// 加载完成事件
        /// </summary>
        event Action InitOverEvent;
    }
}