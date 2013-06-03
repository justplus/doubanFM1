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
    /// DefSongList.xaml 的交互逻辑
    /// </summary>
    public partial class DefSongList : UserControl
    {
        public Song song { get; set; }
        public bool isPlaying { get; set; }
        public DefSongList(Song song)
        {
            InitializeComponent();
            this.song = song;
            this.songName.Content = song.title;
            this.singerAlbumName.Content = string.Format("{0} <{1}>", song.artist, song.albumtitle);
            this.albumPic.Source = new BitmapImage(new Uri(song.picture, UriKind.RelativeOrAbsolute));
            songRow.MouseEnter += delegate
            {
                if (isPlaying)
                    return;
                removeSong.Visibility = Visibility.Visible;
                this.Background = new SolidColorBrush(Color.FromRgb(224, 232, 190));
            };
            songRow.MouseLeave += delegate
            {
                if (isPlaying)
                    return;
                removeSong.Visibility = Visibility.Collapsed;
                this.Background = new SolidColorBrush(Colors.Transparent);
            };
        }
    }
}
