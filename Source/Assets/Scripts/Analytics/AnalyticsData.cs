using System;
using System.Collections.Generic;

namespace EidolonTestTask.Analytics
{
    [Serializable]
    public struct AnalyticsData
    {
        public List<AnalyticsEvent> events { get; set; }
    }
}
