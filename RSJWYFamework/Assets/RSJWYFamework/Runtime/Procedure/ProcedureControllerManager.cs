using System.Collections.Generic;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Module;

namespace RSJWYFamework.Runtime.Procedure
{
    public class ProcedureControllerManager:IModule ,ILife
    {
        Dictionary<string,ProcedureController>Procedures = new(100);
        /// <summary>
        /// 添加一个流程控制器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="procedureController"></param>
        public void AddProcedure(string name,ProcedureController procedureController) 
        {
            if (Procedures.ContainsKey(name))
            {
                RSJWYLogger.Error($"添加流程失败：流程 {name} 已存在！");
            }
            else
            {
                Procedures.Add(name,procedureController);
            }
        }
        /// <summary>
        /// 移除一个流程控制器
        /// </summary>
        public void RemoveProcedure(string name) 
        {
            if (Procedures.ContainsKey(name))
            {
                Procedures.Remove(name);
            }
        }
        /// <summary>
        /// 获取一个流程控制器
        /// </summary>
        /// <param name="name">流程控制器绑定的名字</param>
        /// <returns></returns>
        public ProcedureController GetProcedure(string name)
        {
            if (Procedures.ContainsKey(name))
            {
                return Procedures[name];
            }
            return null;
        }
        
        public void Init()
        {
            
        }

        public void Close()
        {
            
        }

        public void Update(float time, float deltaTime)
        {
            foreach (var procedure in Procedures)
            {
                procedure.Value.OnUpdate(time, deltaTime);
            }
        }

        public void UpdatePerSecond(float time)
        {
            foreach (var procedure in Procedures)
            {
                procedure.Value.OnUpdateSecond(time);
            }
        }

        public void FixedUpdate()
        {
        }

        public void LateUpdate()
        {
        }

        public uint Priority()=> 0;
    }
}