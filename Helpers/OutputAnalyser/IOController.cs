using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OutputAnalyser.Entities;

namespace OutputAnalyser
{
    class IOController
    {
        public List<Message> LoadRawMessages(string outputDirectory, string rawOutputFileName, char rawOutputSeparator)
        {
            var list = new List<Message>();
            using (var reader = new StreamReader(Path.Combine(outputDirectory, rawOutputFileName)))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split();
                    list.Add(new Message
                    {
                        StartTimestamp = Convert.ToInt64(line[0]),
                        EndTimestamp = Convert.ToInt64(line[1]),
                        WordCount = Convert.ToInt32(line[2])
                    });
                }
            }
            return list;
        }

        public void SaveProcessedOutput(List<Processing> processings, string outputDirectory, string processedOutputFileName, string processedOutputHeader, string processedOutputFormat)
        {
            using (var writer = new StreamWriter(Path.Combine(outputDirectory, processedOutputFileName)))
            {
                writer.WriteLine(processedOutputHeader);
                foreach (var processing in processings)
                {
                    writer.WriteLine(processedOutputFormat, processing.Second, processing.StartTime, processing.EndTime, processing.Latency, processing.Time, processing.WordCount, processing.MessageCount);
                }
            }
        }
    }
}
