using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Base;
using RSJWYFamework.Runtime.Config;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using UnityEngine;
using RSJWYFamework.Runtime.Driver;
using RSJWYFamework.Runtime.Scene;
using RSJWYFamework.Runtime.StateMachine;
using Script.AOT.PatchWindows;


namespace Script.AOT
{
    /// <summary>
    /// 框架的管理器，unity挂载
    /// </summary>
    public class LoadSystemAOT:MonoBehaviour
    {
        protected void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            StartApp().Forget();
        }

        async UniTaskVoid StartApp()
        {
            await UniTask.WaitForSeconds(0.5f);
            var pw=Resources.Load<GameObject>("PatchWindow");
            var p=Instantiate(pw);
            p.name = "PatchWindow";
            await UniTask.WaitForSeconds(0.5f);
            
            PatchEventDefine.UpdateProgressEvent.SendEventMessage(0,"正在初始化");
            
            RSJWYLogger.Log("加载项目配置文件，并加载到数据管理中···");
            PatchEventDefine.UpdateProgressEvent.SendEventMessage(0.05f,"加载项目配置文件，并加载到数据管理中···");
            
            var projectset =  Resources.Load<ProjectConfig>("ProjectConfig");
            Main.DataManagerataManager.AddDataSet(projectset);
            
            RSJWYLogger.Log($"日志等级：{projectset.Loglevel}");
            RSJWYLogger.Loglevel = projectset.Loglevel;
            RSJWYLogger.Log("等待包初始化");
            PatchEventDefine.UpdateProgressEvent.SendEventMessage(0.1f,"等待包初始化");
            await Main.YooAssetManager.LoadPackage();
            RSJWYLogger.Log("包初始化完成，加载热更代码");
            PatchEventDefine.UpdateProgressEvent.SendEventMessage(0.8f,"加载程序");
            await Main.HybridClrManager.LoadHotCodeDLL();
            PatchEventDefine.UpdateProgressEvent.SendEventMessage(1f,"初始化完成");
            var toScecne= new SwitchSceneOperation(new LoadHotScene(),"加载热更入口场景",false);
            toScecne.StartProcedure();
        }
    }

    internal sealed class LoadHotScene : LoadNextSceneStateNode
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

        protected override async UniTask LoadNextScene(StateNodeBase lastStateNodeBase)
        {
            RSJWYLogger.Log("加载入口场景");
            Main.YooAssetManager.GetPackage("PrefabPackage",out var package);
            var scene = package.LoadSceneAsync("Scenes_Enter");
            await scene.ToUniTask();
        }
    }
}