using System;
using System.Collections.Generic;
using OutputAnalyser.Entities;
using System.IO;

namespace OutputAnalyser.IO
{
    class KafkaStreamsReader : Reader
    {
        public KafkaStreamsReader(string directory, char separator) : base(directory, separator) { }

        public override void Read()
        {
            foreach (var filePath in Directory.EnumerateFiles(directory))
            {
                using (var reader = new StreamReader(filePath))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine().Remove(0, 32);
                        var index = line.LastIndexOf('(');
                        var parts = line.Substring(index + 1).Split(separator);
                        var word = line.Remove(index).Replace(" , ", "");
                        messages.Add(new Message
                        {
                            Word = word,
                            StartTimestamp = Convert.ToInt64(parts[0]),
                            EndTimestamp = Convert.ToInt64(parts[1]),
                            TotalWordCount = Convert.ToInt32(parts[2].Remove(parts[2].IndexOf("<-"))),
                            NodeId = 0,
                            ThreadId = 0
                        });
                        //AddToDictionary(word);
                    }
                }
            }
        }
    }
}
