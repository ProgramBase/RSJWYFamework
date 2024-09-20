namespace RSJWYFamework.Runtime.ReferencePool
{
    /// <summary>
    /// 引用接口
    /// </summary>
    public interface IReference
    {
        /// <summary>
        /// 重置引用-回收时
        /// </summary>
        void Release();
    }
}