namespace OutputAnalyser.Entities
{
    class Batch
    {
        public long StartTimestamp { get; set; }
        public long EndTimestamp { get; set; }
        public int WordCount { get; set; }
        public int MessageCount { get; set; }
    }
}
