using RSJWYFamework.Runtime.Event;

namespace RSJWYFamework.Runtime.Default.EventsLibrary
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