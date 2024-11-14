using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.HybridCLR.AsyncOperation;
using RSJWYFamework.Runtime.Module;

namespace RSJWYFamework.Runtime.Default.Manager
{
    /// <summary>
    /// 热更管理器
    /// </summary>
    public class HybirdCLRManager:IModule
    {
        /// <summary>
        /// 加载到的程序集
        /// </summary>
        private static Dictionary<string, Assembly> HotCode = new();
        
        public void Init()
        {
            
        } 
        public async UniTask LoadHotCodeDLL()
        {
            var op = new LoadHotCodeOperation();
            await op.UniTask;
            HotCode = op.HotCode;
        }

       

        public void Close()
        {
            
        }
    }
}