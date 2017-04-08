using System.Collections.Generic;

namespace TweetsParser.Json
{
    public class Hashtag
    {
        public string text { get; set; }
        public List<int> indices { get; set; }
    }
}
