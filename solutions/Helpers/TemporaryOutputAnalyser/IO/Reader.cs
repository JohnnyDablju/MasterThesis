using System;
using System.Collections.Generic;
using StockTweetJoinOutputAnalyser.Entities;
using System.IO;

namespace StockTweetJoinOutputAnalyser.IO
{
    abstract class Reader
    {
        private Dictionary<string, long> positions;

        protected char separator;
        protected string directory;
        protected List<Message> messages;
        protected long startTimestamp;

        protected enum Action { GetMessages, GetStartTimestamp };

        public Reader(string directory, char separator)
        {
            this.directory = directory;
            this.separator = separator;
            positions = new Dictionary<string, long>();
        }

        public long GetStartTimestamp()
        {
            startTimestamp = long.MaxValue;
            EnumerateFiles(Action.GetStartTimestamp, null);
            return startTimestamp;
        }

        public List<Message> GetMessages(long timestampBoundary)
        {
            messages = new List<Message>();
            EnumerateFiles(Action.GetMessages, timestampBoundary);
            return messages;
        }

        protected virtual void EnumerateFiles(Action action, long? timestampBoundary)
        {
            foreach (var filePath in Directory.EnumerateFiles(directory))
            {
                ProcessFile(action, filePath, timestampBoundary);
            }
        }

        protected virtual Message GetMessage(string line)
        {
            var parts = line.Split(separator);
            return new Message
            {
                InputTweetTimestamp = Convert.ToInt64(parts[0]),
                InputStockTimestamp = Convert.ToInt64(parts[1]),
                OutputTimestamp = Convert.ToInt64(parts[2])
            };
        }

        protected void ProcessFile(Action action, string filePath, long? timestampBoundary)
        {
            using (var reader = new StreamReader(filePath))
            {
                switch (action)
                {
                    case Action.GetStartTimestamp:
                        SeekForStartTimestamp(reader);
                        break;
                    case Action.GetMessages:
                        if (!positions.ContainsKey(filePath))
                        {
                            positions.Add(filePath, 0);
                        }
                        var position = LoadMessages(reader, positions[filePath], timestampBoundary);
                        positions[filePath] = position;
                        break;
                }
            }
        }

        private long LoadMessages(StreamReader reader, long? position, long? boundaryTimestamp)
        {
            reader.BaseStream.Position = position.Value;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                try
                {
                    var message = GetMessage(line);
                    if (message.OutputTimestamp <= boundaryTimestamp.Value)
                    {
                        messages.Add(message);
                        // change below when using Linux/Windows
                        position += (line.Length + 1);
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("{0}\t\t\t\tFile corrupted, position {1}, line {2}", DateTime.Now, position, line);
                    continue;
                }
                
            }
            return position.Value;
        }

        private void SeekForStartTimestamp(StreamReader reader)
        {
            var lineCounter = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var message = GetMessage(line);
                var tweetTimestamp = message.InputTweetTimestamp;
                var stockTimestamp = message.InputStockTimestamp;
                var timestamp = tweetTimestamp < stockTimestamp
                    ? tweetTimestamp
                    : stockTimestamp;
                if (timestamp < startTimestamp)
                {
                    startTimestamp = timestamp;
                }
                lineCounter++;
            }
        }
    }
}
