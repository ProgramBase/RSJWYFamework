using RSJWYFamework.Runtime.AsyncOperation;
using RSJWYFamework.Runtime.Procedure;

namespace RSJWYFamework.Runtime.Scene
{
    /// <summary>
    /// 切换场景流程，请不要进行等待，并且确保初始化后没有后文
    /// 有参构造函数会在初始化时进行调用启动函数，并开始完整流程
    /// </summary>
    public class SwitchOperation:GameRAsyncOperation,IProcedureUser
    {
        enum RSteps
        {
            None,
            /// <summary>
            /// 切换场景到中转场景
            /// </summary>
            SwitchToTransfer,
            /// <summary>
            /// 清理上一个场景相关资源
            /// </summary>
            LastClear,
            /// <summary>
            /// 预加载加载下一个场景资源
            /// </summary>
            PreLoad,
            /// <summary>
            /// 加载并前往下一个场景
            /// </summary>
            LoadNextScene,
            Done
        }
        private readonly ProcedureController pc;
        private RSteps _steps = RSteps.None;

        public SwitchOperation(LastClearrProcedure lastClearrProcedure, PreLoadProcedure preLoadProcedure, LoadNextSceneProcedure loadNextSceneProcedure)
        {
            pc = new ProcedureController(this);
            
            
            
            Main.Main.RAsyncOperationSystem.StartOperation(typeof(SwitchOperation).FullName, this);
        }
        
        protected override void OnStart()
        {
            
        }

        protected override void OnUpdate(float time, float deltaTime)
        {
        }

        protected override void OnUpdatePerSecond(float time)
        {
        }

        protected override void OnAbort()
        {
        }

        public void Exception(ProcedureException exception)
        {
        }
    }
}