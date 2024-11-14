using RSJWYFamework.Runtime.Event;

namespace RSJWYFamework.Runtime.Tips
{
    public class TipsInfoEventArgs:EventArgsBase
    {
        public string tips;
        public override void Release()
        {
            tips = string.Empty;
        }
    }
}