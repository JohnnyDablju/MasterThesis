using System;
using System.Collections.Generic;
using StockTweetJoinOutputAnalyser.Entities;
using System.IO;

namespace StockTweetJoinOutputAnalyser.IO
{
    class SparkReader : Reader
    {
        public SparkReader(string directory, char separator) : base(directory, separator) { }

        private const string TEMPORARY_DIR = "_temporary/0";

        protected override void EnumerateFiles(Action action, long? timestampBoundary)
        {
            //foreach (var timestampDirectory in Directory.EnumerateDirectories(directory))
            {
                foreach (var taskDirectory in Directory.EnumerateDirectories(Path.Combine(directory, TEMPORARY_DIR)))
                {
                    EnumerateInnerFiles(taskDirectory, action, timestampBoundary);
                }
            }
        }

        private void EnumerateInnerFiles(string directory, Action action, long? timestampBoundary)
        {
            foreach (var filePath in Directory.EnumerateFiles(directory))
            {
                var fileName = Path.GetFileName(filePath);
                if (fileName.Contains("part") && !fileName.Contains("crc"))
                {
                    ProcessFile(action, filePath, timestampBoundary);
                }
            }
        }

        protected override Message GetMessage(string line)
        {
            line = line.Remove(0, line.IndexOf(",") + 1).Replace(")", "");
            return base.GetMessage(line);
        }
    }
}
