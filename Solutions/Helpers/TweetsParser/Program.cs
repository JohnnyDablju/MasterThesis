using System.Configuration;
using System.Collections.Generic;

namespace TweetsParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputDirectory = ConfigurationManager.AppSettings["inputDirectory"];
            var outputFormat = ConfigurationManager.AppSettings["outputFormat"].Replace("\\t", "\t");
            var outputDirectory = ConfigurationManager.AppSettings["outputDirectory"];
            var outputFileName = ConfigurationManager.AppSettings["outputFileName"];

            var controller = new Controller();
            controller.SplitByInstancesCount(4, inputDirectory, outputFileName, outputDirectory, ".txt");
            //controller.MergeAll(inputDirectory, outputDirectory, outputFileName);
            //controller.Generate(inputDirectory, outputFormat, "16/00", 3);
        }  
    }
}
