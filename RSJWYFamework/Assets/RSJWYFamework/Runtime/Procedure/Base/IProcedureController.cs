using System;
using System.Collections.Generic;

namespace RSJWYFamework.Runtime.Procedure.Base
{
    /// <summary>
    /// 流程管理器接口
    /// </summary>
    public interface IProcedureController
    {
        /// <summary>
        /// 切换流程
        /// </summary>
        /// <param name="type">目标流程</param>
        public void SwitchProcedure(Type type);
        /// <summary>
        /// 切换至下一流程
        /// </summary>
        void SwitchNextProcedure();

        /// <summary>
        /// 添加流程
        /// </summary>
        /// <param name="procedure"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void AddProcedure(ProcedureBase procedure);
        /// <summary>
        /// 移除流程
        /// </summary>
        /// <param name="procedure"></param>
        /// <returns></returns>
        public void RemoveProcedure(Type type);

        /// <summary>
        /// 是否存在某一个流程
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsExistProcedur(Type type);


    }
}