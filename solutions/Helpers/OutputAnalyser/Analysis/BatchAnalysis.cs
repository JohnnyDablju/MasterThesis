namespace OutputAnalyser
{
    class BatchAnalysis : Analysis
    {
        public BatchAnalysis(long startTimestamp) : base(startTimestamp) { }

        protected override void PreprocessMessages()
        {
            messages.ForEach(m => m.ProcessedWordCount = m.TotalWordCount);
        }
    }
}
