using System.Collections.Generic;
using System.Reflection;
using RSJWYFamework.Runtime.HybridCLR;
using RSJWYFamework.Runtime.HybridCLR.Procedure;
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
        /// 补充元DLL列表
        /// </summary>
        private static readonly List<string> AOTMetaAssemblyFiles = new();
        /// <summary>
        /// 热更新程序集加载列表
        /// </summary>
        private static readonly List<string> HotDllFiles = new();
        
        /// <summary>
        /// 加载到的程序集
        /// </summary>
        private static readonly Dictionary<string, Assembly> HotCode = new();
        public void Init()
        {
            pc = new DefaultProcedureController(this);
            pc.ProcedureSwitchEvent += ProcedureSwitchEven;
        } 
        public void InitProcedure()
        {
            pc.AddProcedure(new GetDLLListProcedure());
            pc.AddProcedure(new LoadDLLByteProcedure());
            pc.AddProcedure(new LoadHotCodeProcedure());
            pc.AddProcedure(new LoadHotCodeDone());
        }
        public void ProcedureSwitchEven(IProcedure last, IProcedure next)
        {
            
        }

       

        public void Close()
        {
            
        }
    }
}