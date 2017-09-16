using System;
using System.Collections.Generic;
using System.Linq;
using OutputAnalyser.Entities;

namespace OutputAnalyser
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
        private List<Batch> batches;
        private List<ReportItem> report;

        public bool AddSecond(int second, List<Message> messages)
        {
            if (messages.Count > 0)
            {
                this.messages = messages;
                Console.WriteLine("{0}\t\t\tPreprocessing messages...", DateTime.Now);
                PreprocessMessages();
                Console.WriteLine("{0}\t\t\tCompiling batches...", DateTime.Now);
                CompileBatches();
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

        protected virtual void PreprocessMessages()
        {
            messages.ForEach(m => m.ProcessedWordCount = 1);
        }

        private void CompileBatches()
        {
            batches = messages
                .GroupBy(m => new { m.InputTimestamp, m.OutputTimestamp }, (key, messageGroup) => new Batch
                {
                    InputTimestamp = key.InputTimestamp,
                    OutputTimestamp = key.OutputTimestamp,
                    WordCount = messageGroup.Sum(m => m.ProcessedWordCount.Value),
                    MessageCount = messageGroup.Count()
                })
                .ToList();
        }

        private void AddToReport(int second)
        {
            report.Add(
                new ReportItem
                {
                    Second = second,
                    MessageCount = batches.Sum(b => b.MessageCount),
                    WordCount = batches.Sum(b => b.WordCount),
                    AverageLatency = Convert.ToInt32
                    (
                        batches.Sum(b => (b.OutputTimestamp - b.InputTimestamp) * b.MessageCount)
                        / 
                        batches.Sum(b => b.MessageCount)
                    )
                }
            );
        }
    }
}
