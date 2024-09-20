using System;
using RSJWYFamework.Runtime.Main;
using UnityEngine;

namespace RSJWYFamework.Runtime.ExceptionLogManager
{
    public class RSJWYException: UnityException
    {
        public RSJWYFameworkEnum Renum
        {
            get;
            private set;
        }
        public string message{
            get;
            private set;
        }
        // 构造函数，接受异常信息和内部异常
        public RSJWYException(RSJWYFameworkEnum Renum,string message, Exception inner) : base($"[模块：{Renum}]\n异常内容：{message}\n内部错误：{inner}")
        {
            this.Renum = Renum;
            this.message = message;
        }
        // 构造函数，接受异常信息
        public RSJWYException(RSJWYFameworkEnum Renum,string message) : base($"模块：{Renum}]\n异常内容：{message}")
        {
            this.Renum = Renum;
            this.message = message;
        }
        public RSJWYException(RSJWYFameworkEnum Renum,Exception inner) : base($"模块：{Renum}]\n内部错误：{inner}")
        {
            this.Renum = Renum;
            this.message = inner.ToString();
        }
        public RSJWYException(Exception inner) : base(inner.ToString())
        {
            this.Renum = RSJWYFameworkEnum.None;
            this.message = inner.ToString();
        }
        public RSJWYException(string err) : base(err)
        {
            this.Renum = RSJWYFameworkEnum.None;
            this.message = err;
        }

        public override string ToString()
        {
            return $"基础错误：{base.ToString()},模块：{Renum}]\n异常内容：{message} ";
        }
    }
}