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
                //获取列表
                var MFALisRFH = Main.Main.YooAssetManager.RawPackage.LoadRawFileAsync("HotCode_HotCodeDLL");
                await MFALisRFH.ToUniTask();
                var loadLis = JsonConvert.DeserializeObject<HotCodeDLL>(MFALisRFH.GetRawFileText());
                var hotCodeBytesMap = new Dictionary<string, HotCodeBytes>();
                
                foreach (var asset in loadLis.HotCode)
                {
                    //string dllPath = MyTool.GetYooAssetWebRequestPath(asset);
                    string _dllname = $"{asset}.dll";
                    string _pdbname = $"{asset}.pdb";
                    //Debug.Log($"加载资产:{_n}");
                    //资源地址是否有效
                    if (Main.Main.YooAssetManager.RawPackage.CheckLocationValid(_dllname))
                    {
                        var hotcode = new HotCodeBytes();
                        //执行加载
                        var _rfh = Main.Main.YooAssetManager.RawPackage.LoadRawFileAsync(_dllname);
                        await _rfh.ToUniTask();
                        //转byte数组
                        hotcode.dllBytes = _rfh.GetRawFileData();
                        if (Main.Main.YooAssetManager.RawPackage.CheckLocationValid(_pdbname))
                        {
                            var _rfhPDB = Main.Main.YooAssetManager.RawPackage.LoadRawFileAsync(_pdbname);
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
                
                var MFAOTbytesMap = new Dictionary<string, byte[]>();
                foreach (var asset in loadLis.MetadataForAOTAssemblies)
                {
                    //string dllPath = MyTool.GetYooAssetWebRequestPath(asset);
                    string _dllname = $"{asset}.dll";
                    //Debug.Log($"加载资产:{_n}");
                    //资源地址是否有效
                    if (Main.Main.YooAssetManager.RawPackage.CheckLocationValid(_dllname))
                    {
                        //执行加载
                        var _rfh = Main.Main.YooAssetManager.RawPackage.LoadRawFileAsync(_dllname);
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