namespace RSJWYFamework.Runtime.Module
{
    /// <summary>
    /// 生命周期接口
    /// </summary>
    public interface ILife
    {
        /// <summary>
        /// 帧更新
        /// </summary>
        /// <param name="time"></param>
        /// <param name="realtime"></param>
        void OnUpdate(float time, float realtime);


        //void OnClose();
    }
}