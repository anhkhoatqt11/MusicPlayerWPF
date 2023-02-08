using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio.Wave;
using System.ComponentModel;
using System.Timers;

namespace KMusic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   
    public partial class MainWindow : Window
    {
        private WaveOutEvent waveOut;
        private AudioFileReader audioFile;
        private System.Timers.Timer timer;

        public string Title { get; set; }
        public string Artist { get; set; }
        public ImageSource AlbumArt { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Sidebar.SelectedIndex = 0;
            // initialize timer
            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {                           
                Dispatcher.Invoke(() =>
            {
                MusicSlider.Value = audioFile.CurrentTime.TotalSeconds;
                double elapsed = audioFile.CurrentTime.TotalSeconds;
                double total = audioFile.TotalTime.TotalSeconds;
                CurrentTimeTextBlock.Text = TimeSpan.FromSeconds(elapsed).ToString(@"hh\:mm\:ss");
                TotalLengthTextBlock.Text = TimeSpan.FromSeconds(total).ToString(@"hh\:mm\:ss");
            
            });
        }

        public void UpdateAudioFile(AudioFileReader audioFile)
        {
            this.audioFile = audioFile;
            MusicSlider.Maximum = audioFile.TotalTime.TotalSeconds;
            timer.Start();
        }

        private void MusicSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            audioFile.CurrentTime = TimeSpan.FromSeconds(e.NewValue);
        }



        public void UpdateTitleAndArtist(string title, string artist)
        {
            TitleTextBlock.Text = title;
            ArtistTextBlock.Text = artist;
            if (artist == null)
            {
                ArtistTextBlock.Text = "Unknow";
            }
        }

        private void Sidebar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = Sidebar.SelectedItem as NavButton;

            navframe.Navigate(selected.Navlink);
        }
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
