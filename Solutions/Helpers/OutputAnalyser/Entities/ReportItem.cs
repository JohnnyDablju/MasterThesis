namespace OutputAnalyser.Entities
{
    class ReportItem
    {
        public int Second { get; set; }
        public int WordCount { get; set; }
        public int MessageCount { get; set; }
        public int AverageLatency { get; set; }
    }
}
