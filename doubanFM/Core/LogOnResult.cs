using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace doubanFM.Core
{
    public class PlayRecord
    {
        public int banned { get; set; }
        public int liked { get; set; }
        public int played { get; set; }
    }

    public class UserInfo
    {
        public string ck { get; set; }
        public Int32 id { get; set; }
        public string name { get; set; }
        public PlayRecord play_record { get; set; }
        public string url { get; set; }
    }

    public class LogOnResult
    {
        public int r { get; set; }
        public int err_no { get; set; }
        public string err_msg { get; set; }
        public UserInfo user_info { get; set; }
        public static LogOnResult FromJson(string json)
        {
            try
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(LogOnResult));
                using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                    return (LogOnResult)ser.ReadObject(stream);
            }
            catch
            {
                return null;
            }
        }
    }


}
