using System;
using Cysharp.Threading.Tasks;
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
        /// <param name="lastProcedureBase"></param>
        /// <param name="nextProcedureBase"></param>
        void ProcedureSwitchEven(ProcedureBase lastProcedureBase, ProcedureBase nextProcedureBase);
        /// <summary>
        /// /初始化包
        /// </summary>
        UniTask LoadPackage();
        /// <summary>
        /// 加载完成事件
        /// </summary>
        event Action InitOverEvent;

        public void Start();
    }
}