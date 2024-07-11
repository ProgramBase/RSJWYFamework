using RSJWYFamework.Runtime.Procedure;
using UnityEngine;

namespace RSJWYFamework.Runtime.YooAssetModule.Procedure
{
    /// <summary>
    /// 
    /// </summary>
    public class InitPackageProcedure:IProcedure
    {
        public IProcedureController pc { get; set; }
        
        
        public void OnInit()
        {
        }

        public void OnClose()
        {
            throw new System.NotImplementedException();
        }

        public void OnEnter(IProcedure lastProcedure)
        {
            throw new System.NotImplementedException();
        }

        public void OnLeave(IProcedure nextProcedure)
        {
            throw new System.NotImplementedException();
        }

        public void OnUpdate()
        {
            throw new System.NotImplementedException();
        }

        public void OnUpdateSecond()
        {
            throw new System.NotImplementedException();
        }
    }
}