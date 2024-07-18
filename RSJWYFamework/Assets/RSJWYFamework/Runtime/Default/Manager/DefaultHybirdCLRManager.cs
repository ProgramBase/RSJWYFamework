using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.AsyncOperation;
using RSJWYFamework.Runtime.HybridCLR;
using RSJWYFamework.Runtime.HybridCLR.AsyncOperation;
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
            RAsyncOperationSystem.StartOperation(string.Empty,op);
            await op.UniTask;
            HotCode = op.HotCode;
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