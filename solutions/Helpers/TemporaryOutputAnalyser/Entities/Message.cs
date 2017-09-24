namespace StockTweetJoinOutputAnalyser.Entities
{
    class Message
    {
        public long InputTweetTimestamp { get; set; }
        public long InputStockTimestamp { get; set; }
        public long OutputTimestamp { get; set; }
    }
}
