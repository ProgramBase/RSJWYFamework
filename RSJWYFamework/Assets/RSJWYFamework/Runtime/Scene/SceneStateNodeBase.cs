using System;
using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.StateMachine;
using UnityEngine.SceneManagement;
using YooAsset;

namespace RSJWYFamework.Runtime.Scene
{
    /// <summary>
    /// 离开当前场景前处理
    /// </summary>
    public abstract class PreDepartureProcessingStateNodeBase : StateNodeBase
    {
        public override void OnEnter(StateNodeBase lastStateNodeBase)
        {
            UniTask.Create(async () =>
            {
                RSJWYLogger.Log($"{pc.OtherInfo}--离开当前场景前相关处理");
                //初始化并打开过度遮罩
                await Main.Main.SceneTransition.ToTransferScene();
                Main.Main.SceneTransition.UpdateProgress(1, "切换场景中...");
                await PreDepartureProcessing(lastStateNodeBase);
                pc.SwitchProcedure<SwitchToTransferStateNode>();
            });
        }

        /// <summary>
        /// 清理上一个场景相关资源
        /// </summary>
        protected abstract UniTask PreDepartureProcessing(StateNodeBase lastStateNodeBase);
    }
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
                Main.Main.SceneTransition.UpdateProgress(5, "清理资源中...");
                await Clear(lastStateNodeBase);
                Main.Main.SceneTransition.UpdateProgress(20, "清理完毕...");
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
                Main.Main.SceneTransition.UpdateProgress(21, "正在预加载...");
                await PreLoad(lastStateNodeBase);
                Main.Main.SceneTransition.UpdateProgress(60, "预加载完成...");
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
                Main.Main.SceneTransition.UpdateProgress(61, "正在进入...");
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
                Main.Main.SceneTransition.UpdateProgress(80, "正在初始化...");
                await SceneInit(lastStateNodeBase);
                Main.Main.SceneTransition.UpdateProgress(100, "初始化完成...");
                await Main.Main.SceneTransition.ToNextScene();
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