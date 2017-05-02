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

            var controller = new Controller();
            controller.Generate(inputDirectory, outputFormat, "10/07", 1);
        }  
    }
}
