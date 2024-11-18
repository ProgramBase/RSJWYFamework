using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Procedure;

namespace RSJWYFamework.Runtime.Scene
{
    /// <summary>
    /// 不需要清理上一个场景数据
    /// </summary>
    public sealed class NoneLastClearProcedure : LastClearProcedure
    {
        public override void OnInit()
        {
        }

        public override void OnClose()
        {
        }

        public override void OnLeave(ProcedureBase nextProcedureBase)
        {
        }

        public override void OnUpdate()
        {
        }

        public override void OnUpdateSecond()
        {
        }

        protected override async UniTask Clear(ProcedureBase lastProcedureBase)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
        }
    }
    /// <summary>
    /// 不需要预加载下一个场景数据
    /// </summary>
    public sealed class NonePreLoadProcedure : PreLoadProcedure
    {
        public override void OnInit()
        {
        }

        public override void OnClose()
        {
        }

        public override void OnLeave(ProcedureBase nextProcedureBase)
        {
        }

        public override void OnUpdate()
        {
        }

        public override void OnUpdateSecond()
        {
        }

        protected override async UniTask PreLoad(ProcedureBase lastProcedureBase)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
        }
    }
    /// <summary>
    /// 不需要初始化下一个场景
    /// </summary>
    public sealed class NoneNextSceneInitProcedure : NextSceneInitProcedure
    {
        public override void OnInit()
        {
        }

        public override void OnClose()
        {
        }

        public override void OnLeave(ProcedureBase nextProcedureBase)
        {
        }

        public override void OnUpdate()
        {
        }

        public override void OnUpdateSecond()
        {
        }

        protected override async UniTask SceneInit(ProcedureBase lastProcedureBase)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
        }
    }
}