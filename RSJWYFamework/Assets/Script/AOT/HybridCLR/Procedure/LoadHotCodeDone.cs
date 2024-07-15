using RSJWYFamework.Runtime.Procedure;

namespace Script.AOT.HybridCLR.Procedure
{
    /// <summary>
    /// 加载热更代码流程结束
    /// </summary>
    public class LoadHotCodeDone:IProcedure
    {
        public IProcedureController pc { get; set; }
        public void OnInit()
        {
            
        }

        public void OnClose()
        {
        }

        public void OnEnter(IProcedure lastProcedure)
        {
        }

        public void OnLeave(IProcedure nextProcedure)
        {
        }

        public void OnUpdate()
        {
        }

        public void OnUpdateSecond()
        {
        }
    }
}