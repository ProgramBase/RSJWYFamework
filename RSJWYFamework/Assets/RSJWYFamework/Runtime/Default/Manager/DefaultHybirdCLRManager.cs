using System;
using System.Collections.Generic;
using System.Reflection;
using RSJWYFamework.Runtime.HybridCLR;
using RSJWYFamework.Runtime.HybridCLR.Procedure;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Module;
using RSJWYFamework.Runtime.Procedure;

namespace RSJWYFamework.Runtime.Default.Manager
{
    /// <summary>
    /// 热更管理器
    /// </summary>
    public class DefaultHybirdCLRManager:IHybridCLRManager,IModule
    {
        private IProcedureController pc;
        /// <summary>
        /// 加载到的程序集
        /// </summary>
        private static Dictionary<string, Assembly> HotCode = new();
        
        public event Action LoadHotCodeOver;
        public void Init()
        {
            pc = new DefaultProcedureController(this);
            pc.ProcedureSwitchEvent += ProcedureSwitchEven;
        } 
        public void InitProcedure()
        {
            //创建流程
            pc.AddProcedure(new LoadDLLByteProcedure());
            pc.AddProcedure(new LoadHotCodeProcedure());
            pc.AddProcedure(new LoadHotCodeDone());
            pc.StartProcedure(typeof(LoadDLLByteProcedure));
        }


        public void ProcedureSwitchEven(IProcedure last, IProcedure next)
        {
            if (last is null&&next is null)
                return;
            if (next is LoadHotCodeDone)
            {
                HotCode = (Dictionary<string, Assembly>)pc.GetBlackboardValue("HotCodeAssembly");
                LoadHotCodeOver?.Invoke();
                RSJWYLogger.Log("热更流程执行完毕");
            }
        }

       

        public void Close()
        {
            
        }

        public void Update(float time, float deltaTime)
        {
            
        }

        public void UpdatePerSecond(float time)
        {
            
        }
    }
}