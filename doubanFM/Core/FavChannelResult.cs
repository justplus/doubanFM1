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
    /*[DataContract]
    public class FavChannel:Channel
    {
        [DataMember]
        public Int32 id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string picture { get; set; }

        public FavChannel(Int32 id, string name, string picture)
        {
            this.id = id;
            this.name = name;
            this.picture = picture;
        }
    }*/

    [DataContract]
    public class FavChannelList
    {
        [DataMember]
        public string username { get; set; }
        [DataMember]
        public List<Channel> channels { get; set; }

        public FavChannelList(string username, Int32 id, string name, string picture)
        {
            this.username = username;
            this.channels = new List<Channel>();
            this.channels.Add(new Channel() { id = id, name = name, banner = picture });
        }
    }

    [DataContract]
    public class FavChannelResult
    {
        [DataMember]
        public List<FavChannelList> favchannels { get; set; }

        public FavChannelResult()
        {
            favchannels = new List<FavChannelList>();
        }

        public List<Channel> GetChannels(string username)
        {
            List<Channel> cs = new List<Channel>();
            foreach (FavChannelList fc in favchannels)
            {
                if (fc.username == username)
                {
                    cs = fc.channels;
                    break;
                }
            }
            return cs;
        }

        public bool isFaved(string username,Int32 id)
        {
            List<Channel> channels = GetChannels(username);
            foreach (Channel c in channels)
            {
                if(c.id==id)
                return true;
            }
            return false;
        }

        public void Add(string username, Int32 id, string name, string picture)
        {
            foreach (FavChannelList fc in favchannels)
            {
                if (fc.username == username)
                {
                    foreach (Channel c in fc.channels)
                    {
                        if (c.name == name)
                            return;
                    }
                    fc.channels.Add(new Channel() { id = id, name = name, banner = picture });
                    return;
                }
            }
            favchannels.Add(new FavChannelList(username, id, name, picture));
        }

        public void Insert(string username, Int32 id, string name, string picture)
        {
            foreach (FavChannelList fc in favchannels)
            {
                if (fc.username == username)
                {
                    foreach (Channel c in fc.channels)
                    {
                        if (c.name == name)
                            return;
                    }
                    fc.channels.Insert(0, new Channel() { id = id, name = name, banner = picture });
                    return;
                }
            }
            favchannels.Insert(0, new FavChannelList(username, id, name, picture));
        }


        public void Delete(string username, Int32 id)
        {
            foreach (FavChannelList fcl in favchannels)
            {
                if (fcl.username == username)
                {
                    foreach (Channel fc in fcl.channels)
                    {
                        if (fc.id == id)
                        {
                            fcl.channels.Remove(fc);
                            break;
                        }
                    }
                    break;
                }
            }
        }

        /*public void WriteJson(string json)
        {
            StreamWriter sw = new StreamWriter("favChannel.ini",false);
            sw.Write(json);
            sw.Flush();
        }*/

        public void ToJson()
        {
            try
            {
                if (!File.Exists("favChannel.ini"))
                {
                    File.Create("favChannel.ini");
                }
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(FavChannelResult));
                using (MemoryStream stream = new MemoryStream())
                {
                    ser.WriteObject(stream, this);
                    string json = Encoding.UTF8.GetString(stream.ToArray());
                    using (StreamWriter sw = new StreamWriter("favChannel.ini", false))
                    {
                        sw.Write(json);
                        //sw.Flush();
                    }
                }
            }
            catch
            {
                return;
            }
        }

        public static FavChannelResult FromJson()
        {
            try
            {
                string json = string.Empty;
                if (!File.Exists("favChannel.ini"))
                {
                    File.Create("favChannel.ini");
                    return new FavChannelResult();
                }
                using (StreamReader sr = new StreamReader("favChannel.ini"))
                {
                    json = sr.ReadLine();
                }
                if (string.IsNullOrEmpty(json))
                    return new FavChannelResult();
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(FavChannelResult));
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                    return (FavChannelResult)ser.ReadObject(stream);
            }
            catch
            {
                return null;
            }
        }
    }


}
