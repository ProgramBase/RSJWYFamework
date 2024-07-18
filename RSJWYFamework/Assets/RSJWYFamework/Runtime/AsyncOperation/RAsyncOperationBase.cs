using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Logger;

namespace RSJWYFamework.Runtime.AsyncOperation
{
    /// <summary>
    /// 异步系统基础
    /// </summary>
    public abstract class RAsyncOperationBase : IUniTaskSource, IPlayerLoopItem, IComparable<RAsyncOperationBase>
    {
       
        
        /// <summary>
        /// 异步操作名称
        /// </summary>
        internal string _AsyncOperationName = null;

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
        internal abstract void InternalOnUpdate(float time,float deltaTime);

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
            
            tcs.TrySetResult(this);
        }
        /// <summary>
        /// 设置发生异常
        /// </summary>
        internal void SetException(Exception exception)
        {
            //注意：如果完成回调内发生异常，会导致Task无限期等待
            Status = RAsyncOperationStatus.Failed;
            tcs.TrySetException(exception);
        }

        internal void SetAbort()
        {
            if (IsDone == false)
            {
                Status = RAsyncOperationStatus.Failed;
                Error = "user abort";
                RSJWYLogger.Warning($"Async operaiton {this.GetType().Name} has been abort !");
                InternalOnAbort();
            }
        }
        
        
        #region 排序接口实现

        public int CompareTo(RAsyncOperationBase other)
        {
            return other.Priority.CompareTo(this.Priority);
        }

        #endregion

        #region 异步编程相关

        /// <summary>
        /// 管理异步操作的状态的核心
        /// </summary>
        UniTaskCompletionSource<RAsyncOperationBase> tcs;
        public UniTaskStatus GetStatus(short token)
        {
            return tcs.GetStatus(token);
        }

        public void OnCompleted(Action<object> continuation, object state, short token)
        {
            tcs.OnCompleted(continuation, state, token);
        }

        /// <summary>
        /// 获取结果
        /// </summary>
        public void GetResult(short token)=>tcs.GetStatus(token);

        /// <summary>
        /// 不安全的获取
        /// </summary>
        public UniTaskStatus UnsafeGetStatus()=>tcs.UnsafeGetStatus();
        /// <summary>
        /// 执行异步逻辑
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool MoveNext()
        {
            return !IsDone;
        }
        
        
        /// <summary>
        /// 异步操作任务
        /// </summary>
        public UniTask UniTask
        {
            get
            {
                if (tcs == null)
                {
                    tcs = new ();
                    if (IsDone)
                        tcs.TrySetResult(this);
                }
                return tcs.Task;
            }
        }

        #endregion
    }
}