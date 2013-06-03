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
using System.Windows.Shapes;

namespace doubanFM
{
    /// <summary>
    /// DesktopLyric.xaml 的交互逻辑
    /// </summary>
    public partial class DesktopLyric : Window
    {
        public DesktopLyric()
        {
            InitializeComponent();
            this.Top = SystemParameters.WorkArea.Height - 100;
            this.Left = (SystemParameters.WorkArea.Width - this.Width) / 2;
            /*this.Loaded += delegate
            {
                closeLyric.MouseLeftButtonDown += delegate
                {
                    this.Close();
                };
                titleCanvas.MouseLeftButtonDown += DragMove;
            };*/
        }

        private void DragMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}
