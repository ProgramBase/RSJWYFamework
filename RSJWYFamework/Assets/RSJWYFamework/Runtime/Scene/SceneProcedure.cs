using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Procedure;

namespace RSJWYFamework.Runtime.Scene
{
    public abstract class LastClearrProcedure:ProcedureBase
    {
        /// <summary>
        /// 清理上一个场景相关资源
        /// </summary>
        public abstract UniTask Clear();
    }
    public abstract class PreLoadProcedure:ProcedureBase
    {
        /// <summary>
        /// 清理上一个场景相关资源
        /// </summary>
        public abstract UniTask PreLoad();
    }
    public abstract class LoadNextSceneProcedure:ProcedureBase
    {
        /// <summary>
        /// 清理上一个场景相关资源
        /// </summary>
        public abstract UniTask LoadNextScene();
    }
}