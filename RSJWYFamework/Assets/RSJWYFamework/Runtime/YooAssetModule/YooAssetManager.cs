using System;
using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Config;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Module;
using RSJWYFamework.Runtime.YooAssetModule.AsyncOperation;
using RSJWYFamework.Runtime.YooAssetModule.Tool;
using YooAsset;

namespace RSJWYFamework.Runtime.YooAssetModule
{
    public class YooAssetManager :  IModule
    {
        /// <summary>
        /// 包列表
        /// </summary>
        ConcurrentDictionary<string, ResourcePackage> _packages=new ();

        /// <summary>
        /// 获取包
        /// </summary>
        /// <param name="packageName">包名</param>
        /// <param name="package">获取的包，获取失败为null</param>
        /// <returns>是否成功获取包</returns>
        public bool GetPackage(string packageName,out ResourcePackage package)
        {
            package=null;
            if (_packages.TryGetValue(packageName, out package))
            {
                return true;
            }
            RSJWYLogger.Error(RSJWYFameworkEnum.YooAssets,$"输入的包名:{packageName}不存在");
            return false;
        }


        public void Init()
        {
            YooAssets.Initialize();
        }

        public async UniTask LoadPackage()
        {
            //获取数据并存入数据
            var projectConfig = Main.Main.DataManagerataManager.GetDataSetSB<ProjectConfig>();
            YooAssetManagerTool.Setting(projectConfig.hostServerIP, projectConfig.ProjectName, projectConfig.APPName, projectConfig.Version);
            UniTask[] taskArr=new UniTask[projectConfig.YooAssets.packages.Count];
            for (int i = 0; i < projectConfig.YooAssets.packages.Count; i++)
            {
                //配置异步任务
                LoadPackages operationR = new LoadPackages(projectConfig.YooAssets.packages[i].PackageName,  projectConfig.PlayMode);
                
                taskArr[i]=operationR.UniTask;
            }
            //等待完成
            await UniTask.WhenAll(taskArr);
            foreach (var package in projectConfig.YooAssets.packages)
            {
                _packages.TryAdd(package.PackageName, YooAssets.GetPackage(package.PackageName));
            }
        }

        public event Action InitOverEvent;

        public void Close()
        {
        }
    }
}