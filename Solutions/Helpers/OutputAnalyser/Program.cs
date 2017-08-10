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
            Console.WriteLine("{0}\tReading messages...", DateTime.Now);
            reader.Read();
            Console.WriteLine("{0}\tProcessing messages...", DateTime.Now);
            var analysis = new Analysis(reader.GetMessages(), frameworkName == "Flink");
            Console.WriteLine("{0}\t\tGenerating batches...", DateTime.Now);
            analysis.GetBatches();
            Console.WriteLine("{0}\t\tGenerating report...", DateTime.Now);
            var report = analysis.GetReport();
            Console.WriteLine("{0}\tWriting report...", DateTime.Now);
            new Writer().Write(report, analysisDirectory, frameworkName, reportHeader, reportFormat);
        }
    }
}
