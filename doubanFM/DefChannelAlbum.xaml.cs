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
    /// DefChannelAlbum.xaml 的交互逻辑
    /// </summary>
    public partial class DefChannelAlbum : UserControl
    {
        public Channel channel { get; set; }
        public bool IsFaved { get; set; }
        public DefChannelAlbum(Channel channel,bool isfaved)
        {
            InitializeComponent();
            this.channel = channel;
            this.IsFaved = isfaved;
            this.name.Content = channel.name;
            this.albumPic.Source = new BitmapImage(new Uri(channel.banner, UriKind.RelativeOrAbsolute));
            this.favPic.Source = IsFaved ? new BitmapImage(new Uri("Images/Faved.png", UriKind.RelativeOrAbsolute)) : new BitmapImage(new Uri("Images/AddFav.png", UriKind.RelativeOrAbsolute));
            this.favPic.MouseLeftButtonDown += delegate
            {
                if (IsFaved)
                {
                    this.favPic.Source = new BitmapImage(new Uri("Images/AddFav.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    this.favPic.Source = new BitmapImage(new Uri("Images/Faved.png", UriKind.RelativeOrAbsolute));
                }
            };
        }
    }
}
