using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Procedure;

namespace RSJWYFamework.Runtime.HybridCLR.Procedure
{
    /// <summary>
    /// 加载热更代码流程结束
    /// </summary>
    public class LoadHotCodeDone:ProcedureBase
    {
        public override void OnInit()
        {
            
        }

        public override void OnClose()
        {
        }

        public override void OnEnter(ProcedureBase lastProcedureBase)
        {
            RSJWYLogger.Log($"热更流程结束");
        }

        public override void OnLeave(ProcedureBase nextProcedureBase)
        {
        }

        public override void OnUpdate()
        {
        }

        public override void OnUpdateSecond()
        {
        }
    }
}