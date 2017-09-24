namespace StockTweetJoinOutputAnalyser.Entities
{
    class ReportItem
    {
        public int Second { get; set; }
        public int MessageCount { get; set; }
        public int TweetAverageLatency { get; set; }
        public int StockAverageLatency { get; set; }
        public int AverageLatency { get; set; }
    }
}
