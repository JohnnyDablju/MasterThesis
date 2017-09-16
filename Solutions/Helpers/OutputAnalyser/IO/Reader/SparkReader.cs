using System;
using System.Collections.Generic;
using OutputAnalyser.Entities;
using System.IO;

namespace OutputAnalyser.IO
{
    class SparkReader : Reader
    {
        public SparkReader(string directory, char separator) : base(directory, separator) { }

        private const string TEMPORARY_DIR = "_temporary/0";

        protected override void EnumerateFiles(Action action, long? timestampBoundary)
        {
            /*foreach (var timestampDirectory in Directory.EnumerateDirectories(directory))
            {
                EnumerateInnerFiles(timestampDirectory, action, timestampBoundary);
                if (Directory.Exists(timestampDirectory + TEMPORARY_PATH))
                {*/
                    foreach (var taskDirectory in Directory.EnumerateDirectories(Path.Combine(directory, TEMPORARY_DIR)))
                    {
                        EnumerateInnerFiles(taskDirectory, action, timestampBoundary);
                    }
                /*}
            }*/
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
    }
}
