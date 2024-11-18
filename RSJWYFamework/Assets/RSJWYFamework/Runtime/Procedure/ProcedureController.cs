using System;
using System.Collections.Generic;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;

namespace RSJWYFamework.Runtime.Procedure
{
    /// <summary>
    /// 流程控制器
    /// </summary>
    public class ProcedureController
    {
        /// <summary>
        /// 流程控制器名称
        /// </summary>
        public string Name{get;private set;}
        /// <summary>
        /// 其他信息
        /// </summary>
        public string OtherInfo{get;private set;}
        /// <summary>
        /// 所属使用者
        /// </summary>
        public IProcedureUser User { get; private set; }
        
        /// <summary>
        /// 当前流程
        /// </summary>
        private ProcedureBase _currentProcedureBase;
        
        /// <summary>
        /// 任意流程切换事件（上一个离开的流程、下一个进入的流程）
        /// </summary>
        public event Action<ProcedureBase, ProcedureBase> ProcedureSwitchEvent;
        
        /// <summary>
        /// 黑板数据
        /// </summary>
        private readonly Dictionary<string, System.Object> blackboard = new (100);
        
        /// <summary>
        /// 流程表
        /// </summary>
        private readonly Dictionary<Type, ProcedureBase> Procedures = new(100);
        /// <summary>
        /// 所有流程
        /// </summary>
        private readonly List<Type> ProcedureTypes = new(100);


       

        public ProcedureController(IProcedureUser module,string name,string otherInfo="")
        {
            this.User = module;
            this.Name = name;
            this.OtherInfo = otherInfo;
        }
        
        /// <summary>
        /// 帧更新
        /// </summary>
        /// <param name="time"></param>
        /// <param name="realtime"></param>
        public void OnUpdate(float time, float realtime)
        {
            _currentProcedureBase?.OnUpdate();
        }
        /// <summary>
        /// 秒更新
        /// </summary>
        /// <param name="time"></param>
        /// <param name="realtime"></param>
        public void OnUpdateSecond(float time)
        {
            _currentProcedureBase?.OnUpdateSecond();
        }
        
        #region 黑板行为
        /// <summary>
        /// 设置黑板数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetBlackboardValue(string key, object value)
        {
            if (blackboard.ContainsKey(key))
                blackboard.Add(key, value);
            else
                blackboard[key] = value;
        }
        /// <summary>
        /// 获取黑板数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetBlackboardValue(string key)
        {
            if (blackboard.TryGetValue(key, out Object value))
            {
                return value;
            }
            else
            {
                RSJWYLogger.Warning(RSJWYFameworkEnum.YooAssets,$"未能从黑板中获取数据：{key}");
                return null;
            }
        }
        /// <summary>
        /// 清空黑板数据
        /// </summary>
        public void ClearBlackboard()
        {
            blackboard.Clear();
        }

        #endregion


        #region 流程行为

        /// <summary>
        /// 获取现在正在执行的流程
        /// </summary>
        /// <returns></returns>
        public Type GetNowProcedure()
        {
            return _currentProcedureBase.GetType();
        }
        /// <summary>
        /// 切换到指定流程
        /// </summary>
        /// <typeparam name="TProcedureBase">流程类型</typeparam>
        public void SwitchProcedure<TProcedureBase>() where TProcedureBase : ProcedureBase
        {
            SwitchProcedure(typeof(TProcedureBase));
        }
        
        /// <summary>
        /// 切换到指定流程
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="RSJWYException"></exception>
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
        /// <summary>
        /// 切换到下一流程，
        /// 如果当前是最后一个流程，则切换到第一个流程
        /// </summary>
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
        /// <summary>
        /// 开始流程
        /// </summary>
        /// <param name="type">指定的开始源</param>
        public void StartProcedure(Type type)
        {
            SwitchProcedure(type);
        }
        /// <summary>
        /// 开始流程，从指定的开始源开始
        /// </summary>
        /// <typeparam name="TProcedureBase"></typeparam>
        public void StartProcedure<TProcedureBase>()
        {
            SwitchProcedure(typeof(TProcedureBase));
        }
        /// <summary>
        /// 开始流程，从第一个开始
        /// </summary>
        public void StartProcedure()
        {
            SwitchProcedure(ProcedureTypes[0]);
        }
        
        /// <summary>
        /// 判断一个流程是否存在
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="RSJWYException"></exception>
        public bool IsExistProcedur(Type type)
        {
            if (type.IsAssignableFrom(typeof(ProcedureBase)))
                throw new RSJWYException(RSJWYFameworkEnum.Procedure, $"流程 {type.Name} 并非继承自流程基类！");
            return Procedures.ContainsKey(type.GetType());
        }
        /// <summary>
        /// 添加一个流程
        /// </summary>
        /// <param name="procedureBase"></param>
        /// <exception cref="RSJWYException"></exception>
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
        /// <summary>
        /// 添加一个流程
        /// 使用本方法必须传递的是一个包含无参构造函数的流程类，否则会抛出异常
        /// 否则请使用AddProcedure(ProcedureBase procedureBase)传递实例化好的
        /// </summary>
        /// <param name="procedureBase"></param>
        /// <exception cref="RSJWYException"></exception>
        public void AddProcedure<TProcedureBase>() where TProcedureBase : ProcedureBase,new()
        {
            var type = typeof(TProcedureBase);
            var procedure = Activator.CreateInstance<TProcedureBase>();
            if (!Procedures.ContainsKey(type))
            {
                Procedures.Add(type, procedure);
                ProcedureTypes.Add(type);
                procedure.pc = this;
                procedure.OnInit();
            }
            else
            {
                throw new RSJWYException(RSJWYFameworkEnum.Procedure, $"添加流程失败：流程 {type.Name} 已存在！");
            }
        }
        /// <summary>
        /// 移除一个流程
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="RSJWYException"></exception>
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

        #endregion

     
    }
}