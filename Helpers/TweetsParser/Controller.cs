using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;
using Newtonsoft.Json;
using System.Diagnostics;


namespace TweetsParser
{
    class Controller
    {
        private string tweetsPath;

        public Controller()
        {
            tweetsPath = ConfigurationManager.AppSettings["tweetsPath"].ToString();
        }

        public void MergeTweets()
        {
            for (int i = 0; i <= 23; i++)
            {
                var hour = i.ToString("00");
                using (var outputStream = File.Create(Path.Combine(tweetsPath, String.Format("{0}.txt", hour))))
                {
                    foreach (var dayDirectory in Directory.EnumerateDirectories(tweetsPath))
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

        public void GenerateTweets()
        {
            var totalTweetsCount = 0;
            foreach (var dayDirectory in Directory.EnumerateDirectories(tweetsPath))
            {
                Parallel.ForEach(Directory.EnumerateDirectories(dayDirectory), hourDirectory =>
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    var outputFilePath = Path.Combine(hourDirectory, String.Format("{0}_{1}.txt", dayDirectory.Substring(dayDirectory.LastIndexOf('\\') + 1), hourDirectory.Substring(hourDirectory.LastIndexOf('\\') + 1)));
                    Console.WriteLine("Creation of {0} has started.", outputFilePath);
                    var tweets = new List<Tuple<string, string>>();
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
                                            var ca = tweet.created_at;
                                            var isoCreatedAt = String.Format("{0}-{1}-{2}T{3}", ca.Substring(26), "10", ca.Substring(8, 2), ca.Substring(11, 8));
                                            tweets.Add(new Tuple<string, string>(isoCreatedAt, tweet.text));
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
                            outputWriter.WriteLine("{0}\t{1}", tweet.Item1, tweet.Item2);
                        }
                        totalTweetsCount += tweets.Count;
                    }
                    sw.Stop();
                    Console.WriteLine("\nCreating {0} took {1} seconds.\n{2} tweets generated.\n{3} tweets in total.\n", outputFilePath, sw.ElapsedMilliseconds / 1000, tweets.Count, totalTweetsCount);
                });
            }
        }
    }
}
