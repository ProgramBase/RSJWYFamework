using System;
using System.Collections.Generic;
using RSJWYFamework.Runtime.AsyncOperation;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Procedure;

namespace RSJWYFamework.Runtime.Scene
{
    /// <summary>
    /// 切换场景流程，请不要进行等待，并且确保初始化后没有后文
    /// 有参构造函数会在初始化时进行调用启动函数，并开始完整流程
    /// </summary>
    public class SwitchSceneOperation:IProcedureUser
    {
        private readonly ProcedureController pc;
        private bool run=false;
        
        public string SceneName { get; private set; }
        Type _lastClearType; 
        Type _preLoadType;
        Type _loadNextSceneType;
        Type _NextSceneInitType;

        /// <summary>
        /// 初始化场景切换流程
        /// isAuto==true时，自动调用StartProcedure执行，否则需要手动调用StartProcedure
        /// </summary>
        /// <param name="sceneName">场景名称-便于调试</param>
        /// <param name="isAuto">是否直接添加到执行队列执行</param>
        /// <param name="lastClearProcedure">清理上一个场景资源-可为null</param>
        /// <param name="preLoadProcedure">预加载下一个场景资源-可为null</param>
        /// <param name="loadNextSceneProcedure">加载下一个场景-必须实现，直接跳转</param>
        /// <param name="nextSceneInitProcedure">下一个场景初始化操作-可为null</param>
        /// <param name="blackboardKeyValue">设置黑板数据</param>
        public SwitchSceneOperation(LoadNextSceneProcedure loadNextSceneProcedure,string sceneName,bool isAuto=true,LastClearProcedure lastClearProcedure=null, PreLoadProcedure preLoadProcedure=null, NextSceneInitProcedure nextSceneInitProcedure=null,Dictionary<string,object>blackboardKeyValue=null)
        {
            pc = new ProcedureController(this,"场景切换",$"{sceneName}");
            pc.ProcedureSwitchEvent += SwitchSceneOperationEvent;
            
            pc.AddProcedure<SwitchToTransferProcedure>();
            lastClearProcedure ??= new NoneLastClearProcedure();
            pc.AddProcedure(lastClearProcedure);
            _lastClearType = lastClearProcedure.GetType();
            preLoadProcedure??= new NonePreLoadProcedure();
            pc.AddProcedure(preLoadProcedure);
            _loadNextSceneType = preLoadProcedure.GetType();
            if (_loadNextSceneType==null)
            {
                throw new RSJWYException("请确保加载下一个场景流程不为空");
            }
            pc.AddProcedure(loadNextSceneProcedure);
            _preLoadType = loadNextSceneProcedure.GetType();
            nextSceneInitProcedure??= new NoneNextSceneInitProcedure();
            pc.AddProcedure(nextSceneInitProcedure);
            _NextSceneInitType= nextSceneInitProcedure.GetType();
            pc.AddProcedure<SwitchSceneDoneProcedure>();
            if (blackboardKeyValue!= null)
            {
                foreach (var blackboard in blackboardKeyValue)
                {
                    pc.SetBlackboardValue(blackboard.Key,blackboard.Value);
                }
            }
            pc.SetBlackboardValue("LastClearType",_lastClearType);
            pc.SetBlackboardValue("PreLoadType",_preLoadType);
            pc.SetBlackboardValue("LoadNextSceneType",_loadNextSceneType);
            pc.SetBlackboardValue("NextSceneInitType",_NextSceneInitType);
            //添加到管理器并启动
            if (isAuto)
            {
                StartProcedure();
                run = true;
            }
        }

        /// <summary>
        /// 启动流程
        /// </summary>
        public void StartProcedure()
        {
            if (run)
                return;
            run = true;
            Main.Main.ProcedureControllerManager.AddProcedure(pc.Name,pc);
            pc.StartProcedure<SwitchToTransferProcedure>();
        }
        /// <summary>
        /// 流程切换回调
        /// </summary>
        /// <param name="last">上一流程</param>
        /// <param name="next">下一流程</param>
        void SwitchSceneOperationEvent(ProcedureBase last, ProcedureBase next)
        {
            if (next is SwitchSceneDoneProcedure)
            {
                //切换到结尾后，退出
                Main.Main.ProcedureControllerManager.RemoveProcedure(pc.Name);
            }
        }

        public void Exception(ProcedureException exception)
        {
            RSJWYLogger.Error($"发生异常：{exception}");
        }

        public void Abort(string reason)
        {
            RSJWYLogger.Error($"流程中止：{reason}");
        }
    }
}