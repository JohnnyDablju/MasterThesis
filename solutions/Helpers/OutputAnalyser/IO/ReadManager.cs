using System;
using System.Collections.Generic;

namespace OutputAnalyser.IO
{
    class ReadManager
    {
        private const int PROCESSING_TIME_LIMIT = 1000000;

        private Dictionary<int, long> timestampBoundaries;
        private Analysis analysis;
        private Reader reader;

        public ReadManager(Reader reader, Analysis analysis)
        {
            this.reader = reader;
            this.analysis = analysis;
        }

        public void Process(long startTimestamp)
        {
            var noMessagesCounter = 0;
            foreach (var boundary in GetTimestampBoundaries(startTimestamp))
            {
                Console.WriteLine("{0}\t\tGetting messages for second {1}...", DateTime.Now, boundary.Key);
                var messages = reader.GetMessages(boundary.Value);
                noMessagesCounter =
                    analysis.AddSecond(boundary.Key, messages)
                    ? 0
                    : noMessagesCounter + 1;
                if (noMessagesCounter > 59)
                {
                    Console.WriteLine("{0}\t\tNo messages processed for a minute. Stopping...", DateTime.Now);
                    break;
                }
            }
        }

        private Dictionary<int, long> GetTimestampBoundaries(long startTimestamp)
        {
            if (timestampBoundaries == null)
            {
                timestampBoundaries = new Dictionary<int, long>();
                for (var t = startTimestamp + 1; t < startTimestamp + PROCESSING_TIME_LIMIT; t++)
                {
                    var second = Convert.ToInt32((t - startTimestamp) / 1000);
                    if (!timestampBoundaries.ContainsKey(second))
                    {
                        timestampBoundaries.Add(second, t);
                    }
                    timestampBoundaries[second] = t;
                }
            }
            return timestampBoundaries;
        }
    }
}
