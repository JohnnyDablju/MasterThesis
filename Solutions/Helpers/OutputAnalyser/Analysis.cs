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
        public Analysis(List<Message> messages, bool wordPerMessage)
        {
            this.messages = messages;
            //this.dictionary = dictionary;
            this.DetermineProcessedWordCount(wordPerMessage);
            batches = null;
            report = null;
        }

        //private Dictionary<string, List<int>> dictionary;
        private List<Message> messages;
        private List<Batch> batches;
        private List<ReportItem> report;

        private void DetermineProcessedWordCount(bool wordPerMessage)
        {
            /*Parallel.ForEach(messages, message =>
            {
                var maxWordCount = 0;
                dictionary[message.Word].ForEach(i =>
                {
                    var wordCount = messages[i].TotalWordCount;
                    if (wordCount < message.TotalWordCount && wordCount > maxWordCount)
                    {
                        maxWordCount = wordCount;
                    }
                });
                message.ProcessedWordCount = maxWordCount != 0
                    ? message.TotalWordCount - maxWordCount
                    : message.TotalWordCount;

            });*/
            if (wordPerMessage)
            {
                foreach (var message in messages)
                {
                    message.ProcessedWordCount = 1;
                }
            }
            else
            {
                messages = messages
                .OrderBy(m => m.Word)
                .ThenBy(m => m.TotalWordCount)
                .ToList();
                messages[0].ProcessedWordCount = messages[0].TotalWordCount;

                for (var i = 1; i < messages.Count; i++)
                {
                    /*if (messages[i - 1].EndTimestamp > messages[i].EndTimestamp)
                    {
                        throw new Exception("Possible anomaly found");
                    }*/
                    if (messages[i].Word == messages[i - 1].Word)
                    {
                        messages[i].ProcessedWordCount = messages[i].TotalWordCount - messages[i - 1].TotalWordCount;
                    }
                    else
                    {
                        messages[i].ProcessedWordCount = messages[i].TotalWordCount;
                    }
                }
            }
        }

        private void CompileBatches()
        {
            batches = messages
                .GroupBy(m => new { m.StartTimestamp, m.EndTimestamp }, (key, messageGroup) => new Batch
                {
                    StartTimestamp = key.StartTimestamp,
                    EndTimestamp = key.EndTimestamp,
                    WordCount = messageGroup.Sum(m => m.ProcessedWordCount.Value),
                    MessageCount = messageGroup.Count()
                })
                .ToList();
        }

        private void CompileReport()
        {
            var startTimestamp = batches.Min(b => b.StartTimestamp);
            report = batches
                .GroupBy(b => (b.EndTimestamp - startTimestamp) / 1000, (second, batchGroup) => new ReportItem
                {
                    Second = Convert.ToInt32(second),
                    MessageCount = batchGroup.Sum(b => b.MessageCount),
                    WordCount = batchGroup.Sum(b => b.WordCount),
                    AverageLatency = Convert.ToInt32(batchGroup.Average(b => b.EndTimestamp - b.StartTimestamp))
                })
                .OrderBy(r => r.Second)
                .ToList();
        }

        public List<Batch> GetBatches()
        {
            if (batches == null)
            {
                CompileBatches();
            }
            return batches;
        }

        public List<ReportItem> GetReport()
        {
            GetBatches();
            if (report == null)
            {
                CompileReport();
            }
            return report;
        }
    }
}
