using System;

namespace EidolonTestTask.Analytics
{
    [Serializable]
    public struct AnalyticsEvent
    {
        public string type;
        public string data;
    }
}
