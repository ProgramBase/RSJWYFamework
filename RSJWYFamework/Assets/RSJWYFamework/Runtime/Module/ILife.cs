namespace RSJWYFamework.Runtime.Module
{
    /// <summary>
    /// 生命周期接口
    /// </summary>
    public interface ILife
    {
        void OnUpdate(float time, float realtime);

        //void OnClose();
    }
}