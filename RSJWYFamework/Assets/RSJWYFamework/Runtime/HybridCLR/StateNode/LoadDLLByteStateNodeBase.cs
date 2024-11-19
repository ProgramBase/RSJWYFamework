using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.StateMachine;
using YooAsset;

namespace RSJWYFamework.Runtime.HybridCLR.StateNode
{
    /// <summary>
    /// 获取DLL
    /// </summary>
    public class LoadDLLByteStateNodeBase:StateNodeBase
    {
        public override void OnInit()
        {
        }

        public override void OnClose()
        {
        }

        public override void OnEnter(StateNodeBase lastStateNodeBase)
        {
            RSJWYLogger.Log(RSJWYFameworkEnum.HybridCLR,$"加载热更代码数据");
            UniTask.Create(async () =>
            {
                Main.Main.YooAssetManager.GetPackage("RawFilePackage", out var package);
                if (!package.CheckLocationValid("HotUpdateCode_HotList"))
                {
                    RSJWYLogger.Error("无法加载列表文件");
                    return;
                }
                    
                //获取列表
                var MFALisRFH = package.LoadRawFileAsync("HotUpdateCode_HotList");
                await MFALisRFH.ToUniTask();
                var loadLis = JsonConvert.DeserializeObject<HotCodeDLL>(MFALisRFH.GetRawFileText());
                var hotCodeBytesMap = new Dictionary<string, HotCodeBytes>();
                //加载热更代码和pdb
                foreach (var asset in loadLis.HotCode)
                {
                    //string dllPath = MyTool.GetYooAssetWebRequestPath(asset);
                    string _dllname = $"{asset}.dll";
                    string _pdbname = $"{asset}.pdb";
                    //Debug.Log($"加载资产:{_n}");
                    //资源地址是否有效
                    if (package.CheckLocationValid(_dllname))
                    {
                        var hotcode = new HotCodeBytes();
                        //执行加载
                        var _rfh = package.LoadRawFileAsync(_dllname);
                        await _rfh.ToUniTask();
                        //转byte数组
                        hotcode.dllBytes = _rfh.GetRawFileData();
                        if (package.CheckLocationValid(_pdbname))
                        {
                            var _rfhPDB = package.LoadRawFileAsync(_pdbname);
                            await _rfhPDB.ToUniTask();
                            //转byte数组
                            hotcode.pdbBytes = _rfhPDB.GetRawFileData();
                        }
                        else
                        {
                            RSJWYLogger.Warning($"热更获取PDB数据流程，加载PDB资源文件地址：{_dllname}无效，将无法打印行号");
                        }
                        hotCodeBytesMap.Add(asset,hotcode);
                        //Debug.Log($"dll:{asset}  size:{assetData.Length}");
                        RSJWYLogger.Log($"热更加载DLL流程，加载资源dll:{_dllname}");
                        _rfh.Release();
                    }
                    else
                    {
                        RSJWYLogger.Error($"热更获取DLL数据流程，加载热更代码资源文件地址：{_dllname}无效");
                    }
                }
                //加载元数据
                var MFAOTbytesMap = new Dictionary<string, byte[]>();
                foreach (var asset in loadLis.MetadataForAOTAssemblies)
                {
                    //string dllPath = MyTool.GetYooAssetWebRequestPath(asset);
                    string _dllname = $"{asset}.dll";
                    //Debug.Log($"加载资产:{_n}");
                    //资源地址是否有效
                    if (package.CheckLocationValid(_dllname))
                    {
                        //执行加载
                        var _rfh = package.LoadRawFileAsync(_dllname);
                        await _rfh.ToUniTask();
                        //转byte数组
                        var MFAOT = _rfh.GetRawFileData();
                        MFAOTbytesMap.Add(asset,MFAOT);
                        //Debug.Log($"dll:{asset}  size:{assetData.Length}");
                        RSJWYLogger.Log($"热更加载DLL流程，加载补充元数据资源dll:{_dllname}");
                        _rfh.Release();
                    }
                    else
                    {
                        RSJWYLogger.Error($"热更获取DLL数据流程，加载资源文件地址：{_dllname}无效");
                    }
                }
                pc.SetBlackboardValue("LoadList",loadLis);
                pc.SetBlackboardValue("HotcodeDic",hotCodeBytesMap);
                pc.SetBlackboardValue("MFAOTDic",MFAOTbytesMap);
                pc.SwitchProcedure(typeof(LoadHotCodeStateNodeBase));
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
}