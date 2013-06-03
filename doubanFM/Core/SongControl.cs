using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace doubanFM.Core
{
    public class SongControl
    {
        public int channelNo = 1;
        public List<Song> currentQueue = new List<Song>();
        public int currentSongIdx = 0;
        public float currentSongPT = 0.0f;

        //获取下一首歌曲
        public bool Next(bool channelChanged = false)
        {
            bool getListSuccess = true;
            if (channelChanged)
                getListSuccess = GetNewList("n");
            else
            {
                if (currentSongIdx != currentQueue.Count - 1)
                    getListSuccess = GetNewList("e");
                else
                    getListSuccess = GetNewList("p");
            }
            return getListSuccess;
        }

        public void Skip()
        {
            GetNewList("s");
        }

        //向服务器发送“喜欢”、“不喜欢”标志
        public void Like(bool islike)
        {
            if (islike)
                GetNewList("r");
            else
                GetNewList("u");
        }

        //私人频道，垃圾桶（不再播放）功能
        public void Ban()
        {
            GetNewList("b");
        }

        private bool GetNewList(string type)
        {
            bool retSuccess = true;
            Song currentSong = currentQueue[currentSongIdx];
            string sidstr = (currentSongIdx == 0) ? string.Empty : currentSong.sid;
            Random random = new Random();
		    byte[] bytes = new byte[8];
            random.NextBytes(bytes);
            string r = (BitConverter.ToUInt64(bytes, 0) % 0xFFFFFFFFFF).ToString("x10");
            Parameters parameters = new Parameters();
            parameters.Add("type", type);
            parameters.Add("pb", "64");
            parameters.Add("sid", sidstr);
            parameters.Add("pt", currentSongPT.ToString());
            parameters.Add("channel", channelNo.ToString());
            parameters.Add("from", "mainsite");
            parameters.Add("r", r);
            string PostUrl = ConnectionBase.ConstructUrlWithParameters(@"http://douban.fm/j/mine/playlist", parameters);
            string SongJson = new ConnectionBase().Get(PostUrl);
            SongResult songresult = SongResult.FromJson(SongJson);
            if (type == "e")
                retSuccess = true;
            else if (songresult == null)
                retSuccess = false;
            else if (songresult != null && songresult.r != 0)
                retSuccess = false;
            else
            {
                if (songresult != null)
                {
                    if (songresult.song.Count == 0)
                        retSuccess = false;
                    else
                    {
                        if (type != "p")
                        {
                            currentQueue.Clear();
                            currentSongIdx = 0;
                        }
                        else
                        {
                            Song lastSong = currentQueue[currentQueue.Count - 1];
                            currentQueue.Clear();
                            currentSongIdx = 0;
                            currentQueue.Add(lastSong);
                        }
                        if (type == "r" || type == "u")
                            currentQueue.Add(currentSong);
                        
                        foreach (Song s in songresult.song)
                        {
                            if (s.subtype != "T")//剔除广告音频
                                currentQueue.Add(s);
                        }
                    }
                }
            }
            return retSuccess;
        }
    }
}
