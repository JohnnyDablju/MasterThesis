using System;
using System.Collections.Generic;
using OutputAnalyser.Entities;
using System.IO;

namespace OutputAnalyser.IO
{
    class FlinkReader : Reader
    {
        public FlinkReader(string directory, char separator) : base(directory, separator) { }
    }
}
