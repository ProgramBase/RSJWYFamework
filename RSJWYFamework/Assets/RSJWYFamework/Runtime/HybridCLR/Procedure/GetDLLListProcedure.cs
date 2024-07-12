using RSJWYFamework.Runtime.Procedure;

namespace RSJWYFamework.Runtime.HybridCLR.Procedure
{
    /// <summary>
    /// 获取要加载的DLL列表
    /// </summary>
    public class GetDLLListProcedure:IProcedure
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
        {;
        }

        public void OnUpdate()
        {
        }

        public void OnUpdateSecond()
        {
        }
    }
}