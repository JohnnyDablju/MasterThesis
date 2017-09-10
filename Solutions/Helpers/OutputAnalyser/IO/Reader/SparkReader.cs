﻿using System;
using System.Collections.Generic;
using OutputAnalyser.Entities;
using System.IO;

namespace OutputAnalyser.IO
{
    class SparkReader : Reader
    {
        public SparkReader(string directory, char separator) : base(directory, separator) { }

        protected override void EnumerateFiles(Action action, long? timestampBoundary)
        {
            foreach (var timestampDirectory in Directory.EnumerateDirectories(directory))
            {
                foreach (var filePath in Directory.EnumerateFiles(timestampDirectory))
                {
                    var fileName = Path.GetFileName(filePath);
                    if (fileName.Contains("part") && !fileName.Contains("crc"))
                    {
                        ProcessFile(action, filePath, timestampBoundary);
                    }
                }
            }
        }
    }
}
