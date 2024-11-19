using System;

namespace RSJWYFamework.Runtime.StateMachine
{
    /// <summary>
    /// 流程使用者接口
    /// </summary>
    public interface IStateMachineUser
    {
        /// <summary>
        /// 发生异常
        /// </summary>
        /// <param name="exception"></param>
        void Exception( ProcedureException exception);
        /// <summary>
        /// 终止
        /// </summary>
        void Abort(string reason);
    }

    public class ProcedureException : Exception
    {
        /// <summary>
        /// 流程发生异常
        /// </summary>
        /// <param name="message"></param>
        public ProcedureException(string message) : base(message)
        {
        }

        public ProcedureException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}