namespace OutputAnalyser.Entities
{
    class Batch
    {
        public long InputTimestamp { get; set; }
        public long OutputTimestamp { get; set; }
        public int WordCount { get; set; }
        public int MessageCount { get; set; }
    }
}
