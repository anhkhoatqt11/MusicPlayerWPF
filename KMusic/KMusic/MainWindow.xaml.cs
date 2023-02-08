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

        public class MusicFromFolder
        {
            public LiteDB.ObjectId _id { get; set; }
            public String Title { get; set; }
            public String Path { get; set; }
        }

        public string Title { get; set; }
        public string Artist { get; set; }
        public ImageSource AlbumArt { get; set; }




        public MainWindow()
        {
            InitializeComponent();
            Sidebar.SelectedIndex = 0;
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 1000;
            _timer.Tick += Timer_Tick;
            _timer.Start();
            DisplayPresetData();
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

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
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

                Thread.Sleep(1000);
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
                var albumArt = file.Tag.Pictures.FirstOrDefault();
                if (albumArt != null)
                {
                    var bitmap = new BitmapImage();
                    using (var stream = new MemoryStream(albumArt.Data.Data))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                    }
                    var imageControl = new Image();
                    imageControl.Source = bitmap;
                    albumArtImage = imageControl;
                }
                else
                {
                    albumArtImage = null;
                }

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


        //private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (Global.waveOut.PlaybackState == PlaybackState.Playing)
        //    {
        //        Global.waveOut.Pause();
        //    }
        //    else
        //    {
        //        Global.waveOut.Play();
        //    }
        //}

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayPausePath == null)
            {
                return;
            }

            if (Global.waveOut.PlaybackState == PlaybackState.Playing)
            {
                // Pause playback
                Global.waveOut.Pause();

                Dispatcher.Invoke(() =>
                {
                    PlayPausePath.Data = null;
                    PlayPausePath.Data = Geometry.Parse("M5,20 L15,12.5 L15,27.5 Z");
                });
            }
            else
            {
                // Resume playback
                Global.waveOut.Play();

                Dispatcher.Invoke(() =>
                {
                    PlayPausePath.Data = null;
                    PlayPausePath.Data = Geometry.Parse("M5,20 L15,12.5 L15,27.5 M10,12.5 L10,27.5 Z");
                });
            }
        }
    }
}
