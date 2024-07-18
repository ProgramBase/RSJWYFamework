namespace RSJWYFamework.Runtime.Module
{
    public interface IModule
    {
        /// <summary>
        /// 添加时初始化
        /// </summary>
        void Init();
        /// <summary>
        /// 关闭
        /// </summary>
        void Close();
        
        /// <summary>
        /// 每帧更新
        /// 勿在此执行高耗时应用
        /// </summary>
        /// <param name="time">自启动后已经经过的时间</param>
        /// <param name="deltaTime">自上一帧完成以来经过的时间量</param>
        void Update(float time,float deltaTime);

        /// <summary>
        /// 每秒更新时间
        /// </summary>
        /// <param name="time">自启动后已经经过的时间</param>
        void UpdatePerSecond(float time);
    }
}