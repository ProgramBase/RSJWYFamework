using System;
using System.Diagnostics;
using RSJWYFamework.Runtime.AsyncOperation;
using RSJWYFamework.Runtime.Logger;
using UnityEngine;
using RSJWYFamework.Runtime.Main;

namespace RSJWYFamework.Runtime.Driver
{
    /// <summary>
    /// 服务驱动程序
    /// </summary>
    internal sealed class RSJWYFameworkDriverService:MonoBehaviour
    {
        private static int LastestUpdateFrame = 0;
        private float timer = 0f;
        private float interval = 1f;

       
        void Update()
        {
            Main.Main.Update(Time.time,Time.deltaTime);
            // 累加计时器
            timer += Time.deltaTime;
            // 如果计时器达到时间间隔
            if (timer >= interval)
            {
                // 重置计时器
                timer = 0f;
                // 调用逻辑函数
                Main.Main.UpdatePerSecond(Time.time); 
            }
            //DebugCheckDuplicateDriver();
        }

        private void FixedUpdate()
        {
            Main.Main.FixedUpdate();
        }

        private void LateUpdate()
        {
            Main.Main.LateUpdate();
        }

        private void OnDestroy()
        {
            Main.Main.CloseAllModule();
        }


        void OnApplicationQuit()
        {
            
        }

        [Conditional("DEBUG")]
        private void DebugCheckDuplicateDriver()
        {
            if (LastestUpdateFrame > 0)
            {
                if (LastestUpdateFrame == Time.frameCount)
                    RSJWYLogger.Warning($"There are two {nameof(RSJWYFameworkDriverService)} in the scene. Please ensure there is always exactly one driver in the scene.");
            }

            LastestUpdateFrame = Time.frameCount;
        }
    }
}