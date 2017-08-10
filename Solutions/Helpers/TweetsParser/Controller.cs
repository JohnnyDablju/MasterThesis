using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;
using Newtonsoft.Json;
using System.Diagnostics;


namespace TweetsParser
{
    class Controller
    {
        public void MergeAll(string inputDirectory, string outputDirectory, string outputFileName)
        {
            using (var outputStream = File.Create(Path.Combine(outputDirectory, outputFileName)))
            {
                foreach (var dayDirectory in Directory.EnumerateDirectories(inputDirectory))
                {
                    foreach (var hourDirectory in Directory.EnumerateDirectories(dayDirectory))
                    {
                        using (var inputStream = File.OpenRead(Path.Combine(hourDirectory, String.Format("{0}_{1}.txt", dayDirectory.Substring(dayDirectory.LastIndexOf('\\') + 1), hourDirectory.Substring(hourDirectory.LastIndexOf('\\') + 1)))))
                        {
                            inputStream.CopyTo(outputStream);
                        }
                    }
                }
            }
        }

        public void SplitByInstancesCount(int instancesCount, string inputDirectory, string inputFileName, string outputDirectory, string outputFileNamePattern)
        {
            var inputPath = Path.Combine(inputDirectory, inputFileName);
            var linesCount = File.ReadLines(inputPath).Count();
            var linesPerFileCount = linesCount / instancesCount;
            using (var inputStream = new StreamReader(inputPath))
            {
                for (var i = 0; i < instancesCount; i++)
                {
                    var prefix = i.ToString("00");
                    var outputFileName = String.Format("{0}_{1}", prefix, outputFileNamePattern);
                    using (var outputStream = new StreamWriter(Path.Combine(outputDirectory, outputFileName)))
                    {
                        for (var l = 0; l < linesPerFileCount && !inputStream.EndOfStream; l++)
                        {
                            var line = inputStream.ReadLine().Split('\t');
                            outputStream.WriteLine(line[2]);
                        }
                    }
                }
            }
        }

        public void MergeByHour(string inputDirectory)
        {
            for (int i = 0; i <= 23; i++)
            {
                var hour = i.ToString("00");
                using (var outputStream = File.Create(Path.Combine(inputDirectory, String.Format("{0}.txt", hour))))
                {
                    foreach (var dayDirectory in Directory.EnumerateDirectories(inputDirectory))
                    {
                        var hourDirectory = Path.Combine(dayDirectory, hour);
                        if (Directory.Exists(hourDirectory))
                        {
                            using (var inputStream = File.OpenRead(Path.Combine(hourDirectory, String.Format("{0}_{1}.txt", dayDirectory.Substring(dayDirectory.LastIndexOf('\\') + 1), hour))))
                            {
                                inputStream.CopyTo(outputStream);
                            }
                        }
                    }
                }
            }
        }

        //08/12
        public void Generate(string inputDirectory, string outputFormat, string startWith, int threadsCount)
        {
            var totalTweetsCount = 0;
            foreach (var dayDirectory in Directory.EnumerateDirectories(inputDirectory))
            {
                Parallel.ForEach(Directory.EnumerateDirectories(dayDirectory), new ParallelOptions { MaxDegreeOfParallelism = threadsCount }, hourDirectory =>
                {
                    if (Convert.ToInt32(dayDirectory.Substring(dayDirectory.Length - 2)) > Convert.ToInt32(startWith.Substring(0, 2)) 
                        || (Convert.ToInt32(dayDirectory.Substring(dayDirectory.Length - 2)) == Convert.ToInt32(startWith.Substring(0, 2)) && Convert.ToInt32(hourDirectory.Substring(hourDirectory.Length - 2)) >= Convert.ToInt32(startWith.Substring(3, 2))))
                    {
                        var sw = new Stopwatch();
                        sw.Start();
                        var outputFilePath = Path.Combine(hourDirectory, String.Format("{0}_{1}.txt", dayDirectory.Substring(dayDirectory.LastIndexOf('\\') + 1), hourDirectory.Substring(hourDirectory.LastIndexOf('\\') + 1)));
                        Console.WriteLine("Creation of {0} has started.", outputFilePath);
                        var tweets = new List<Tweet>();
                        foreach (var minuteFile in Directory.EnumerateFiles(hourDirectory, "*.bz2"))
                        {
                            using (var sourceStream = new FileStream(minuteFile, FileMode.Open, FileAccess.Read))
                            {
                                using (var unzippedStream = new BZip2InputStream(sourceStream))
                                {
                                    using (var streamReader = new StreamReader(unzippedStream))
                                    {
                                        while (!streamReader.EndOfStream)
                                        {
                                            var line = streamReader.ReadLine();
                                            var tweet = JsonConvert.DeserializeObject<Json.Tweet>(line);
                                            if (tweet != null && tweet.lang == "en")
                                            {
                                                //var ca = tweet.created_at;
                                                //var isoCreatedAt = String.Format("{0}-{1}-{2}T{3}", ca.Substring(26), "10", ca.Substring(8, 2), ca.Substring(11, 8));
                                                tweets.Add(new Tweet
                                                {
                                                    Id = tweet.id_str,
                                                    Timestamp = tweet.timestamp_ms,
                                                    Text = tweet.text.Replace("\n", " ").Replace("\r", " ").Replace("\t", " ")
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        using (var outputWriter = new StreamWriter(outputFilePath))
                        {
                            foreach (var tweet in tweets)
                            {
                                outputWriter.WriteLine(outputFormat, tweet.Id, tweet.Timestamp, tweet.Text);
                            }
                            totalTweetsCount += tweets.Count;
                        }
                        sw.Stop();
                        Console.WriteLine("\nCreating {0} took {1} seconds.\n{2} tweets generated.\n{3} tweets in total.\n", outputFilePath, sw.ElapsedMilliseconds / 1000, tweets.Count, totalTweetsCount);
                    }
                });
            }
        }
    }
}
