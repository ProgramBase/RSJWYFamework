using System;
using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.StateMachine;
using UnityEngine.SceneManagement;
using YooAsset;

namespace RSJWYFamework.Runtime.Scene
{
    /// <summary>
    /// 切换到中转场景
    /// </summary>
    public sealed class SwitchToTransferStateNode:StateNodeBase
    {
        public override void OnInit()
        {
        }

        public override void OnClose()
        {
        }

        public override void OnEnter(StateNodeBase lastStateNodeBase)
        {
            UniTask.Create(async () =>
            {
                RSJWYLogger.Log($"{pc.OtherInfo}--切换到中转场景");
                //初始化并打开过度遮罩
                var transition = SceneTransition.Instance();
                pc.SetBlackboardValue("SceneTransition",transition);
                await transition.ToTransferScene();
                //切换场景
                var sceneLoadOperation =SceneManager.LoadSceneAsync("SwitchScene");
                await sceneLoadOperation.ToUniTask();
                sceneLoadOperation.allowSceneActivation = true;
                //进入下一个流程
                await UniTask.Yield(PlayerLoopTiming.PreLateUpdate);
                var nextType= (Type)pc.GetBlackboardValue("LastClearType");
                pc.SwitchProcedure(nextType);
            });
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
    }
    /// <summary>
    /// 清理资源-在中转场景内调用
    /// </summary>
    public abstract class LastClearStateNode:StateNodeBase
    {
        public override void OnEnter(StateNodeBase lastStateNodeBase)
        {
            UniTask.Create(async () =>
            {
                RSJWYLogger.Log($"{pc.OtherInfo}--清理不需要的资源");
                await Clear(lastStateNodeBase);
                var nextType= (Type)pc.GetBlackboardValue("PreLoadType");
                pc.SwitchProcedure(nextType);
            });
        }

        /// <summary>
        /// 清理上一个场景相关资源
        /// </summary>
        protected abstract UniTask Clear(StateNodeBase lastStateNodeBase);
    }
    /// <summary>
    /// 加载下一个场景资源
    /// </summary>
    public abstract class PreLoadStateNode:StateNodeBase
    {
        public override void OnEnter(StateNodeBase lastStateNodeBase)
        {
            UniTask.Create(async () =>
            {
                RSJWYLogger.Log($"{pc.OtherInfo}--预加载下一场景需要的资源");
                await PreLoad(lastStateNodeBase);
                var nextType= (Type)pc.GetBlackboardValue("LoadNextSceneType");
                pc.SwitchProcedure(nextType);
            });
        }
        /// <summary>
        /// 预加载下一个场景资源
        /// </summary>
        protected abstract UniTask PreLoad(StateNodeBase lastStateNodeBase);
    }
    /// <summary>
    /// 加载并切换下一个场景
    /// </summary>
    public abstract class LoadNextSceneStateNode:StateNodeBase
    {
        public override void OnEnter(StateNodeBase lastStateNodeBase)
        {
            UniTask.Create(async () =>
            {
                RSJWYLogger.Log($"{pc.OtherInfo}--加载并切换下一个场景");
                await LoadNextScene(lastStateNodeBase);
                var nextType= (Type)pc.GetBlackboardValue("NextSceneInitType");
                pc.SwitchProcedure(nextType);
            });
        }
        protected abstract UniTask LoadNextScene(StateNodeBase lastStateNodeBase);
    }

    /// <summary>
    /// 下一个场景初始化流程
    /// </summary>
    public abstract class NextSceneInitStateNode : StateNodeBase
    {
        public override void OnEnter(StateNodeBase lastStateNodeBase)
        {
            UniTask.Create(async () =>
            {
                RSJWYLogger.Log($"{pc.OtherInfo}--正在初始化下一个场景");
                await SceneInit(lastStateNodeBase);
                pc.SwitchProcedure<SwitchSceneDoneStateNode>();
            });
        }
        protected abstract UniTask SceneInit(StateNodeBase lastStateNodeBase);
    }
    /// <summary>
    /// 流程执行结束
    /// </summary>
    public sealed class SwitchSceneDoneStateNode:StateNodeBase
    {
        public override void OnInit()
        {
            
        }

        public override void OnClose()
        {
            
        }

        public override void OnEnter(StateNodeBase lastStateNodeBase)
        {
            RSJWYLogger.Log($"{pc.OtherInfo}--加载场景流程结束");
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
    }
}