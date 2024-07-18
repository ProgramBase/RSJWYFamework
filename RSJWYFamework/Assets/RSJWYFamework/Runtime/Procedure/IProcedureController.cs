/*using System;
using RSJWYFamework.Runtime.Module;

namespace RSJWYFamework.Runtime.Procedure
{
    /// <summary>
    /// 流程管理器接口
    /// </summary>
    public interface IProcedureController
    {
        event Action<IProcedure, IProcedure> ProcedureSwitchEvent;
        
        /// <summary>
        /// 切换流程
        /// </summary>
        /// <param name="type">目标流程</param>
        void SwitchProcedure(Type type);
        /// <summary>
        /// 切换至下一流程
        /// </summary>
        void SwitchNextProcedure();

        /// <summary>
        /// 启动流程
        /// </summary>
        /// <param name="type"></param>
        void StartProcedure(Type type);

        /// <summary>
        /// 添加流程
        /// </summary>
        /// <param name="procedure"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void AddProcedure(IProcedure procedure);
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

        /// <summary>
        /// 初始化
        /// </summary>
        void Init();
        /// <summary>
        /// 关闭
        /// </summary>
        void Close();

        /// <summary>
        /// 设置黑板数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetBlackboardValue(string key, System.Object value);

        /// <summary>
        /// 获取黑板数据
        /// </summary>
        public System.Object GetBlackboardValue(string key);
        
        /// <summary>
        /// 清空黑板数据
        /// </summary>
        public void ClearBlackboard();

        /// <summary>
        /// 获取当前的流程
        /// </summary>
        /// <returns></returns>
        public Type GetNowProcedure();

    }
}*/