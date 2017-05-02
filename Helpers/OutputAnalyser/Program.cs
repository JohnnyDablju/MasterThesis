using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Configuration;

namespace OutputAnalyser
{
    class Program
    {
        static void Main(string[] args)
        {
            var outputDirectory = ConfigurationManager.AppSettings["outputDirectory"];
            var rawOutputFileName = ConfigurationManager.AppSettings["rawOutputFileName"];
            var processedOutputFileName = ConfigurationManager.AppSettings["processedOutputFileName"];
            var rawOutputSeparator = Convert.ToChar(ConfigurationManager.AppSettings["rawOutputSeparator"].Replace("\\t", "\t"));
            var processedOutputHeader = ConfigurationManager.AppSettings["processedOutputHeader"].Replace("\\t", "\t");
            var processedOutputFormat = ConfigurationManager.AppSettings["processedOutputFormat"].Replace("\\t", "\t");

            var ioController = new IOController();
            var messages = ioController.LoadRawMessages(outputDirectory, rawOutputFileName, rawOutputSeparator);
            var analysis = new Analysis(messages);
            var processings = analysis.GetProcessings();
            ioController.SaveProcessedOutput(processings, outputDirectory, processedOutputFileName, processedOutputHeader, processedOutputFormat);
        }
    }
}
