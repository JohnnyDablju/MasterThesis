using System;
using System.Collections.Generic;
using System.IO;
using OutputAnalyser.Entities;

namespace OutputAnalyser.IO
{
    class Writer
    {
        public void Write(List<ReportItem> report, string outputDirectory, string processedOutputFileName, string processedOutputHeader, string processedOutputFormat, string description)
        {
            var fileName = String.Format("{0}_{1}_{2}", processedOutputFileName, DateTime.Now.ToString("MMddHHmm"), description);
            using (var writer = new StreamWriter(Path.Combine(outputDirectory, fileName)))
            {
                writer.WriteLine(processedOutputHeader);
                foreach (var item in report)
                {
                    writer.WriteLine(processedOutputFormat, item.Second, item.MessageCount, item.WordCount, item.AverageLatency);
                }
            }
        }
    }
}
