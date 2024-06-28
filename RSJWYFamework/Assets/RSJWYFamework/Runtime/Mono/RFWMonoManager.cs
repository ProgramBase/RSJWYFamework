using System;
using MyFamework.Runtime.Base;
using RSJWYFamework.Runtime.Senseshield;
using UnityEngine;
namespace RSJWYFamework.Runtime.Mono
{
    /// <summary>
    /// 框架的管理器，unity挂载
    /// </summary>
    public class RFWMonoManager:SingletonBaseMono<RFWMonoManager>
    {

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);
            Main.Main.Instance.AddModule<SenseshieldServer>();
        }
    }
}