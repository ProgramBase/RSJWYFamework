using System;
using RSJWYFamework.Runtime.Main;
using UnityEngine;

namespace RSJWYFamework.Runtime.ExceptionLogManager
{
    public class RSJWYException: UnityException
    {
        // 构造函数，接受异常信息和内部异常
        public RSJWYException(RSJWYFameworkEnum @enum,string message, Exception inner) : base($"[模块：{@enum}]\n异常内容：{message}\n内部错误：{inner}")
        {
        }
        // 构造函数，接受异常信息
        public RSJWYException(RSJWYFameworkEnum @enum,string message) : base($"模块：{@enum}]\n异常内容：{message}")
        {
        }
        public RSJWYException(RSJWYFameworkEnum @enum,Exception inner) : base($"模块：{@enum}]\n内部错误：{inner}")
        {
        }
    }
}