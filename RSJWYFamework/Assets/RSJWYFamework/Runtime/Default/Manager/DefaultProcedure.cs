using System;
using System.Collections.Generic;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Module;
using RSJWYFamework.Runtime.Procedure;
using UnityEngine;

namespace RSJWYFamework.Runtime.Default.Manager
{
    /// <summary>
    /// 默认的流程控制
    /// </summary>
    public class DefaultProcedureController:IProcedureController
    {
        /// <summary>
        /// 所属模块
        /// </summary>
        public IModule modle;
        
        /// <summary>
        /// 当前流程
        /// </summary>
        private IProcedure CurrentProcedure;

        /// <summary>
        /// 流程表
        /// </summary>
        private Dictionary<Type, IProcedure> Procedures = new();

        /// <summary>
        /// 所有流程的类型
        /// </summary>
        public List<Type> ProcedureTypes = new();

        /// <summary>
        /// 任意流程切换事件（上一个离开的流程、下一个进入的流程）
        /// </summary>
        public event Action<IProcedure, IProcedure> ProcedureSwitchEvent;
        
        
        private float _timer = 0;
        private IProcedureController _procedureControllerImplementation;

        public DefaultProcedureController(IModule module)
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

        public void OnUpdate(float time, float realtime)
        {
            if (CurrentProcedure != null)
            {
                CurrentProcedure.OnUpdate();

                if (_timer < 1)
                {
                    _timer += Time.deltaTime;
                }
                else
                {
                    _timer -= 1;
                    CurrentProcedure.OnUpdateSecond();
                }
            }
        }


        public void SwitchProcedure(Type type)
        {
            if (type.IsAssignableFrom(typeof(IProcedure)))
                throw new RSJWYException(RSJWYFameworkEnum.Procedure, $"切换流程失败：流程 {type.Name} 并非继承自流程基类！");
            if (Procedures.ContainsKey(type))
            {
                if (CurrentProcedure == Procedures[type])
                    return;

                var lastProcedure = CurrentProcedure;
                var nextProcedure = Procedures[type];
                if (lastProcedure != null)
                {
                    lastProcedure.OnLeave(nextProcedure);
                }
                nextProcedure.OnEnter(lastProcedure);
                CurrentProcedure = nextProcedure;

                ProcedureSwitchEvent?.Invoke(lastProcedure, nextProcedure);
            }
            else
            {
                throw new RSJWYException(RSJWYFameworkEnum.Procedure, $"切换流程失败：不存在流程 {type.Name} 或者流程未激活！");
            }
        }
        public void SwitchNextProcedure()
        {
            int index = ProcedureTypes.IndexOf(CurrentProcedure.GetType());
            if (index >= ProcedureTypes.Count - 1)
            {
                SwitchProcedure(ProcedureTypes[0]);
            }
            else
            {
                SwitchProcedure(ProcedureTypes[index + 1]);
            }
        }


        public bool IsExistProcedur(Type type)
        {
            if (type.IsAssignableFrom(typeof(IProcedure)))
                throw new RSJWYException(RSJWYFameworkEnum.Procedure, $"流程 {type.Name} 并非继承自流程基类！");
            return Procedures.ContainsKey(type.GetType());
        }

        public void AddProcedure(IProcedure procedure)
        {
            Type _t = procedure.GetType();
            
            if (_t.IsAssignableFrom(typeof(IProcedure)))
                throw new RSJWYException(RSJWYFameworkEnum.Procedure, $"增加流程失败：流程 {_t.Name} 并非继承自流程基类！");
            
            if (!Procedures.ContainsKey(_t))
            {
                Procedures.Add(_t, procedure);
                ProcedureTypes.Add(_t);
                procedure.pc = this;
                procedure.OnInit();
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
                if (CurrentProcedure == procedure)
                {
                    CurrentProcedure = null;
                }
            }
            else
            {
                throw new RSJWYException(RSJWYFameworkEnum.Procedure, $"移除流程失败：流程 {type.Name} 不存在！");
            }
        }

     
    }
}