using RSJWYFamework.Runtime.Procedure;

namespace RSJWYFamework.Runtime.HybridCLR.Procedure
{
    /// <summary>
    /// 加载DLL流程
    /// </summary>
    public class LoadHotCodeProcedure:IProcedure
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