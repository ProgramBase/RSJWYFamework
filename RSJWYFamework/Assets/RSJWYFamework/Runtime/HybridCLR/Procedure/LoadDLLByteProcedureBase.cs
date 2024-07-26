using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Procedure;

namespace RSJWYFamework.Runtime.HybridCLR.Procedure
{
    /// <summary>
    /// 获取DLL
    /// </summary>
    public class LoadDLLByteProcedureBase:ProcedureBase
    {
        public override void OnInit()
        {
        }

        public override void OnClose()
        {
        }

        public override void OnEnter(ProcedureBase lastProcedureBase)
        {
            RSJWYLogger.Log(RSJWYFameworkEnum.HybridCLR,$"加载热更代码数据");
            UniTask.Create(async () =>
            {
                var loadDLLDic = new Dictionary<string, byte[]>();
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
                    string _n = $"{asset}.dll";
                    //Debug.Log($"加载资产:{_n}");
                    //资源地址是否有效
                    if (Main.Main.YooAssetManager.RawPackage.CheckLocationValid(_n))
                    {
                        //执行加载
                        var _rfh =  Main.Main.YooAssetManager.RawPackage.LoadRawFileAsync(_n);
                        //等待加载完成
                        await _rfh.Task;
                        //转byte数组
                        byte[] assetData = _rfh.GetRawFileData();
                        loadDLLDic.Add(asset,assetData);
                        //Debug.Log($"dll:{asset}  size:{assetData.Length}");
                        RSJWYLogger.Log($"热更加载DLL流程，加载资源dll:{_n}  size:{assetData.Length}");
                        _rfh.Release();
                    }
                    else
                    {
                        RSJWYLogger.Error($"热更获取DLL数据流程，加载资源文件地址：{_n}无效");
                    }
                }
                pc.SetBlackboardValue("LoadList",loadLis);
                pc.SetBlackboardValue("DLLDic",loadDLLDic);
                pc.SwitchProcedure(typeof(LoadHotCodeProcedureBase));
            });
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
    }
}