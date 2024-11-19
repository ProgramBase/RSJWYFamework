using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.StateMachine;

namespace RSJWYFamework.Runtime.Scene
{
    /// <summary>
    /// 不需要清理上一个场景数据
    /// </summary>
    public sealed class NoneLastClearStateNode : LastClearStateNode
    {
        public override void OnInit()
        {
        }

        public override void OnClose()
        {
        }

        public override void OnLeave(StateNodeBase nextStateNodeBase)
        {
        }

        public override void OnUpdate()
        {
        }

        public override void OnUpdateSecond()
        {
        }

        protected override async UniTask Clear(StateNodeBase lastStateNodeBase)
        {
            await UniTask.CompletedTask;
        }
    }
    /// <summary>
    /// 不需要预加载下一个场景数据
    /// </summary>
    public sealed class NonePreLoadStateNode : PreLoadStateNode
    {
        public override void OnInit()
        {
        }

        public override void OnClose()
        {
        }

        public override void OnLeave(StateNodeBase nextStateNodeBase)
        {
        }

        public override void OnUpdate()
        {
        }

        public override void OnUpdateSecond()
        {
        }

        protected override async UniTask PreLoad(StateNodeBase lastStateNodeBase)
        {
            await UniTask.CompletedTask;
        }
    }
    /// <summary>
    /// 不需要初始化下一个场景
    /// </summary>
    public sealed class NoneNextSceneInitStateNode : NextSceneInitStateNode
    {
        public override void OnInit()
        {
        }

        public override void OnClose()
        {
        }

        public override void OnLeave(StateNodeBase nextStateNodeBase)
        {
        }

        public override void OnUpdate()
        {
        }

        public override void OnUpdateSecond()
        {
        }

        protected override async UniTask SceneInit(StateNodeBase lastStateNodeBase)
        {
            await UniTask.CompletedTask;
        }
    }
}