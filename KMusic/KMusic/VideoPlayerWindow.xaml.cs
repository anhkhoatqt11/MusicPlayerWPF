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
using System.Windows.Threading;

namespace KMusic
{
    /// <summary>
    /// Interaction logic for VideoPlayerWindow.xaml
    /// </summary>
    public partial class VideoPlayerWindow : Window
    {
        private DispatcherTimer _timer;
        private bool _isPlaying;

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
            VideoPlayer.MediaOpened += (s, args) =>
            {
                PlaybackSlider.Maximum = VideoPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            };
        }

        private void LoadVideo()
        {
            VideoPlayer.Source = new Uri(_videoPath);
            VideoPlayer.Play();
            _isPlaying = true;
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isPlaying)
            {
                VideoPlayer.Pause();
                PlayPauseButton.Content = "Play";
            }
            else
            {
                VideoPlayer.Play();
                PlayPauseButton.Content = "Pause";
            }
            _isPlaying = !_isPlaying;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Stop();
            this.Close();
        }
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            VideoPlayer.Volume = VolumeSlider.Value / 100.0;
        }

        private void PlaybackSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            VideoPlayer.Position = TimeSpan.FromSeconds(e.NewValue);

        }
        private void VideoSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            VideoPlayer.Pause();
        }

        private void VideoSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            VideoPlayer.Play();
        }


    }
}
