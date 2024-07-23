
using YooAsset;

namespace RSJWYFamework.Runtime.AsyncOperation
{
    /// <summary>
    /// 游戏的异步操作
    /// 允许非本程序集访问
    /// </summary>
    public abstract class GameRAsyncOperation : RAsyncOperationBase
    {
        internal override void InternalOnStart()
        {
            OnStart();
        }
        internal override void InternalOnUpdate(float time,float deltaTime)
        {
            OnUpdate( time, deltaTime);
        }
        internal override void InternalOnAbort()
        {
            OnAbort();
        }

        /// <summary>
        /// 异步操作开始
        /// </summary>
        protected abstract void OnStart();

        /// <summary>
        /// 异步操作更新
        /// </summary>
        protected abstract void OnUpdate(float time,float deltaTime);

        /// <summary>
        /// 异步操作终止
        /// </summary>
        protected abstract void OnAbort();
        
    }
}