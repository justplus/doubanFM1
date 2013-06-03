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
using System.Text.RegularExpressions;
using System.Windows.Threading;
using doubanFM.Core;

namespace doubanFM
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum Opeartions : int
        {
            Login = 0,
            Channel,
            Setting,
            Search,
            PlayList
        };
        //登陆状态属性
        public static int LoginState { get; set; }
        private string CaptchaID;
        private string CaptchaUrl;
        private string loginUserName;
        //频道列表当前页
        public static int currentType = 1;
        public static int currentPage = 1;
        public static int totalPage = 1;
        //播放列表控制对象
        SongControl songControl = new SongControl();
        //收藏播放列表
        FavChannelResult favChannels = new FavChannelResult();
        //定时器
        private DispatcherTimer timer = new DispatcherTimer();              //控制歌曲进度
        private DispatcherTimer delaytimer = new DispatcherTimer();         //如果歌曲加载10s则跳下一首
        //桌面歌词窗口
        DesktopLyric lyricWindow = new DesktopLyric();
        LyricMV lyric = new LyricMV();

        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = new TimeSpan(0, 0, 1);
            delaytimer.Interval = new TimeSpan(0, 0, 10);
        }

        /// <summary>
        /// 窗口初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double oriHeight = this.Height;
            double loginHeight = oriHeight + 200;
            double channelHeight = oriHeight + 350;
            double settingHeight = oriHeight + 150;
            double searchHeight = oriHeight + 350;
            double playlistHeight = oriHeight + 350;
            //提取设置项
            string bkimagepath = Properties.Settings.Default.BKImage;
            if (!string.IsNullOrEmpty(bkimagepath) && System.IO.File.Exists(bkimagepath))
            {
                ImageBrush imageBrush = new ImageBrush(new BitmapImage(new Uri(bkimagepath, UriKind.Absolute)));
                imageBrush.Stretch = Stretch.UniformToFill;
                imageBrush.Opacity = 1.0;
                backBorder.Background = imageBrush;
            }
            else
            {
                byte[] bkcolor = Properties.Settings.Default.BKColor;
                if (bkcolor == null)
                {
                    bkcolor = new byte[] { 89, 147, 115 };
                }
                else
                {
                    backBorder.Background = new SolidColorBrush(Color.FromRgb(bkcolor[0], bkcolor[1], bkcolor[2]));
                }
            }
            //置顶
            topButton.MouseLeftButtonDown += delegate
            {
                this.Topmost = !this.Topmost;
            };
            //最小化
            minButton.MouseLeftButtonDown += delegate
            {
                this.WindowState = WindowState.Minimized;
            };
            //关闭
            closeButton.MouseLeftButtonDown += delegate
            {
                this.Close();
            };
            this.Closing += delegate { lyricWindow.Close(); };
            //拖动
            titleCanvas.MouseLeftButtonDown += DragMove;
            //声音调节
            volumeSlider.ValueChanged += delegate
            {
                player.Volume = volumeSlider.Value / 10;
            };
            //获取播放列表
            getSongList(true);

            foreach (Border border in opCanvas.Children)
            {
                border.MouseLeftButtonDown += delegate
                {
                    int opTag = Int32.Parse(border.Tag.ToString());
                    Canvas cv = border.Child as Canvas;
                    Label lb = cv.Children[1] as Label;
                    if (opTag >= 10)
                    {
                        defaultOpBar();//恢复已点击的控制按钮状态
                        this.Height = oriHeight;
                        border.Tag = opTag - 10;
                        lb.Foreground = new SolidColorBrush(Colors.White);
                        //border.Background = new SolidColorBrush(Colors.Transparent);
                    }
                    else
                    {
                        defaultOpBar();//恢复已点击的控制按钮状态
                        border.Tag = opTag + 10;
                        lb.Foreground = new SolidColorBrush(Colors.Red);
                        //border.Background = new LinearGradientBrush(Colors.Transparent, Colors.White, new Point(0, 0.9), new Point(0, 1.0));
                        //border.Background = new SolidColorBrush(Color.FromRgb(89, 147, 115));
                        if (opTag == (int)Opeartions.Login)
                        {
                            this.Height = loginHeight;
                        }
                        else if (opTag == (int)Opeartions.Channel)
                        {
                            this.Height = channelHeight;
                        }
                        else if (opTag == (int)Opeartions.Setting)
                        {
                            this.Height = settingHeight;
                        }
                        else if (opTag == (int)Opeartions.Search)
                        {
                            this.Height = searchHeight;
                            ascPage.Visibility = Visibility.Collapsed;
                            decPage.Visibility = Visibility.Collapsed;
                        }
                        else if (opTag == (int)Opeartions.PlayList)
                        {
                            this.Height = playlistHeight;
                        }
                        changeOverWindow(opTag);
                    }
                };
            }

            //设置按钮事件
            Dictionary<Border, Brush> colors = new Dictionary<Border, Brush>();
            Border[] colorborders = new Border[] { colorborder1, colorborder2, colorborder3, colorborder4, colorborder5, colorborder6, colorborder7, colorborder8, colorborder9, colorborder10, colorborder11 };
            foreach (Border b in colorborders)
            {
                b.MouseLeftButtonDown += delegate
                {
                    //backBorder.Background = b.Background;
                    //Properties.Settings.Default.BKColor[0]=b.Background.
                    SolidColorBrush scb = b.Background as SolidColorBrush;
                    if (Properties.Settings.Default.BKColor == null)
                        Properties.Settings.Default.BKColor = new byte[3];
                    Properties.Settings.Default.BKColor[0] = scb.Color.R;
                    Properties.Settings.Default.BKColor[1] = scb.Color.G;
                    Properties.Settings.Default.BKColor[2] = scb.Color.B;
                    Properties.Settings.Default.BKImage = string.Empty;
                    Properties.Settings.Default.Save();
                    backBorder.Background = b.Background;
                };
            }
            chooseBkCanvas.MouseLeftButtonDown += delegate
            {
                System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
                fileDialog.Filter = "图片文件(*.jpg,*.png,*.bmp)|*.jpg;*.png;*.bmp";
                if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ImageBrush imageBrush = new ImageBrush(new BitmapImage(new Uri(fileDialog.FileName, UriKind.Absolute)));
                    imageBrush.Stretch = Stretch.UniformToFill;
                    imageBrush.Opacity = 1.0;
                    backBorder.Background = imageBrush;
                    Properties.Settings.Default.BKImage = fileDialog.FileName;
                    Properties.Settings.Default.Save();
                }
            };

            //搜索按钮事件
            querySongCanvas.MouseLeftButtonDown += delegate
            {
                SongsQueryed.Children.Clear();
                getSongs(queryType.SelectedIndex, querySongWord.Text.Trim());
            };

            hotChannelCanvas.MouseLeftButtonDown += delegate
            {
                currentPage = 1;
                currentType = 1;
                getChannels();
            };
            uptrendChannelCanvas.MouseLeftButtonDown += delegate
            {
                currentPage = 1;
                currentType = 2;
                getChannels();
            };
            searchChannelCanvas.MouseLeftButtonDown += delegate
            {
                channelTable.Children.Clear();
                searchBoxCanvas.Visibility = Visibility.Visible;
                decPage.Visibility = Visibility.Collapsed;
                ascPage.Visibility = Visibility.Collapsed;
                searchBoxChannelCanvas.MouseLeftButtonDown += delegate
                {
                    string keyword = queryWord.Text.Trim();
                    currentPage = 1;
                    currentType = 3;
                    getChannels(keyword);
                };
            };
            favChannelCanvas.MouseLeftButtonDown += delegate
            {
                favChannels = FavChannelResult.FromJson();
                if (LoginState == 1)
                {
                    favChannels.Insert(loginUserName, 0, "私人兆赫", "Images/Personnel.png");
                    favChannels.Insert(loginUserName, -3, "红心兆赫", "Images/LoveChannel.jpg");
                }
                favChannels.ToJson();
                currentPage = 1;
                currentType = 0;
                getChannels();
            };
            decPage.MouseLeftButtonDown += delegate
            {
                //ascPage.IsEnabled = true;
                ascPage.Visibility = Visibility.Visible;
                currentPage--;
                if (currentPage == 1)
                    decPage.Visibility = Visibility.Collapsed;
                if (currentType == 3)
                {
                    string keyword = queryWord.Text.Trim();
                    getChannels(keyword);
                }
                else
                    getChannels();
            };
            ascPage.MouseLeftButtonDown += delegate
            {
                decPage.Visibility = Visibility.Visible;
                currentPage++;
                if (currentPage == totalPage)
                    ascPage.Visibility = Visibility.Collapsed;
                if (currentType == 3)
                {
                    string keyword = queryWord.Text.Trim();
                    getChannels(keyword);
                }
                else
                    getChannels();
            };
            //默认打开歌词窗口
            bool desktopLyric = Properties.Settings.Default.DesktopLyric;
            if(desktopLyric)
                lyricWindow.Show();
            else
                lyricWindow.Hide();

            //歌词显示控制
            lyricEnable.Checked += delegate
            {
                lyricWindow.Show();
                Properties.Settings.Default.DesktopLyric = true;
                Properties.Settings.Default.Save();
            };
            lyricEnable.Unchecked += delegate
            {
                lyricWindow.Hide();
                Properties.Settings.Default.DesktopLyric = false;
                Properties.Settings.Default.Save();
            };

            //判断是否登陆
            LoginState = await HasLogined();
            if (LoginState == 0)
                loginTitle.Content = "登陆";
            else if (LoginState == 1)
                loginTitle.Content = "用户";
            //登出
            logoutButton.MouseLeftButtonDown += delegate
            {
                ConnectionBase.cc = new System.Net.CookieContainer(1000, 1000, 100000);
                ConnectionBase.SaveCookies();
                loginCanvas.Visibility = Visibility.Visible;
                loginedCanvas.Visibility = Visibility.Collapsed;
                loginTitle.Content = "登陆";
                LoginState = 0;
            };
            
            //登陆按钮事件
            loginBorder.MouseLeftButtonDown += delegate
            {
                if (LoginState == 0 || LoginState == -1)//未登陆状态
                {
                    loginCanvas.Visibility = Visibility.Visible;
                    loginedCanvas.Visibility = Visibility.Collapsed;
                }
                else if (LoginState == 1)
                {
                    loginCanvas.Visibility = Visibility.Collapsed;
                    loginedCanvas.Visibility = Visibility.Visible;
                }
            };

            //频道列表事件
            channelBorder.MouseLeftButtonDown += delegate
            {
                /**默认载入收藏列表**/
                favChannels = FavChannelResult.FromJson();
                if (LoginState == 1)
                {
                    favChannels.Insert(loginUserName, 0, "私人兆赫", "Images/Personnel.png");
                    favChannels.Insert(loginUserName, -3, "红心兆赫", "Images/LoveChannel.jpg");
                }
                favChannels.ToJson();
                currentPage = 1;
                currentType = 0;
                getChannels();
                /**end**/
            };
        }

        /// <summary>
        /// 拖拽窗口标题栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DragMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        /// <summary>
        /// 操作栏的初始状态
        /// </summary>
        private void defaultOpBar()
        {
            foreach (Border border in opCanvas.Children)
            {
                int opTag = Int32.Parse(border.Tag.ToString());
                border.Tag = opTag >= 10 ? opTag - 10 : opTag;
                Canvas cv = border.Child as Canvas;
                Label lb = cv.Children[1] as Label;
                lb.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void changeOverWindow(int opTag)
        {
            for (int i = 0; i < 5; i++)
            {
                overWindowCanvas.Children[i].Visibility = (i != opTag) ? Visibility.Hidden : Visibility.Visible;
            }
        }

        /// <summary>
        /// 异步判断用户是否登陆
        /// </summary>
        /// <returns>登录状态：[-1]未联网 [0]未登录 [1]已登录</returns>
        private async Task<int> HasLogined()
        {
            int retValu = 0;
            string logName = string.Empty, logPlayed = string.Empty, logLiked = string.Empty, logBanned = string.Empty;
            await Task.Run(() =>
                {
                    string loginresult = new ConnectionBase().Get(@"http://douban.fm/");
                    if (!string.IsNullOrEmpty(loginresult))
                    {
                        Match indMatch = Regex.Match(loginresult, @"豆瓣", RegexOptions.IgnoreCase);
                        if (indMatch == null || string.IsNullOrEmpty(indMatch.Groups[0].Value))
                            retValu = -1;
                        else
                        {
                            Match match = Regex.Match(loginresult, @"var\s*globalConfig\s*=\s*{\s*uid\s*:\s*'(\d*)'", RegexOptions.IgnoreCase);
                            string s = match.Groups[1].Value;
                            if (string.IsNullOrEmpty(s))
                                retValu = 0;
                            else
                            {
                                retValu = 1;
                                MatchCollection mc = Regex.Matches(loginresult, @"<span id=""user_name"">([^{].*?)\s");
                                logName = mc[0].Groups[1].Value;
                                match = Regex.Match(loginresult, @"累积收听<span id=""rec_played"">(\d+)</span>首");
                                logPlayed = match.Groups[1].Value + "首";
                                match = Regex.Match(loginresult, @"加红心<span id=""rec_liked"">(\d+)</span>首");
                                logLiked = match.Groups[1].Value + "首";
                                match = Regex.Match(loginresult, @"<span id=""rec_banned"">(\d+)</span>首不再播放");
                                logBanned = match.Groups[1].Value + "首";
                            }
                        }
                    }
                    else
                        retValu = -1;
                });
            if (retValu == 1)
            {
                loginedName.Content = logName;
                loginedPlayed.Content = logPlayed;
                loginedLiked.Content = logLiked;
                loginedBanned.Content = logBanned;
                loginUserName = logName;
            }
            return retValu;
        }

        /// <summary>
        /// 异步加载验证码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void opLoginCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (LoginState != 0)
                return;
            await Task.Run(() =>
                {
                    CaptchaID = new ConnectionBase().Get("http://douban.fm/j/new_captcha");
                    CaptchaID = CaptchaID.Trim('\"');
                    CaptchaUrl = @"http://douban.fm/misc/captcha?size=m&id=" + CaptchaID;
                });
            if (!string.IsNullOrEmpty(CaptchaID))
            {
                BitmapImage captcha = new BitmapImage(new Uri(CaptchaUrl, UriKind.Absolute));
                captchaImage.Source = captcha;
            }
            else
            {
                errorMessage.Content = "验证码加载失败";
            }
        }

        /// <summary>
        /// 用户登陆
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void loginButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(userName.Text.Trim()))
                errorMessage.Content = "用户名不能为空";
            else if (string.IsNullOrEmpty(userPassword.Password.Trim()))
                errorMessage.Content = "密码不能为空";
            else if (string.IsNullOrEmpty(userCaptcha.Text.Trim()))
                errorMessage.Content = "验证码不能为空";
            else
            {
                Parameters parameters = new Parameters();
                parameters.Add("source", "radio");
                parameters.Add("alias", userName.Text.Trim());
                parameters.Add("form_password", userPassword.Password.Trim());
                parameters.Add("captcha_solution", userCaptcha.Text.Trim());
                parameters.Add("captcha_id", CaptchaID);
                parameters.Add("from", "mainsite");
                parameters.Add("remember", "on");
                string LogOnJson = "";
                await Task.Run(() =>
                {
                    LogOnJson = new ConnectionBase().Post("http://douban.fm/j/login", Encoding.UTF8.GetBytes(parameters.ToString()));
                });
                if (LogOnJson == string.Empty)
                {
                    errorMessage.Content = "网络连接错误";
                    return;
                }
                else
                {
                    LogOnResult logonresult = LogOnResult.FromJson(LogOnJson);
                    if (logonresult.r == 1)
                    {
                        errorMessage.Content = logonresult.err_msg;
                        return;
                    }
                    else if (logonresult.r == 0)
                    {
                        loginCanvas.Visibility = Visibility.Collapsed;
                        loginedCanvas.Visibility = Visibility.Visible;
                        loginedName.Content = logonresult.user_info.name;
                        loginedPlayed.Content = logonresult.user_info.play_record.played + "首";
                        loginedLiked.Content = logonresult.user_info.play_record.liked + "首";
                        loginedBanned.Content = logonresult.user_info.play_record.banned + "首";
                        loginTitle.Content = "用户";
                        LoginState = 1;
                        loginUserName = logonresult.user_info.name; 
                    }
                }
            }
        }

        /// <summary>
        /// 获取兆赫列表
        /// </summary>
        /// <param name="type">类型：[0]收藏兆赫 [1]热门兆赫 [2]上升兆赫 [3]搜索到兆赫</param>
        /// <param name="page">页码</param>
        private async void getChannels(string queryKeywrod="")
        {
            string url = string.Empty;
            searchBoxCanvas.Visibility = Visibility.Collapsed;
            if(currentType==1)
                url = string.Format("http://douban.fm/j/explore/hot_channels?start={0}&limit={1}", (currentPage - 1) * 6, 6);
            else if (currentType == 2)
                url = string.Format("http://douban.fm/j/explore/up_trending_channels?start={0}&limit={1}", (currentPage - 1) * 6, 6);
            else if (currentType == 3)
            {
                if (queryKeywrod.Trim() != string.Empty)
                    url = string.Format("http://douban.fm/j/explore/search?query={0}&start={1}&limit={2}", queryKeywrod, (currentPage - 1) * 6, 6);
                else
                    return;
            }
            else
            {
                List<Channel> favchannels = favChannels.GetChannels(loginUserName);
                totalPage = (favchannels.Count - 1) / 6 + 1;
                if(currentPage==1)
                    decPage.Visibility = Visibility.Collapsed;
                if (currentPage == totalPage)
                    ascPage.Visibility = Visibility.Collapsed;
                else
                    ascPage.Visibility = Visibility.Visible;
                int start = 6 * (currentPage - 1);
                int end = (favchannels.Count >= 6 * currentPage) ? 6 * currentPage - 1 : favchannels.Count - 1;
                channelTable.Children.Clear();
                for (int i = start; i <= end; i++)
                {
                    int r = (i - start) / 3;
                    int c = (i - start) % 3;
                    DefChannelAlbum channelAlbum = new DefChannelAlbum(favchannels[i],true);
                    channelAlbum.Margin = new Thickness(20 + 160 * c, 140 * r, 0, 0);
                    channelAlbum.favPic.MouseLeftButtonDown += delegate
                    {
                        if (channelAlbum.IsFaved)
                        {
                            favChannels.Delete(loginUserName, channelAlbum.channel.id);
                            favChannels.ToJson();
                            getChannels();
                        }
                        else
                        {
                            favChannels.Add(loginUserName, channelAlbum.channel.id, channelAlbum.channel.name, channelAlbum.channel.banner);
                            favChannels.ToJson();
                            getChannels();
                        }
                    };
                    channelAlbum.albumPic.MouseLeftButtonDown += delegate
                    {
                        songControl.channelNo = channelAlbum.channel.id;
                        softwareTitle.Content = string.Format("豆瓣FM--{0}兆赫", channelAlbum.channel.name);
                        getSongList(true);
                    };
                    channelTable.Children.Add(channelAlbum);
                }
                return;
            }
            ChannelResult cr = new ChannelResult();
            tipLabel.Content = "加载中...";
            await Task.Run(() =>
            {
                string jsonresults = new ConnectionBase().Get(url);
                cr = ChannelResult.FromJson(jsonresults);
            });
            if (cr != null && cr.status)
            {
                tipLabel.Content = string.Empty;
                List<Channel> channels = cr.data.channels;
                totalPage = cr.data.total;
                if (currentPage == 1)
                    decPage.Visibility = Visibility.Collapsed;
                if (currentPage == totalPage)
                    ascPage.Visibility = Visibility.Collapsed;
                else
                    ascPage.Visibility = Visibility.Visible;
                int start = 0;
                int end = channels.Count - 1;
                channelTable.Children.Clear();
                for (int i = start; i <= end; i++)
                {
                    int r = (i - start) / 3;
                    int c = (i - start) % 3;
                    DefChannelAlbum channelAlbum = new DefChannelAlbum(channels[i % 6], favChannels.isFaved(loginUserName, channels[i % 6].id));
                    channelAlbum.Margin = new Thickness(20 + 160 * c, 140 * r, 0, 0);
                    channelAlbum.favPic.MouseLeftButtonDown += delegate
                    {
                        if (channelAlbum.IsFaved)
                        {
                            favChannels.Delete(loginUserName, channelAlbum.channel.id);
                            favChannels.ToJson();
                        }
                        else
                        {
                            favChannels.Add(loginUserName, channelAlbum.channel.id, channelAlbum.channel.name, channelAlbum.channel.banner);
                            favChannels.ToJson();
                        }
                    };
                    channelAlbum.albumPic.MouseLeftButtonDown += delegate
                    {
                        songControl.channelNo = channelAlbum.channel.id;
                        softwareTitle.Content = string.Format("豆瓣FM--{0}兆赫", channelAlbum.channel.name);
                        getSongList(true);
                    };
                    channelTable.Children.Add(channelAlbum);
                }
            }
            else
                tipLabel.Content = "加载失败...";
        }

        /// <summary>
        /// 异步获取歌曲列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="queryKeywrod"></param>
        private async void getSongs(int type, string queryKeywrod)
        {
            if (queryKeywrod != string.Empty)
            {
                string cate = string.Empty;
                int limit = 175;
                if (type == 0)
                    cate = "misc";
                else if (type == 1)
                    cate = "name";
                else if (type == 2)
                    cate = "album";
                else
                    cate = "singer";
                string url = string.Format("http://douban.fm/j/open_channel/creation/search?keyword={0}&cate={1}&limit={2}", queryKeywrod, cate, limit);
                SearchSongResult ssr = new SearchSongResult();
                tipSongLabel.Content = "加载中...";
                await Task.Run(() =>
                {
                    string jsonresults = new ConnectionBase().Get(url);
                    ssr = SearchSongResult.FromJson(jsonresults);
                });
                if (ssr != null && ssr.status)
                {
                    tipSongLabel.Content = "";
                    foreach (SimpleSong simpleSong in ssr.data.songs)
                    {
                        DefSongInfo defSongInfo = new DefSongInfo(simpleSong);
                        defSongInfo.playButton.MouseLeftButtonDown += delegate
                        {
                            player.Pause();
                            playerPlay.Source = new BitmapImage(new Uri("Images/Play.png", UriKind.RelativeOrAbsolute));
                            player.Tag = "Play";
                            if (!defSongInfo.IsPlaying)
                            {
                                searchplayer.Source = new Uri(defSongInfo.SimpleSongInfo.url, UriKind.Absolute);
                                searchplayer.Play();
                                defSongInfo.IsPlaying = true;
                                defSongInfo.playButton.Source = new BitmapImage(new Uri("Images/Pause.png", UriKind.RelativeOrAbsolute));
                            }
                            else
                            {
                                searchplayer.Pause();
                                defSongInfo.IsPlaying = false;
                                defSongInfo.playButton.Source = new BitmapImage(new Uri("Images/Play.png", UriKind.RelativeOrAbsolute));
                            }
                        };
                        SongsQueryed.Children.Add(defSongInfo);
                    }
                }
                else
                    tipSongLabel.Content = "加载失败...";
            }
        }

        /// <summary>
        /// 异步获取播放列表
        /// </summary>
        private void getSongList(bool channelChanged=false,bool naturalEnd=false)
        {
            songControl.Next(channelChanged);
            if (!naturalEnd)
            {
                for (int i = 0; i < songControl.currentQueue.Count; i++)
                {
                    DefSongList songList = new DefSongList(songControl.currentQueue[i]);
                    songList.MouseDoubleClick += delegate
                    {
                        songList.removeSong.Visibility = Visibility.Collapsed;
                        SetPlayer(songList.song);
                    };
                    songListCanvas.Children.Add(songList);
                }
                SetPlayer(songControl.currentQueue[0]);
            }
            if (songControl.currentSongIdx < songControl.currentQueue.Count)
            {
                SetPlayer(songControl.currentQueue[songControl.currentSongIdx]);
            }
        }

        //异步加载歌词
        private async void getSongLyric(Song song)
        {
            await Task.Run(() =>
                {
                    lyric = new LyricMV();
                    string keyword = string.Empty;
                    if (song.title.Contains("("))
                        keyword += song.title.Substring(0, song.title.IndexOf("("));
                    else
                        keyword += song.title;
                    keyword += @" ";
                    keyword += song.artist;
                    lyric.GetSongIds(keyword);
                    if (lyric.songID.Count == 0)
                        return;
                    lyric.LyricContent(lyric.songID[0]);
                });
        }

        private void SetPlayer(Song song)
        {
            timer.Start();
            player.Source = new Uri(song.url, UriKind.Absolute);
            player.Play();
            playerSongName.Content = song.title;
            playerAlbum.Source = new BitmapImage(new Uri(song.picture, UriKind.Absolute));
            playerSingerAlbum.Content = string.Format("{0} <{1}>", song.artist, song.albumtitle);
            playerLike.Source = (song.like == "1") ? new BitmapImage(new Uri("Images/RedHeart.png", UriKind.RelativeOrAbsolute)) : new BitmapImage(new Uri("Images/Heart.png", UriKind.RelativeOrAbsolute));
            playerLike.Tag = (song.like == "1") ? "Like" : "Unlike";
            songControl.currentSongIdx = songControl.currentQueue.IndexOf(song);
            getSongLyric(song);
            DefSongList dsl = songListCanvas.Children[songControl.currentSongIdx] as DefSongList;
            foreach (DefSongList tmp in songListCanvas.Children)
            {
                tmp.Background = new SolidColorBrush(Colors.Transparent);
                tmp.isPlaying = false;
            }
            dsl.Background = new SolidColorBrush(Color.FromRgb(224,232,190));
            dsl.isPlaying = true;
            int length = (int)(Double.Parse(song.length));
            timer.Tick += delegate
            {
                TimeSpan AfterSpan = player.Position;
                playerSongTime.Content = string.Format("{0:D2}:{1:D2}/{2:D2}:{3:D2}", AfterSpan.Minutes, AfterSpan.Seconds, length / 60, length - length / 60 * 60);
                playerProgress.Value = AfterSpan.TotalSeconds / length * 100;

                //设置歌词
                lyric.Refresh(AfterSpan);
                lyricWindow.lyric1.Content = lyric.lyric1;
                lyricWindow.lyric2.Content = lyric.lyric2;
                lyricWindow.lyric3.Content = lyric.lyric3;
            };
        }

        private void player_MediaEnded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            playerSongTime.Content = "00:00/00:00";
            playerProgress.Value = 0;
            getSongList();
        }

        private void playerPlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((string)player.Tag == "Play")
            {
                playerPlay.Source = new BitmapImage(new Uri("Images/Pause.png", UriKind.RelativeOrAbsolute));
                player.Tag = "Pause";
                player.Play();
                //****
                if (SongsQueryed.Children.Count > 0)
                {
                    searchplayer.Pause();
                    foreach(DefSongInfo defsi in SongsQueryed.Children)
                    {
                        defsi.playButton.Source = new BitmapImage(new Uri("Images/Play.png", UriKind.RelativeOrAbsolute));
                        defsi.IsPlaying = false;
                    }
                }
                //*****
            }
            else
            {
                playerPlay.Source = new BitmapImage(new Uri("Images/Play.png", UriKind.RelativeOrAbsolute));
                player.Tag = "Play";
                player.Pause();
            }
        }

        private void playerLike_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((string)playerLike.Tag == "Like")
            {
                playerLike.Source = new BitmapImage(new Uri("Images/Heart.png", UriKind.RelativeOrAbsolute));
                playerLike.Tag = "Unlike";
                songControl.Like(false);
            }
            else
            {
                playerLike.Source = new BitmapImage(new Uri("Images/RedHeart.png", UriKind.RelativeOrAbsolute));
                playerLike.Tag = "Like";
                songControl.Like(true);
            }
        }

        private void playerSkip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
            playerSongTime.Content = "00:00/00:00";
            playerProgress.Value = 0;
            getSongList();
        }

        private void playerBan_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            songControl.Ban();
            getSongList();
        }
    }
}
