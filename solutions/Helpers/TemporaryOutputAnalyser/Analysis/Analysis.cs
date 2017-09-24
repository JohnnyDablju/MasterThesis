using System;
using System.Collections.Generic;
using System.Linq;
using StockTweetJoinOutputAnalyser.Entities;

namespace StockTweetJoinOutputAnalyser
{
    class Analysis
    {
        public Analysis(long startTimestamp)
        {
            this.startTimestamp = startTimestamp;
            report = new List<ReportItem>();
        }

        private long startTimestamp;
        protected List<Message> messages;
        private List<ReportItem> report;

        public bool AddSecond(int second, List<Message> messages)
        {
            if (messages.Count > 0)
            {
                this.messages = messages;
                Console.WriteLine("{0}\t\t\tAdding to report...", DateTime.Now);
                AddToReport(second);
                return true;
            }
            else
            {
                Console.WriteLine("{0}\t\t\tNo messages.", DateTime.Now);
                return false;
            }
        }

        public List<ReportItem> GetReport()
        {
            return report;
        }

        private void AddToReport(int second)
        {
            report.Add(
                new ReportItem
                {
                    Second = second,
                    MessageCount = messages.Count,
                    StockAverageLatency = Convert.ToInt32(messages.Average(m => m.OutputTimestamp - m.InputStockTimestamp)),
                    TweetAverageLatency = Convert.ToInt32(messages.Average(m => m.OutputTimestamp - m.InputTweetTimestamp)),
                    AverageLatency = Convert.ToInt32(messages.Average(m => m.OutputTimestamp - (m.InputStockTimestamp + m.InputTweetTimestamp)/2))
                }
            );
        }
    }
}
