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
using System.Windows.Threading;
using System.Threading;
using LiteDB;
using MessageBox = System.Windows.MessageBox;
using System.IO;
using KMusic.Pages;
using static KMusic.Pages.Music;
using Application = System.Windows.Application;
using TagLib.Riff;
using Microsoft.VisualBasic.Devices;

namespace KMusic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   
    public partial class MainWindow : Window
    {
        //private WaveOutEvent waveOut;
        private AudioFileReader audioFile;
        private System.Windows.Forms.Timer _timer;
        private bool _isUserChange = false;
        // Load the songs from LiteDB
        private int _currentSongIndex = 0;
        private List<MusicFromFolder> _songs;

        private bool _isShuffleEnabled;
        public bool IsShuffleEnabled
        {
            get { return _isShuffleEnabled; }
            set { _isShuffleEnabled = value; OnPropertyChanged(); }
        }

        private void OnPropertyChanged()
        {
            throw new NotImplementedException();
        }

        public class MusicFromFolder
        {
            public LiteDB.ObjectId _id { get; set; }
            public String Title { get; set; }
            public String Path { get; set; }
            public bool IsShuffle { get; set; } = false;
            public bool IsLoop { get; set; } = false;
        }



        public string Title { get; set; }
        public string Artist { get; set; }
        public ImageSource AlbumArt { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Sidebar.SelectedIndex = 0;
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 100;
            _timer.Tick += Timer_Tick;
            _timer.Start();
            DisplayPresetData();
            _songs = GetAll();
        }

        public void GetSongAudio()
        {
            _songs = GetAll();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (audioFile != null)
            {
                double elapsed = audioFile.CurrentTime.TotalSeconds;
                double total = audioFile.TotalTime.TotalSeconds;

                Dispatcher.Invoke(() =>
                {
                    CurrentTimeTextBlock.Text = TimeSpan.FromSeconds(elapsed).ToString(@"hh\:mm\:ss");
                    TotalLengthTextBlock.Text = TimeSpan.FromSeconds(total).ToString(@"hh\:mm\:ss");
                    MusicSlider.Value = elapsed;
                });
            }
        }


        private void Path_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Global.waveOut.PlaybackState == PlaybackState.Playing)
            {
                Global.waveOut.Pause();
            }
            else
            {
                Global.waveOut.Play();
            }
        }


        public void UpdateAudioFile(AudioFileReader audioFile)
        {
            this.audioFile = audioFile;
            MusicSlider.Maximum = audioFile.TotalTime.TotalSeconds;
        }


        public void UpdateTitleAndArtist(string title, string artist)
        {
            TitleTextBlock.Text = title;
            ArtistTextBlock.Text = artist;
            if (artist == null)
            {
                ArtistTextBlock.Text = "Không rõ";
            }
        }

        private void Sidebar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = Sidebar.SelectedItem as NavButton;
            navframe.Navigate(selected.Navlink);
        }
        private List<MusicFromFolder> GetAll()
        {
            var list = new List<MusicFromFolder>();
            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                var col = db.GetCollection<MusicFromFolder>("dsp");
                foreach (MusicFromFolder _id in col.FindAll())
                {
                    list.Add(_id);
                }
            }
            return list;
        }


        public void DisplayPresetData()
        {
            DSP.ItemsSource = GetAll();
        }

        private void MusicSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _isUserChange = true;
        }

        private void MusicSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _isUserChange = false;
        }

        private void MusicSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isUserChange)
            {
                audioFile.CurrentTime = TimeSpan.FromSeconds(e.NewValue);
            }
        }

        private void Del_DSP(object sender, RoutedEventArgs e)
        {
            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                var col = db.GetCollection<MusicFromFolder>("dsp");
                var selectedRow = (MusicFromFolder)DSP.SelectedItem;
                var music = col.FindOne(x => x.Title == selectedRow.Title && x.Path == selectedRow.Path);
                if (music != null)
                {
                    col.Delete(music._id);
                }
            }
            DisplayPresetData();
            _songs = GetAll();
        }

        private void DSPDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            MusicFromFolder selectedRow = dataGrid.SelectedItem as MusicFromFolder;
            if (selectedRow != null)
            {
                if (Global.waveOut != null)
                {
                    Global.waveOut.Stop();
                    Global.waveOut.Dispose();
                }

                string cellValue = selectedRow.Path;


                var file = TagLib.File.Create(cellValue);

                string title = file.Tag.Title;
                string artist = file.Tag.FirstPerformer;

                if (string.IsNullOrEmpty(title))
                {
                    title = System.IO.Path.GetFileNameWithoutExtension(cellValue);
                }

                Global.waveOut = new WaveOutEvent();
                var audioFile = new AudioFileReader(cellValue);
                Global.waveOut.Init(audioFile);
                Global.waveOut.Play();

                UpdateTitleAndArtist(title, artist);
                UpdateAudioFile(audioFile);
            }
            else
            {
                MessageBox.Show("Please select a row in the DataGrid.");
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;

            if (Global.waveOut.PlaybackState == PlaybackState.Playing)
            {
                // Pause playback
                Global.waveOut.Pause();
                button.Content = new MaterialDesignThemes.Wpf.PackIcon()
                {
                    Kind = MaterialDesignThemes.Wpf.PackIconKind.Play
                };
            }
            else
            {
                // Resume playback
                Global.waveOut.Play();
                button.Content = new MaterialDesignThemes.Wpf.PackIcon()
                {
                    Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause
                };
            }
        }


        private void PlaySong(string path)
        {
            // Use Naudio to play the song
            Global.waveOut.Stop();
            audioFile = new AudioFileReader(path);
            Global.waveOut.Init(audioFile);

            //if (_isShuffleEnabled)
            //{
            //    var rand = new Random();
            //    // Play the next song randomly
            //    _currentSongIndex = rand.Next(0, _songs.Count);
            //}
            //else
            //{
            //    // Play the next song in the list
            //    _currentSongIndex = (_currentSongIndex + 1) % _songs.Count;
            //}
            Global.waveOut.Play();
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            // Switch to the previous song in the playlist
            _currentSongIndex--;
            if (_currentSongIndex < 0)
            {
                _currentSongIndex = _songs.Count - 1;
            }
            PlaySong(_songs[_currentSongIndex].Path);

            var file = TagLib.File.Create(_songs[_currentSongIndex].Path);

            string title = file.Tag.Title;
            string artist = file.Tag.FirstPerformer;

            if (string.IsNullOrEmpty(title))
            {
                title = System.IO.Path.GetFileNameWithoutExtension(_songs[_currentSongIndex].Path);
            }
            var audioFile = new AudioFileReader(_songs[_currentSongIndex].Path);

            UpdateTitleAndArtist(title, artist);
            UpdateAudioFile(audioFile);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // Switch to the next song in the playlist
            _currentSongIndex++;
            if (_currentSongIndex >= _songs.Count)
            {
                _currentSongIndex = 0;
            }
            PlaySong(_songs[_currentSongIndex].Path);

            var file = TagLib.File.Create(_songs[_currentSongIndex].Path);

            string title = file.Tag.Title;
            string artist = file.Tag.FirstPerformer;

            if (string.IsNullOrEmpty(title))
            {
                title = System.IO.Path.GetFileNameWithoutExtension(_songs[_currentSongIndex].Path);
            }
            var audioFile = new AudioFileReader(_songs[_currentSongIndex].Path);

            UpdateTitleAndArtist(title, artist);
            UpdateAudioFile(audioFile);
        }


    }
}
