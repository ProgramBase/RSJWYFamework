
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.StateMachine;

namespace RSJWYFamework.Runtime.HybridCLR.StateNode
{
    /// <summary>
    /// 加载热更代码流程结束
    /// </summary>
    public class LoadHotCodeDone:StateNodeBase
    {
        public override void OnInit()
        {
            
        }

        public override void OnClose()
        {
        }

        public override void OnEnter(StateNodeBase lastStateNodeBase)
        {
            RSJWYLogger.Log($"热更流程结束");
        }

        public override void OnLeave(StateNodeBase nextStateNodeBase)
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