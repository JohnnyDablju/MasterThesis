using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Configuration;
using OutputAnalyser.IO;

namespace OutputAnalyser
{
    class Program
    {
        static void Main(string[] args)
        {
            var reportHeader = ConfigurationManager.AppSettings["reportHeader"].Replace("\\t", "\t");
            var reportFormat = ConfigurationManager.AppSettings["reportFormat"].Replace("\\t", "\t");

            var outputSeparator = Convert.ToChar(ConfigurationManager.AppSettings["outputSeparator"].Replace("\\t", "\t"));
            var experimentDirectory = ConfigurationManager.AppSettings["experimentDirectory"];
            var analysisDirectory = Path.Combine(experimentDirectory, ConfigurationManager.AppSettings["analysisFolder"]);

            var frameworkName = ConfigurationManager.AppSettings["frameworkName"];
            var frameworkDirectory = Path.Combine(experimentDirectory, frameworkName);

            Reader reader = null; 
            switch (frameworkName)
            {
                case "KafkaStreams": reader = new KafkaStreamsReader(frameworkDirectory, outputSeparator); break;
                case "Spark": reader = new SparkReader(frameworkDirectory, outputSeparator); break;
                case "Flink": reader = new FlinkReader(frameworkDirectory, outputSeparator); break;
            }
            Console.WriteLine("Enter description:");
            var description = Console.ReadLine();
            Console.WriteLine("Words per message?");
            var wordsPerMessage = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("{0}\tGetting processing start timestamp...", DateTime.Now);
            var startTimestamp = reader.GetStartTimestamp();
            Console.WriteLine("{0}\tStarting processing...", DateTime.Now);
            var analysis = new Analysis(startTimestamp, wordsPerMessage);
            var readManager = new ReadManager(reader, analysis);
            readManager.Process(startTimestamp);
            Console.WriteLine("{0}\tWriting report...", DateTime.Now);
            var report = analysis.GetReport();
            new Writer().Write(report, analysisDirectory, frameworkName, reportHeader, reportFormat, description);
        }
    }
}
