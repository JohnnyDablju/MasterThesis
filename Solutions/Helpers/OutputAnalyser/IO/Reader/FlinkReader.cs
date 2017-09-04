using System;
using System.Collections.Generic;
using OutputAnalyser.Entities;
using System.IO;

namespace OutputAnalyser.IO
{
    class FlinkReader : Reader
    {
        public FlinkReader(string directory, char separator) : base(directory, separator) { }

        public override void Read()
        {
            foreach (var filePath in Directory.EnumerateFiles(directory))
            {
                var fileName = Convert.ToInt32(Path.GetFileName(filePath));
                ProcessFile(filePath, Convert.ToByte(fileName/4), Convert.ToByte(fileName%4));
            }
        }
    }
}
