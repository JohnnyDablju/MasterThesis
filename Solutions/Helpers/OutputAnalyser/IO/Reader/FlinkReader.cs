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
                var threadId = Convert.ToByte(Path.GetFileName(filePath));
                ProcessFile(filePath, 0, threadId);
            }
        }
    }
}
