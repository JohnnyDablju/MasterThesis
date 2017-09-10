using System;
using System.Collections.Generic;
using System.Linq;
using OutputAnalyser.Entities;
using System.Threading.Tasks;
using Common;

namespace OutputAnalyser
{
    class Analysis
    {
        public Analysis(long startTimestamp, int wordsPerMessage)
        {
            this.startTimestamp = startTimestamp;
            this.wordsPerMessage = wordsPerMessage;
            report = new List<ReportItem>();
            countsBuffer = new Dictionary<string, int>();
        }

        private int wordsPerMessage;
        private long startTimestamp;
        private List<Message> messages;
        private List<Batch> batches;
        private List<ReportItem> report;
        private Dictionary<string, int> countsBuffer;

        public void AddSecond(int second, List<Message> messages)
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
            }
            else
            {
                Console.WriteLine("{0}\t\t\tNo messages.", DateTime.Now);
            }
        }

        public List<ReportItem> GetReport()
        {
            return report;
        }

        private void PreprocessMessages()
        {
            if (wordsPerMessage != 1)
            {
                messages = messages
                    .OrderBy(m => m.Word)
                    .ThenBy(m => m.TotalWordCount)
                    .ToList();

                messages[0].ProcessedWordCount = CalculateProcessedWordCount(messages[0]);
                for (var i = 1; i < messages.Count; i++)
                {
                    if (messages[i].Word == messages[i - 1].Word)
                    {
                        messages[i].ProcessedWordCount = messages[i].TotalWordCount - messages[i - 1].TotalWordCount;
                        HandleCountsBuffer(messages[i]);
                    }
                    else
                    {
                        messages[i].ProcessedWordCount = CalculateProcessedWordCount(messages[i]);
                    }
                }
            }
            else
            {
                messages.ForEach(m => m.ProcessedWordCount = 1);
            }
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

        private void HandleCountsBuffer(Message message)
        {
            if (!countsBuffer.ContainsKey(message.Word))
            {
                countsBuffer.Add(message.Word, message.TotalWordCount);
            }
            else
            {
                countsBuffer[message.Word] = message.TotalWordCount;
            }
        }

        private int CalculateProcessedWordCount(Message message)
        {
            var count = 0;
            if (countsBuffer.ContainsKey(message.Word))
            {
                count = message.TotalWordCount - countsBuffer[message.Word];
            }
            else
            {
                count = message.TotalWordCount;
            }
            HandleCountsBuffer(message);
            return count;
        }
    }
}
