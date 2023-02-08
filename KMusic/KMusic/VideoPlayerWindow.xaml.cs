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

namespace KMusic
{
    /// <summary>
    /// Interaction logic for VideoPlayerWindow.xaml
    /// </summary>
    public partial class VideoPlayerWindow : Window
    {
        public VideoPlayerWindow()
        {
            InitializeComponent();
        }
        private string _videoPath;
        public VideoPlayerWindow(string videoPath)
        {
            InitializeComponent();
            _videoPath = videoPath;
            VideoPlayer.LoadedBehavior = MediaState.Manual;
            VideoPlayer.UnloadedBehavior = MediaState.Manual;
            LoadVideo();
        }

        private void LoadVideo()
        {
            VideoPlayer.Source = new Uri(_videoPath);
            VideoPlayer.Play();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Stop();
            this.Close();
        }
    }
}
