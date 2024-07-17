using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Logger;

namespace RSJWYFamework.Runtime.AsyncOperation
{
    public abstract class RAsyncOperationBase : IUniTaskSource, IPlayerLoopItem, IComparable<RAsyncOperationBase>
    {
        /// <summary>
        /// 管理异步操作的状态的核心
        /// </summary>
        UniTaskCompletionSourceCore<object> core;

        /// <summary>
        /// 异步操作名称
        /// </summary>
        internal string _AsyncOperationName = null;

        /// <summary>
        /// 循环检测次数限制
        /// </summary>
        private int _whileFrame = 1000;

        /// <summary>
        /// 是否已经完成
        /// </summary>
        internal bool IsFinish = false;

        /// <summary>
        /// 优先级
        /// </summary>
        public uint Priority { set; get; } = 0;

        /// <summary>
        /// 状态
        /// </summary>
        public RAsyncOperationStatus Status { get; protected set; } = RAsyncOperationStatus.None;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; protected set; }

        /// <summary>
        /// 处理进度
        /// </summary>
        public float Progress { get; protected set; }

        /// <summary>
        /// 是否已经完成
        /// </summary>
        public bool IsDone
        {
            get { return Status == RAsyncOperationStatus.Failed || Status == RAsyncOperationStatus.Succeed; }
        }
        
        

        /// <summary>
        /// 内部启动
        /// </summary>
        internal abstract void InternalOnStart();

        /// <summary>
        /// 内部刷新
        /// </summary>
        internal abstract void InternalOnUpdate();

        /// <summary>
        /// 内部秒刷新
        /// </summary>
        internal abstract void InternalOnUpdatePerSecond(float time);

        /// <summary>
        /// 内部中止
        /// </summary>
        internal virtual void InternalOnAbort()
        {
        }

        /// <summary>
        /// 内部等待完成异步
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        internal virtual void InternalWaitForAsyncComplete()
        {
            throw new System.NotImplementedException(this.GetType().Name);
        }

        internal void SetStart()
        {
            Status = RAsyncOperationStatus.Processing;
            InternalOnStart();
        }

        internal void SetFinish()
        {
            IsFinish = true;

            // 进度百分百完成
            Progress = 1f;

            //注意：如果完成回调内发生异常，会导致Task无限期等待
            
            core.TrySetResult(null);
        }

        internal void SetAbort()
        {
            if (IsDone == false)
            {
                Status = RAsyncOperationStatus.Failed;
                Error = "user abort";
                RSJWYLogger.LogWarning($"Async operaiton {this.GetType().Name} has been abort !");
                InternalOnAbort();
            }
        }

        /// <summary>
        /// 执行While循环
        /// </summary>
        protected bool ExecuteWhileDone()
        {
            if (IsDone == false)
            {
                // 执行更新逻辑
                InternalOnUpdate();

                // 当执行次数用完时
                _whileFrame--;
                if (_whileFrame == 0)
                {
                    Status = RAsyncOperationStatus.Failed;
                    Error = $"Operation {this.GetType().Name} failed to wait for async complete !";
                    RSJWYLogger.LogError(Error);
                }
            }

            return IsDone;
        }

        /// <summary>
        /// 等待异步执行完毕
        /// </summary>
        public void WaitForAsyncComplete()
        {
            if (IsDone)
                return;

            InternalWaitForAsyncComplete();
        }

        #region 排序接口实现

        public int CompareTo(RAsyncOperationBase other)
        {
            return other.Priority.CompareTo(this.Priority);
        }

        #endregion

        #region 异步编程相关

        public UniTaskStatus GetStatus(short token)
        {
            return core.GetStatus(token);
        }

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
            return !IsDone;
        }

        #endregion
    }
}