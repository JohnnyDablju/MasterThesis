using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            foreach (var boundary in GetTimestampBoundaries(startTimestamp))
            {
                Console.WriteLine("{0}\t\tGetting messages for second {1}...", DateTime.Now, boundary.Key);
                var messages = reader.GetMessages(boundary.Value);
                analysis.AddSecond(boundary.Key, messages);
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
