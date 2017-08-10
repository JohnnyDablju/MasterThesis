using System;
using System.Collections.Generic;
using OutputAnalyser.Entities;
using System.IO;

namespace OutputAnalyser.IO
{
    class SparkReader : Reader
    {
        public SparkReader(string directory, char separator) : base(directory, separator) { }

        public override void Read()
        {
            foreach (var timestampDirectory in Directory.EnumerateDirectories(directory))
            {
                foreach (var filePath in Directory.EnumerateFiles(timestampDirectory))
                {
                    var fileName = Path.GetFileName(filePath);
                    if (fileName.Contains("part") && !fileName.Contains("crc"))
                    {
                        var threadId = Convert.ToByte(Path.GetFileName(filePath).Substring(5));
                        ProcessFile(filePath, 0, threadId);
                    }
                }
            }
        }
    }
}
