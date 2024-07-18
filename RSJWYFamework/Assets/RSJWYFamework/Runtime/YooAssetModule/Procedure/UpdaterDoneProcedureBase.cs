using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Procedure;

namespace RSJWYFamework.Runtime.YooAssetModule.Procedure
{
    /// <summary>
    /// 完成更新流程
    /// </summary>
    public class UpdaterDoneProcedureBase:ProcedureBase
    {
        public override void OnInit()
        {
           
        }

        public override void OnClose()
        {
            
        }

        public override void OnEnter(ProcedureBase lastProcedureBase)
        {
            var packageName = (string)pc.GetBlackboardValue("PackageName");
            RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"完成包{packageName}更新流程");
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