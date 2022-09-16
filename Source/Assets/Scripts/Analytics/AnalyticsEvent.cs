using System;

namespace EidolonTestTask.Analytics
{
    [Serializable]
    public struct AnalyticsEvent
    {
        public string type { get; set; }
        public string data { get; set; }
    }
}
