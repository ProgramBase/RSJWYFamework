using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Procedure;

namespace RSJWYFamework.Runtime.HybridCLR.Procedure
{
    /// <summary>
    /// 获取DLL
    /// </summary>
    public class LoadDLLByteProcedure:IProcedure
    {
        public IProcedureController pc { get; set; }
        public void OnInit()
        {
        }

        public void OnClose()
        {
        }

        public void OnEnter(IProcedure lastProcedure)
        {
            UniTask.Create(async () =>
            {
                var loadDLLDic = new Dictionary<string, byte[]>();
                await UniTask.SwitchToThreadPool();
                //获取列表
                var MFALisRFH = Main.Main.YooAssetManager.RawPackage.LoadRawFileAsync("Config_HotCodeDLL");
                await MFALisRFH.ToUniTask();
                var loadLis = JsonConvert.DeserializeObject<HotCodeDLL>(MFALisRFH.GetRawFileText());
                //拼接成一个整体
                var _DLL = loadLis.MetadataForAOTAssemblies.Union(loadLis.HotCode).ToList();
                //加载
                foreach (var asset in _DLL)
                {
                    //string dllPath = MyTool.GetYooAssetWebRequestPath(asset);
                    //string _n = $"{asset}.dll";
                    //Debug.Log($"加载资产:{_n}");
                    //资源地址是否有效
                    if (Main.Main.YooAssetManager.RawPackage.CheckLocationValid(asset))
                    {
                        //执行加载
                        var _rfh =  Main.Main.YooAssetManager.RawPackage.LoadRawFileAsync(asset);
                        //等待加载完成
                        await _rfh.Task;
                        //转byte数组
                        byte[] assetData = _rfh.GetRawFileData();
                        loadDLLDic.Add(asset,assetData);
                        //Debug.Log($"dll:{asset}  size:{assetData.Length}");
                        RSJWYLogger.Log($"热更加载DLL流程，加载资源dll:{asset}  size:{assetData.Length}");
                    }
                    else
                    {
                        RSJWYLogger.LogError($"热更获取DLL数据流程，加载资源文件地址：{asset}无效");
                    }
                }
                await UniTask.SwitchToMainThread();
                pc.SetBlackboardValue("LoadList",loadLis);
                pc.SetBlackboardValue("DLLDic",loadDLLDic);
                pc.SwitchProcedure(typeof(LoadHotCodeProcedure));
            });
        }

        public void OnLeave(IProcedure nextProcedure)
        {
        }

        public void OnUpdate()
        {
        }

        public void OnUpdateSecond()
        {
        }
    }
}