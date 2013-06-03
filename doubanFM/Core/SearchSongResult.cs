using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace doubanFM.Core
{
    public class SimpleSong
    {
        public string artist { get; set; }
        public Int32 id { get; set; }
        public bool is_deleted { get; set; }
        public string name { get; set; }
        public Source source { get; set; }
        public string url { get; set; }
    }

    public class Source
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class SongData
    {
        public List<SimpleSong> songs { get; set; }
        public Int32 total { get; set; }
    }

    public class SearchSongResult
    {
        public bool status { get; set; }
        public SongData data { get; set; }
        public static SearchSongResult FromJson(string json)
        {
            try
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(SearchSongResult));
                using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                    return (SearchSongResult)ser.ReadObject(stream);
            }
            catch
            {
                return null;
            }
        }
    }
}

