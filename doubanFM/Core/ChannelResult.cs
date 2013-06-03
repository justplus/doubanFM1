using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace doubanFM.Core
{
    public class Channel
    {
        public string intro { get; set; }
        public string name { get; set; }
        public int song_num { get; set; }
        public Creator creator { get; set; }
        public string banner { get; set; }
        public string cover { get; set; }
        public Int32 id { get; set; }
        public List<string> hot_songs { get; set; }
    }

    public class Creator
    {
        public string url { get; set; }
        public string name { get; set; }
        public Int32 id { get; set; }
    }

    public class Data
    {
        public List<Channel> channels { get; set; }
        public Int32 total { get; set; }
    }

    public class ChannelResult
    {
        public bool status { get; set; }
        public Data data { get; set; }
        public static ChannelResult FromJson(string json)
        {
            try
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ChannelResult));
                using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                    return (ChannelResult)ser.ReadObject(stream);
            }
            catch
            {
                return null;
            }
        }
    }
}
