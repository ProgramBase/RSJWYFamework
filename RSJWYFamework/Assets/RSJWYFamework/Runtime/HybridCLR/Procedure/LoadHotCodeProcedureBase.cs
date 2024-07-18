using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
using RSJWYFamework.Runtime.Default.Manager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Procedure;

namespace RSJWYFamework.Runtime.HybridCLR.Procedure
{
    /// <summary>
    /// 加载DLL流程
    /// </summary>
    public class LoadHotCodeProcedureBase : ProcedureBase
    {

        public override void OnInit()
        {
        }

        public override void OnClose()
        {
        }

        public override void OnEnter(ProcedureBase lastProcedureBase)
        {
            RSJWYLogger.Log($"加载热更代码");
            UniTask.Create(async () =>
            {
                await UniTask.WaitForSeconds(0.5f);
                //获取数据
                var _loadLis = (HotCodeDLL)pc.GetBlackboardValue("LoadList");
                var _DllDic = (Dictionary<string, byte[]>)pc.GetBlackboardValue("DLLDic");
                Dictionary<string, Assembly> hotCode = new();

                await UniTask.SwitchToThreadPool();
                //加载到对应位置
                //加载补充元数据
                string _str_err_name = "";
                try
                {
                    // 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
                    // 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
                    HomologousImageMode mode = HomologousImageMode.SuperSet;
                    foreach (string aotDllName in _loadLis.MetadataForAOTAssemblies)
                    {
                        _str_err_name = aotDllName;
                        byte[] dllBytes = _DllDic[aotDllName];
                        // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                        LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                        if (err != LoadImageErrorCode.OK)
                        {
                            RSJWYLogger.Error($"热更加载DLL流程，加载补充元：{_str_err_name} 时发生异常，HCLR错误码{err.ToString()}");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    pc.modle.Exception(new ProcedureException($"热更加载DLL流程，加载补充元：{_str_err_name} 时发生异常，{ex}"));
                }

                //加载热更代码，注意加载顺序
                try
                {
#if UNITY_EDITOR
                    foreach (var _hotAss in _loadLis.MetadataForAOTAssemblies)
                    {
                        _str_err_name = _hotAss;
                        hotCode.Add(_hotAss,
                            System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == _hotAss));
                    }
#else
                    foreach (var _hotAss in _loadLis.MetadataForAOTAssemblies)
                    {
                        _str_err_name = _hotAss;
                        byte[] _dll = _DllDic[_hotAss];
                        hotCode.Add(_hotAss, Assembly.Load(_dll));
                    }
#endif
                }
                catch (System.Exception ex)
                {
                    pc.modle.Exception(new ProcedureException($"热更加载DLL流程，加载热更：{_str_err_name} 时发生异常，{ex}"));
                }

                await UniTask.SwitchToMainThread();
                pc.SetBlackboardValue("HotCodeAssembly",hotCode);
                pc.SwitchProcedure(typeof(LoadHotCodeDone));
                
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