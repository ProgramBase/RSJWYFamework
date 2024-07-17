using System;
using Cysharp.Threading.Tasks;

namespace RSJWYFamework.Runtime.AsyncOperation
{

    /// <summary>
    /// 异步操作
    /// </summary>
    public class AsyncOperation : IUniTaskSource,IPlayerLoopItem,IComparable<AsyncOperation>
    {
        /// <summary>
        /// 管理异步操作的状态的核心
        /// </summary>
        UniTaskCompletionSourceCore<AsyncUnit> core;
        /// <summary>
        /// 获得异步状态
        /// </summary>
        public UniTaskStatus GetStatus(short token)
        {
            return core.GetStatus(token);
        }
        /// <summary>
        /// 异步任务完成时
        /// </summary>
        public void OnCompleted(Action<object> continuation, object state, short token)
        {
            core.OnCompleted(continuation, state, token);
        }

        /// <summary>
        /// 获取结果
        /// </summary>
        public void GetResult(short token)=>core.GetStatus(token);
        /// <summary>
        /// 不安全的获取
        /// </summary>
        public UniTaskStatus UnsafeGetStatus()=>core.UnsafeGetStatus();

        /// <summary>
        /// 执行异步逻辑
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 拍
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public int CompareTo(AsyncOperation other)
        {
            throw new NotImplementedException();
        }
    }
}