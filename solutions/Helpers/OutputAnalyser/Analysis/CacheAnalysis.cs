using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutputAnalyser.Entities;

namespace OutputAnalyser
{
    class CacheAnalysis : Analysis
    {
        public CacheAnalysis(long startTimestamp) : base(startTimestamp)
        {
            countsBuffer = new Dictionary<string, int>();
        }

        private Dictionary<string, int> countsBuffer;

        protected override void PreprocessMessages()
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
