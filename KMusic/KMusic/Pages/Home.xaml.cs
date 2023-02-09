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

namespace KMusic.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        public Home()
        {
            InitializeComponent();
        }

        private void MusicCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Music.xaml", UriKind.Relative));
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            NavButton musicBtn = mainWindow.MusicBtn;
            musicBtn.IsSelected = true;
        }
        private void VideoCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Video.xaml", UriKind.Relative));
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            NavButton videoBtn = mainWindow.VideoBtn;
            videoBtn.IsSelected = true;
        }
    }
}
