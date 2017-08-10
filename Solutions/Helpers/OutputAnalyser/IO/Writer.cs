using System.Collections.Generic;
using System.IO;
using OutputAnalyser.Entities;

namespace OutputAnalyser.IO
{
    class Writer
    {
        public void Write(List<ReportItem> report, string outputDirectory, string processedOutputFileName, string processedOutputHeader, string processedOutputFormat)
        {
            using (var writer = new StreamWriter(Path.Combine(outputDirectory, processedOutputFileName)))
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
