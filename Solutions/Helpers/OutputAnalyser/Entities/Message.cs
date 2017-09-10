namespace OutputAnalyser.Entities
{
    class Message
    {
        public long InputTimestamp { get; set; }
        public long OutputTimestamp { get; set; }
        public string Word { get; set; }
        public int TotalWordCount { get; set; }
        public int? ProcessedWordCount { get; set; }
    }
}
