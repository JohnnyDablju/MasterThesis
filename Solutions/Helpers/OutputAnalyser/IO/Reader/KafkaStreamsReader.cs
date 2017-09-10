using System;
using System.Collections.Generic;
using OutputAnalyser.Entities;
using System.IO;

namespace OutputAnalyser.IO
{
    class KafkaStreamsReader : Reader
    {
        public KafkaStreamsReader(string directory, char separator) : base(directory, separator) { }

        protected override Message GetMessage(string line)
        {
            line = line.Remove(0, 31);
            var index = line.LastIndexOf('(');
            var parts = line.Substring(index + 1).Split(separator);
            var word = line.Remove(index).Replace(" , ", "");
            return new Message
            {
                InputTimestamp = Convert.ToInt64(parts[0]),
                OutputTimestamp = Convert.ToInt64(parts[1]),
                Word = word,
                TotalWordCount = Convert.ToInt32(parts[2].Remove(parts[2].IndexOf("<-"))),
            };
        }
    }
}
