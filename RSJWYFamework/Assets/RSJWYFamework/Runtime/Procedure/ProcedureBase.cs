namespace RSJWYFamework.Runtime.Procedure
{
    /// <summary>
    /// 流程实现基类
    /// </summary>
    public abstract class ProcedureBase
    {
        /// <summary>
        /// 关联到流程控制器
        /// </summary>
        public ProcedureController pc { get;internal set; }

        /// <summary>
        /// 流程初始化
        /// 添加流程时调用
        /// </summary>
        public abstract void OnInit();
        
        /// <summary>
        /// 流程关闭
        /// 移除流程时调用
        /// </summary>
        public abstract  void OnClose();
        
        /// <summary>
        /// 进入当前流程
        /// </summary>
        /// <param name="lastProcedureBase">上一个离开的流程</param>
        public abstract void OnEnter(ProcedureBase lastProcedureBase);

        /// <summary>
        /// 离开当前流程
        /// </summary>
        /// <param name="nextProcedureBase">下一个进入的流程</param>
        public abstract void OnLeave(ProcedureBase nextProcedureBase);

        /// <summary>
        /// 流程帧更新
        /// </summary>
        public abstract void OnUpdate();

        /// <summary>
        /// 流程秒更新
        /// </summary>
        public abstract void OnUpdateSecond();
    }
}