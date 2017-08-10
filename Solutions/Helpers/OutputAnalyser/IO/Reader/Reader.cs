using System;
using System.Collections.Generic;
using OutputAnalyser.Entities;
using System.IO;

namespace OutputAnalyser.IO
{
    abstract class Reader
    {
        protected List<Message> messages;
        //protected Dictionary<string, List<int>> dictionary;
        private int counter;
        protected char separator;
        protected string directory;

        public Reader(string directory, char separator)
        {
            messages = new List<Message>();
            //dictionary = new Dictionary<string, List<int>>();
            this.separator = separator;
            this.directory = directory;
            counter = 0;
        }

        public abstract void Read();

        protected void ProcessFile(string filePath, byte nodeId, byte threadId)
        {
            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split(separator);
                    messages.Add(new Message
                    {
                        StartTimestamp = Convert.ToInt64(line[0]),
                        EndTimestamp = Convert.ToInt64(line[1]),
                        Word = line[2],
                        TotalWordCount = Convert.ToInt32(line[3]),
                        NodeId = nodeId,
                        ThreadId = threadId
                    });
                    //AddToDictionary(line[2]);
                }
            }
        }
        
        /*protected void AddToDictionary(string word)
        {
            if (!dictionary.ContainsKey(word))
            {
                dictionary.Add(word, new List<int>());
            }
            dictionary[word].Add(counter++);
        }*/

        public List<Message> GetMessages()
        {
            return messages;
        }

        /*public Dictionary<string, List<int>> GetDictionary()
        {
            return dictionary;
        }*/
    }
}
