using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace doubanFM.Core
{
    public class LyricMV
    {
        //private string keyword;
        public List<int> songID;
        public List<string> mvId;
        public string mvUrl = string.Empty;
        public Dictionary<TimeSpan, string> lyricDictionary;
        public LyricMV()
        {
            songID = new List<int>();
            mvId = new List<string>();
            lyricDictionary = new Dictionary<TimeSpan, string>();
        }

        public void GetSongIds(string keyword)
        {
            string lyricXml;
            string PostUrl = string.Format("http://s.plcloud.music.qq.com/fcgi-bin/smartbox.fcg?utf8=1&key={0}&g_tk=5381", keyword);
            lyricXml = getWebContent(PostUrl);

            if (lyricXml.Length >= 20)
            {
                lyricXml = lyricXml.Substring(18, lyricXml.Length - 20);
                lyricXml = ProcessJson(lyricXml);
                Lyric lyric = Lyric.FromJson(lyricXml);
                if (lyric == null)
                    return;
                foreach (SimSongInfo song in lyric.tips.song)
                {
                    songID.Add(song.id);
                }
                foreach (SimMv MV in lyric.tips.mv)
                {
                    mvId.Add(MV.vid);
                }
            }
        }

        //判断是否有MV
        public bool CanFollowMV(string keyword)
        {
            Parameters parameters = new Parameters();
            parameters.Add("searchType", "mv");
            parameters.Add("keyword", keyword);
            string url = ConnectionBase.ConstructUrlWithParameters("http://www.yinyuetai.com/search/mv", parameters);
            string searchresult = new ConnectionBase().Get(url);
            if (string.IsNullOrEmpty(searchresult))
                return false;
            else
            {
                MatchCollection mc = Regex.Matches(searchresult, @"<div class=""noResult"">");
                if (mc.Count != 0)
                {
                    return false;
                }
                else
                {
                    MatchCollection mc1 = Regex.Matches(searchresult, @"<div class=""title mv_title"".*?</div>", RegexOptions.Singleline);
                    if (mc1.Count == 0)
                        return false;
                    else
                    {
                        Match tmpmatch = Regex.Match(mc1[0].Groups[0].Value, @"href=""([^\""]+?)""");
                        mvUrl = @"http://www.yinyuetai.com/video/player/" + tmpmatch.Groups[1].Value.Substring(7) + @"/v_0.swf";
                        return true;
                    }
                }
            }
        }

        private string getWebContent(string url)
        {
            try
            {
                StringBuilder sb = new StringBuilder("");
                WebRequest request = WebRequest.Create(url);
                request.Timeout = 10000;//10秒请求超时
                StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream(), Encoding.GetEncoding("GB2312"));
                while (sr.Peek() >= 0)
                {
                    sb.Append(sr.ReadLine());
                }
                return sb.ToString();
            }
            catch (WebException ex)
            {
                return ex.Message;
            }
        }

        public void LyricContent(int songID)
        {
            string content = string.Empty;
            string PostUrl = string.Format(@"http://music.qq.com/miniportal/static/lyric/{0}/{1}.xml", songID % 100, songID);
            content = getWebContent(PostUrl);
            MatchCollection matches = Regex.Matches(content, @"\[(\d{2,2}):(\d{2,2}).*?\]([\w\s]+)", RegexOptions.IgnoreCase);
            lyricDictionary.Clear();
            foreach (Match match in matches)
            {
                TimeSpan tm = new TimeSpan(0, 0, int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), 0);
                if (!string.IsNullOrEmpty(match.Groups[3].Value))
                    lyricDictionary[tm] = match.Groups[3].Value;
            }
        }

        private int nowLyricIndex = 0;
        public string lyric1 = string.Empty;//....
        public string lyric2 = string.Empty;
        public string lyric3 = string.Empty;
        public bool Refresh(TimeSpan now)
        {
            lyric1 = string.Empty;
            lyric2 = string.Empty;
            lyric3 = string.Empty;
            if (lyricDictionary.Keys.Count == 0)
                return false;
            //按照时间序列进行排序
            var list = lyricDictionary.Keys.ToList();
            list.Sort();

            while (true)
            {
                if (nowLyricIndex >= lyricDictionary.Keys.Count - 1)
                    break;
                TimeSpan lt = list[nowLyricIndex];
                TimeSpan bt = list[nowLyricIndex + 1];
                TimeSpan t1 = now - lt;
                TimeSpan t2 = now - bt;
                if (t1.TotalMilliseconds >= 0 && t2.TotalMilliseconds <= 0)
                {
                    if (nowLyricIndex == 0)
                    {
                        lyric1 = string.Empty;
                        lyric2 = lyricDictionary[lt];
                        lyric3 = lyricDictionary[bt];
                    }
                    else if (nowLyricIndex == lyricDictionary.Keys.Count - 2)
                    {
                        lyric1 = lyricDictionary[lt];
                        lyric2 = lyricDictionary[bt];
                        lyric3 = string.Empty;
                    }
                    else
                    {
                        lyric1 = lyricDictionary[list[nowLyricIndex - 1]];
                        lyric2 = lyricDictionary[lt];
                        lyric3 = lyricDictionary[bt];
                    }
                    break;
                }
                else if (t2.TotalMilliseconds > 0)
                    nowLyricIndex++;
                else
                    break;
            }
            return true;
        }

        private string ProcessJson(string JsonCode)
        {
            JsonCode = JsonCode.Replace("retcode:", @"""retcode"":");
            JsonCode = JsonCode.Replace("tips:", @"""tips"":");
            JsonCode = JsonCode.Replace("song:", @"""song"":");
            JsonCode = JsonCode.Replace("{id:", @"{""id"":");
            JsonCode = JsonCode.Replace(" name:", @" ""name"":");
            JsonCode = JsonCode.Replace(",singer:", @",""singer"":");
            JsonCode = JsonCode.Replace("singer_name:", @"""singer_name"":");
            JsonCode = JsonCode.Replace("album:", @"""album"":");
            JsonCode = JsonCode.Replace("mv:", @"""mv"":");
            JsonCode = JsonCode.Replace("vid:", @"""vid"":");
            return JsonCode;
        }
    }

    public class Lyric
    {
        public int retcode { get; set; }
        public Tips tips { get; set; }

        public Lyric()
        {
            tips = new Tips();
        }

        public static Lyric FromJson(string json)
        {
            try
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Lyric));
                using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                    return (Lyric)ser.ReadObject(stream);
            }
            catch
            {
                return null;
            }
        }
    }

    public class Tips
    {
        public List<SimSongInfo> song { get; set; }
        public List<string> singer { get; set; }
        public List<SimSongInfo> album { get; set; }
        public List<SimMv> mv { get; set; }

        public Tips()
        {
            song = new List<SimSongInfo>();
            singer = new List<string>();
            album = new List<SimSongInfo>();
            mv = new List<SimMv>();
        }
    }

    public class SimSongInfo
    {
        public int id { get; set; }
        public string name { get; set; }
        public string singer_name { get; set; }
    }

    public class SimMv
    {
        public int id { get; set; }
        public string name { get; set; }
        public string singer_name { get; set; }
        public string vid { get; set; }
    }
}
