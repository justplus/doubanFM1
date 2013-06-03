using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using doubanFM.Core;

namespace doubanFM
{
    /// <summary>
    /// DefSongInfo.xaml 的交互逻辑
    /// </summary>
    public partial class DefSongInfo : UserControl
    {
        public SimpleSong SimpleSongInfo { get; set; }
        public bool IsPlaying { get; set; }
        public DefSongInfo(SimpleSong simpleSongInfo)
        {
            InitializeComponent();
            this.IsPlaying = false;
            this.SimpleSongInfo = simpleSongInfo;
            this.songName.Content = simpleSongInfo.name;
            this.singerAlbumName.Content = string.Format("{0} <{1}>", simpleSongInfo.artist, simpleSongInfo.source.name);
        }
    }
}
