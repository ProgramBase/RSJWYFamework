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
    }
}