namespace RSJWYFamework.Runtime.Module
{
    /// <summary>
    /// 生命周期接口
    /// 当需要main通知生命周期，但不想接入模块上，实现此，并添加到main里
    /// </summary>
    public interface ILife
    {
        /// <summary>
        /// 每帧更新
        /// 勿在此执行高耗时应用
        /// </summary>
        /// <param name="time">自启动后已经经过的时间</param>
        /// <param name="deltaTime">自上一帧完成以来经过的时间量</param>
        void GameUpdate(float time,float deltaTime);

        /// <summary>
        /// 每秒更新时间
        /// </summary>
        /// <param name="time">自启动后已经经过的时间</param>
        void UpdatePerSecond(float time);

        /// <summary>
        /// 每0.02s调用
        /// </summary>
        void FixedUpdate();

        /// <summary>
        /// 每帧结束后调用
        /// </summary>
        void LateUpdate();

        /// <summary>
        /// 优先级
        /// </summary>
        public uint Priority();
    }
}