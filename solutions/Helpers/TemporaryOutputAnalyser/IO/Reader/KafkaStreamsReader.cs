using System;
using System.Collections.Generic;
using StockTweetJoinOutputAnalyser.Entities;
using System.IO;

namespace StockTweetJoinOutputAnalyser.IO
{
    class KafkaStreamsReader : Reader
    {
        public KafkaStreamsReader(string directory, char separator) : base(directory, separator) { }

        protected override Message GetMessage(string line)
        {
            line = line.Remove(0, line.IndexOf(",") + 2);
            return base.GetMessage(line);
        }
    }
}
