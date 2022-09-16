using System;
using System.Collections.Generic;

namespace EidolonTestTask.Analytics
{
    [Serializable]
    public struct AnalyticsData
    {
        public Queue<AnalyticsEvent> events { get; set; }

        public AnalyticsData(Queue<AnalyticsEvent> queuedEvenets)
        {
            events = queuedEvenets;
        }
    }
}
