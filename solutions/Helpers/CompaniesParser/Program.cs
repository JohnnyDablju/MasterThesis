using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace CompaniesParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var filteredWords = new List<string> { "inc", "group", "corporation", "corp", "fund", "the", "trust", "investments", "incorporated", "plc", "ltd", "llc", "lp", "limited", "", "etf", "bancorp", "holding", "holdings", "bancshares", "bankshares", "acquisition", "acquisitions", "company", "companies", "partners", "income", "equity", "co" };
            var restrictedIndexes = new List<string> {
                "hbcp", "fbnc", "homb", "tel", "fbp", "good", "loco", "edu", "usb", "usb^a", "usb^h", "usb^m", "usb^o", "ten", "cia", "tru", "tho", "turn", "too", "eat", "gold", "dea", "hall", "roll", "baby", "save", "team", "sand", "two", "rose", "stay", "play", "cop", "win", "wood", "golf", "era", "hi", "do", "wat", "via", "co", "tell", "main", "next", "gain", "an", "flow", "mark", "salt",
                "rt", "for", "it", "on", "at", "by", "if", "so", "he", "am", "all", "re", "or", "ll", "are", "out", "more", "time", "hope", "news", "life", "wow", "has", "fun", "star", "else", "see", "man", "fam", "grow", "cry", "nice", "bro", "big", "hear", "TRUE", "any", "live", "safe", "web", "old", "men", "fit", "meet", "tech", "mind", "fast", "cool", "cc", "job", "post", "link", "plus",
                "call", "fast", "cool", "blue", "sir", "fly", "ring", "rare", "glad", "jobs", "shy", "fix", "key",
                "hunt", "once", "self", "race", "beat", "wash", "car", "tear", "met", "club", "min", "eyes", "np", "twin", "loan", "el", "cats", "ko", "ice",
                "rock", "tv", "mini", "ai", "cost", "rate", "home", "pay", "site", "type", "low", "cars", "born", "town", "elf", "door", "cat", "jack", "apps", "ago", "shop", "son", "five", "bit", "rail", "cubs", "emo", "ego", "pro", "sky", "flic", "earn", "brew", "ty", "trip", "joe", "grid", "calm", "dish", "tax", "nap", "lake", "dare", "cent", "box", "run", "tree", "nick", "pool", "lens", "neon", "fold", "del", "sync", "six", "seed", "pi", "lone", "cake", "cart", "chef", "rice", "pie", "fund", "fork", "land", "slim", "wifi", "wire", "pot", "www", "flat", "glow", "ears", "ash", "plug", "tile", "mod", "gram", "boom", "cuba", "data", "pen", "spa", "pets", "tops", "tap", "wins", "ship", "sons", "jazz", "farm", "flag", "echo", "edge", "cube", "bio", "bold", "ally", "camp", "doc", "pub", "sum", "fate", "fax", "laws", "leg", "info", "fuel", "cash", "edit", "vet", "suns", "yoga", "wage", "rick", "quad", "lion",
                "tour", "usa", "core", "keys", "wing", "pump", "fits", "soda", "lite", "oak", "cyan", "pm", "cuz",
                "airt", "ft", "ln", "chco", "amp", "iroq", "dnow", "gt", "nwsa", "nws", "tisi", "snbc", "tsg", "now", "unt", "fbms", "bll", "af", "wstc",
                // having one word with meaning as a name
                "ccf", "gv", "seb", "avhi", "alog", "ancb", "awre", "bcom", "bctf", "banr", "bybk", "bncl", "bdge", "ca", "cnbka", "cizn", "jva", "cohr", "cbsh", "cbshp", "ctbi", "icbk", "dtrm", "diod", "egbn", "eacq", "eacqu", "eacqw", "eml", "ebtc", "expo", "flex", "fh", "fklyu", "gbci", "grvy", "gnbc", "hlit", "ha", "hiho", "hmta", "hubg", "iclr", "incr", "iboc", "intx", "isbc", "mays", "lark", "mbtf", "macq", "macqu", "macqw", "miii", "miiiu", "miiiw", "mhld", "mtch", "labl", "nksh", "nhld", "nhldw", "ndls", "nbn", "ntrs", "ntrsp", "nwbi", "pfin", "pebo", "bpop", "bpopm", "bpopn", "pinc", "rbcaa", "slct", "shpg", "shbi", "bsrr", "sonc", "sbsi", "oksb", "spls", "sbcp", "sgry", "tbnk", "tbbk", "jynt", "mdco", "navg", "org", "uscr", "ubsh", "unb", "ubcp", "uboh", "ubsi", "unty", "cj", "bxmt", "ccl", "cuk", "chn", "coh", "cr", "cck", "cub", "cubi", "cubi^c", "cubi^d", "cubi^e", "cubi^f", "dlx", "eig", "eqc", "eqc^d", "eqco", "eqr", "eea", "expr", "fpi", "fpi^b", "flr", "gps", "gpi", "ges", "ihc", "ifn", "itgr", "lb", "lfgr", "mh^a", "mh^c", "mh^d", "mhla", "mhnc", "mxf", "msl", "mos", "nvgs", "ne", "nwe", "pagp", "pgr", "pb", "sfs", "soja", "sojb", "sr", "sq", "splp", "splp^a", "tgt", "trv", "xl", "xoxo"
            }; // and all one letter
            var restrictedFirstNames = new List<string> { "first" };

            using (var streamWriter = new StreamWriter(@"C:\Git\MasterThesis\deployment\data\companies"))
            {
                foreach (var filePath in Directory.EnumerateFiles(@"C:\Git\MasterThesis\data\stocks"))
                {
                    using (var parser = new TextFieldParser(filePath))
                    {
                        parser.HasFieldsEnclosedInQuotes = true;
                        parser.TextFieldType = FieldType.Delimited;
                        parser.TrimWhiteSpace = true;
                        parser.SetDelimiters(",");
                        parser.ReadLine();

                        while (!parser.EndOfData)
                        {
                            var fields = parser.ReadFields();

                            // check if index is not restricted
                            var index = fields[0].ToLower().Trim();
                            if (restrictedIndexes.Contains(index) || index.Length == 1)
                            {
                                continue;
                            }

                            // check if first name is not restricted
                            var originalName = fields[1].ToLower()
                                .Replace("(", "").Replace(")", "").Replace(".", "").Replace(",", "")
                                .Replace("&#39;", "'")
                                .Split(' ');
                            if (restrictedFirstNames.Contains(originalName[0].Trim()))
                            {
                                continue;
                            }

                            // assign price if not available
                            var price = fields[2];
                            price = price == "n/a"
                                ? "0.5"
                                : price;

                            // filter words
                            var fullname = new List<string>();
                            foreach (var word in originalName)
                            {
                                var trimmedWord = word.Trim();
                                if (!String.IsNullOrWhiteSpace(trimmedWord) && trimmedWord.Length > 1 && !filteredWords.Contains(trimmedWord))
                                {
                                    fullname.Add(trimmedWord);
                                }
                            }

                            // generate phrases
                            var phrases = new List<string>();
                            phrases.Add(index);
                            phrases.Add(String.Join(" ", fullname));

                            // write to file
                            streamWriter.WriteLine(String.Format("{0}\t{1}\t{2}", index, String.Join(",", phrases), price));
                        }
                    }
                }
            }
        }
    }
}
