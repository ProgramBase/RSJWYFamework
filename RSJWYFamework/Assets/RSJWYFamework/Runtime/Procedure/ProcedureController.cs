using System;
using System.Collections.Generic;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using UnityEngine;

namespace RSJWYFamework.Runtime.Procedure
{
    /// <summary>
    /// 默认的流程控制
    /// </summary>
    public class ProcedureController
    {
        /// <summary>
        /// 所属模块
        /// </summary>
        public IProcedureException modle { get; private set; }
        
        /// <summary>
        /// 当前流程
        /// </summary>
        private ProcedureBase _currentProcedureBase;

        /// <summary>
        /// 流程表
        /// </summary>
        private Dictionary<Type, ProcedureBase> Procedures = new();

        /// <summary>
        /// 所有流程的类型
        /// </summary>
        public List<Type> ProcedureTypes = new();

        /// <summary>
        /// 任意流程切换事件（上一个离开的流程、下一个进入的流程）
        /// </summary>
        public event Action<ProcedureBase, ProcedureBase> ProcedureSwitchEvent;
        
        /// <summary>
        /// 黑板数据
        /// </summary>
        private readonly Dictionary<string, System.Object> blackboard = new (100);
        
        
        private float _timer = 0;

        private ProcedureController _procedureControllerImplementation;
        //private IProcedureController _procedureControllerImplementation;

        public ProcedureController(IProcedureException module)
        {
            this.modle = module;
        }
        
        public void Init()
        {
            
        }

        public void Close()
        {
            foreach (var procedure in Procedures)
            {
                procedure.Value.OnClose();
            }
            Procedures.Clear();
        }

        public void SetBlackboardValue(string key, object value)
        {
            if (blackboard.ContainsKey(key) == false)
                blackboard.Add(key, value);
            else
                blackboard[key] = value;
        }

        public object GetBlackboardValue(string key)
        {
            if (blackboard.TryGetValue(key, out System.Object value))
            {
                return value;
            }
            else
            {
                RSJWYLogger.Warning(RSJWYFameworkEnum.YooAssets,$"未能从黑板中获取数据：{key}");
                return null;
            }
        }

        public void ClearBlackboard()
        {
            blackboard.Clear();
        }

        public Type GetNowProcedure()
        {
            return _currentProcedureBase.GetType();
        }

        public void OnUpdate(float time, float realtime)
        {
            if (_currentProcedureBase != null)
            {
                _currentProcedureBase.OnUpdate();

                if (_timer < 1)
                {
                    _timer += Time.deltaTime;
                }
                else
                {
                    _timer -= 1;
                    _currentProcedureBase.OnUpdateSecond();
                }
            }
        }


        public void SwitchProcedure(Type type)
        {
            if (type.IsAssignableFrom(typeof(ProcedureBase)))
                throw new RSJWYException(RSJWYFameworkEnum.Procedure, $"切换流程失败：流程 {type.Name} 并非继承自流程基类！");
            if (Procedures.ContainsKey(type))
            {
                if (_currentProcedureBase == Procedures[type])
                    return;

                var lastProcedure = _currentProcedureBase;
                var nextProcedure = Procedures[type];
                if (lastProcedure != null)
                {
                    lastProcedure.OnLeave(nextProcedure);
                }
                _currentProcedureBase = nextProcedure;
                nextProcedure.OnEnter(lastProcedure);

                ProcedureSwitchEvent?.Invoke(lastProcedure, nextProcedure);
            }
            else
            {
                throw new RSJWYException(RSJWYFameworkEnum.Procedure, $"切换流程失败：不存在流程 {type.Name} 或者流程未激活！");
            }
        }
        public void SwitchNextProcedure()
        {
            int index = ProcedureTypes.IndexOf(_currentProcedureBase.GetType());
            if (index >= ProcedureTypes.Count - 1)
            {
                SwitchProcedure(ProcedureTypes[0]);
            }
            else
            {
                SwitchProcedure(ProcedureTypes[index + 1]);
            }
        }

        public void StartProcedure(Type type)
        {
            SwitchProcedure(type);
        }


        public bool IsExistProcedur(Type type)
        {
            if (type.IsAssignableFrom(typeof(ProcedureBase)))
                throw new RSJWYException(RSJWYFameworkEnum.Procedure, $"流程 {type.Name} 并非继承自流程基类！");
            return Procedures.ContainsKey(type.GetType());
        }

        public void AddProcedure(ProcedureBase procedureBase)
        {
            Type _t = procedureBase.GetType();
            
            if (_t.IsAssignableFrom(typeof(ProcedureBase)))
                throw new RSJWYException(RSJWYFameworkEnum.Procedure, $"增加流程失败：流程 {_t.Name} 并非继承自流程基类！");
            
            if (!Procedures.ContainsKey(_t))
            {
                Procedures.Add(_t, procedureBase);
                ProcedureTypes.Add(_t);
                procedureBase.pc = this;
                procedureBase.OnInit();
            }
            else
            {
                throw new RSJWYException(RSJWYFameworkEnum.Procedure, $"添加流程失败：流程 {_t.Name} 已存在！");
            }
        }

        public void RemoveProcedure(Type type)
        {
            if (!Procedures.TryGetValue(type,out var procedure))
            {
                procedure.OnClose();
                Procedures.Remove(type);
                ProcedureTypes.Remove(type);
                if (_currentProcedureBase == procedure)
                {
                    _currentProcedureBase = null;
                }
            }
            else
            {
                throw new RSJWYException(RSJWYFameworkEnum.Procedure, $"移除流程失败：流程 {type.Name} 不存在！");
            }
        }

     
    }
}