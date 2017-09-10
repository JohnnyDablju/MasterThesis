namespace OutputAnalyser.Entities
{
    class Message
    {
        public long StartTimestamp { get; set; }
        public long EndTimestamp { get; set; }
        public string Word { get; set; }
        public int TotalWordCount { get; set; }
        public int? ProcessedWordCount { get; set; }

        //public byte NodeId { get; set; }
        //public byte ThreadId { get; set; }
    }
}
