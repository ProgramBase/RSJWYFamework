
using System.Collections.Generic;
using System.Reflection;
using RSJWYFamework.Runtime.AsyncOperation;
using RSJWYFamework.Runtime.AsyncOperation.Procedure;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.HybridCLR.Procedure;

namespace RSJWYFamework.Runtime.HybridCLR.AsyncOperation
{
    /// <summary>
    /// 加载热更代码
    /// </summary>
    public class LoadHotCodeOperation:GameRAsyncOperation,IProcedureUser
    {
        enum RSteps
        {
            None,
            Update,
            Done
        }
        /// <summary>
        /// 加载到的程序集
        /// </summary>
        public Dictionary<string, Assembly> HotCode { get; private set; } = new();
        private readonly ProcedureController pc;
        private RSteps _steps = RSteps.None;
        
        public LoadHotCodeOperation()
        {
            pc = new ProcedureController(this);
            //创建流程
            pc.AddProcedure(new LoadDLLByteProcedureBase());
            pc.AddProcedure(new LoadHotCodeProcedureBase());
            pc.AddProcedure(new LoadHotCodeDone());
        }

        protected override void OnStart()
        {
            _steps = RSteps.Update;
            pc.StartProcedure(typeof(LoadDLLByteProcedureBase));
        }

        protected override void OnUpdate(float time, float deltaTime)
        {
            if (_steps == RSteps.None || _steps == RSteps.Done)
                return;

            if(_steps == RSteps.Update)
            {
                pc.OnUpdate(time,deltaTime);
                if(pc.GetNowProcedure() == typeof(LoadHotCodeDone))
                {
                    Status = RAsyncOperationStatus.Succeed;
                    _steps = RSteps.Done;
                }
            }
        }

        protected override void OnUpdatePerSecond(float time)
        {
            
        }

        protected override void OnAbort()
        {
            
        }

        public void Exception(ProcedureException exception)
        {
            SetException(exception);
        }
    }
}