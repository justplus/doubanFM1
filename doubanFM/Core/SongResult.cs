using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace doubanFM.Core
{
    public class Song
    {
        public string picture { get; set; }
        public string albumtitle { get; set; }
        public string company { get; set; }
        public double rating_avg { get; set; }
        public string public_time { get; set; }
        public string ssid { get; set; }
        public string album { get; set; }
        public string like { get; set; }
        public string artist { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string subtype { get; set; }
        public string length { get; set; }
        public string sid { get; set; }
        public string aid { get; set; }
        public string kbps { get; set; }
    }


    public class SongResult
    {
        public int r { get; set; }
        public List<Song> song { get; set; }
        public SongResult()
        {
            song = new List<Song>();
        }

        public static SongResult FromJson(string json)
        {
            try
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(SongResult));
                using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                    return (SongResult)ser.ReadObject(stream);
            }
            catch
            {
                return null;
            }
        }
    }
}
