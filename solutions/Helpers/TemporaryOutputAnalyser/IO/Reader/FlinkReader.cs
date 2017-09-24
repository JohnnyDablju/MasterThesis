using System;
using System.Collections.Generic;
using StockTweetJoinOutputAnalyser.Entities;
using System.IO;

namespace StockTweetJoinOutputAnalyser.IO
{
    class FlinkReader : Reader
    {
        public FlinkReader(string directory, char separator) : base(directory, separator) { }
    }
}
