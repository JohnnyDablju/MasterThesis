using System;
using System.Collections.Generic;
using System.Linq;
using OutputAnalyser.Entities;
using Common;

namespace OutputAnalyser
{
    class Analysis
    {
        public Analysis(List<Message> messages)
        {
            var startTimestamp = messages.Min(m => m.StartTimestamp);
            start = startTimestamp.TimestampToDateTime();
            finish = messages.Max(m => m.EndTimestamp).TimestampToDateTime();
            processings = new List<Processing>();
            GroupProcessings(messages, startTimestamp);
            Postprocess();
        }

        private DateTime? start;
        private DateTime? finish;
        private List<Processing> processings;

        private void GroupProcessings(List<Message> messages, long start)
        {
            processings = messages
                .GroupBy(m => new { m.StartTimestamp, m.EndTimestamp }, (key, group) => new Processing
                {
                    Second = Convert.ToInt32(key.StartTimestamp + key.EndTimestamp - 2 * start) / 2000,
                    StartTime = Convert.ToInt32(key.StartTimestamp - start),
                    EndTime = Convert.ToInt32(key.EndTimestamp - start),
                    WordCount = group.Sum(x => x.WordCount),
                    MessageCount = group.Count(),
                    Latency = Convert.ToInt32(key.EndTimestamp - key.StartTimestamp)
                })
                .OrderBy(p => p.StartTime).ThenBy(p => p.EndTime)
                .ToList();
        }

        private void Postprocess()
        {
            processings[0].Time = 0;
            for (var i = 1; i < processings.Count; i++)
            {
                processings[i].Time = processings[i].EndTime - processings[i - 1].EndTime;
            }

        }

        public List<Processing> GetProcessings()
        {
            return processings;
        }
    }
}
