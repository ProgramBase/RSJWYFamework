using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Procedure;

namespace Script.AOT.YooAssetModule.Procedure
{
    /// <summary>
    /// 完成更新流程
    /// </summary>
    public class UpdaterDoneProcedure:IProcedure
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
            var packageName = (string)pc.GetBlackboardValue("PackageName");
            RSJWYLogger.Log(RSJWYFameworkEnum.YooAssets,$"完成包{packageName}更新流程");
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