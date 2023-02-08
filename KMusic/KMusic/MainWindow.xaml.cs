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

namespace KMusic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   
    public partial class MainWindow : Window
    {
        private WaveOutEvent waveOut;
        private AudioFileReader audioFile;
        private CancellationTokenSource cancellationTokenSource;

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
            // initialize timer
            cancellationTokenSource = new CancellationTokenSource();
            UpdateSliderAsync(cancellationTokenSource.Token);
            DisplayPresetData();
        }

        private async void UpdateSliderAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
                if (audioFile != null)
                {
                    double elapsed = audioFile.CurrentTime.TotalSeconds;
                    double total = audioFile.TotalTime.TotalSeconds;
                    CurrentTimeTextBlock.Text = TimeSpan.FromSeconds(elapsed).ToString(@"hh\:mm\:ss");
                    TotalLengthTextBlock.Text = TimeSpan.FromSeconds(total).ToString(@"hh\:mm\:ss");
                    double sec = audioFile.CurrentTime.TotalSeconds;
                    MusicSlider.Value = sec;
                }
            }
        }


        public void UpdateAudioFile(AudioFileReader audioFile)
        {
            this.audioFile = audioFile;
            MusicSlider.Maximum = audioFile.TotalTime.TotalSeconds;
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
                if (waveOut != null)
                {
                    waveOut.Stop();
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
                    AlbumArt = bitmap;
                }
                else
                {
                    AlbumArt = null;
                }

                string title = file.Tag.Title;
                string artist = file.Tag.FirstPerformer;

                if (string.IsNullOrEmpty(title))
                {
                    title = System.IO.Path.GetFileNameWithoutExtension(cellValue);
                }

                waveOut = new WaveOutEvent();
                var audioFile = new AudioFileReader(cellValue);
                waveOut.Init(audioFile);
                waveOut.Play();

                UpdateTitleAndArtist(title, artist);
                UpdateAudioFile(audioFile);
            }
            else
            {
                MessageBox.Show("Please select a row in the DataGrid.");
            }
        }
    }
}
