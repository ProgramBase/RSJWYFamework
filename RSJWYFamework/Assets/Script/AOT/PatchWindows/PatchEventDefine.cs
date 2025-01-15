using RSJWYFamework.Runtime.Event;
using RSJWYFamework.Runtime.Main;

namespace Script.AOT.PatchWindows
{
    public class PatchEventDefine
    {
        /// <summary>
        /// 更新进度
        /// </summary>
        public record UpdateProgressEvent : RecordEventArgsBase
        {
            public float progress;
            public string tips;
            
            public static void SendEventMessage(float progress, string tips)
            {
                var msg = new UpdateProgressEvent()
                {
                    progress = progress,
                    tips = tips
                };
                Main.EventModle.Fire(msg);
            }
        }
        /// <summary>
        /// 显示消息盒子
        /// </summary>
        public record ShowMessageEvent : RecordEventArgsBase
        {
            public string tips;
            
            public static void SendEventMessage(float progress, string tips)
            {
                var msg = new ShowMessageEvent()
                {
                    tips = tips
                };
                Main.EventModle.Fire(msg);
            }
        }
    }
}