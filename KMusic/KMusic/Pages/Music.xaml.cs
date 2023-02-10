using LiteDB;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Data;
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
using static KMusic.Pages.Settings;
using NAudio.Wave;
using TagLib;
using System.IO;
using Ookii.Dialogs;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Application;
using System.Reflection;
using System.Collections.ObjectModel;
using static KMusic.Pages.Playlists;

namespace KMusic.Pages
{
    /// <summary>
    /// Interaction logic for Music.xaml
    /// </summary>
    public partial class Music : Page
    {
        public class MusicFromFolder
        {
            public String Title { get; set; }
            public String Path { get; set; }
            public Song Song { get; set; }
        }
        public static class Global
        {
            public static WaveOutEvent waveOut;

            public static NAudio.Wave.AudioFileReader audioFile;

            public static NAudio.Wave.AudioFileReader AudioFile
            {
                get { return audioFile; }
                set { audioFile = value; }
            }
        }
        public ObservableCollection<MusicFromFolder> MusicList { get; set; } = new ObservableCollection<MusicFromFolder>();
        public Music()
        {
            InitializeComponent();
            DisplayPresetData();
            HideColumn();
            UpdatePlaylistForContextMenu();
        }

        public void GetMusic()
        {
            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                var col = db.GetCollection<MusicFromFolder>("music");
                var music = col.FindAll();
                foreach (var m in music)
                {
                    MusicList.Add(m);
                }
            }
        }
        private List<MusicFromFolder> GetAll()
        {
            var list = new List<MusicFromFolder>();
            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                var col = db.GetCollection<MusicFromFolder>("music");
                foreach (MusicFromFolder _id in col.FindAll())
                {
                    list.Add(_id);
                }
            }
            return list;
        }

        public void DisplayPresetData()
        {
            MusicDataGrid.ItemsSource = GetAll();
        }
        private void HideColumn()
        {
            //int columnIndex = 0;
            //MusicDataGrid.Columns[columnIndex].Visibility = Visibility.Collapsed;

        }
        private void MusicDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                MusicFromFolder selectedRow = dataGrid.SelectedItem as MusicFromFolder;
                string cellValue = selectedRow.Path;
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;


                var file = TagLib.File.Create(cellValue);


                if (selectedRow != null)
                {
                    if (Global.waveOut != null)
                    {
                        Global.waveOut.Stop();
                        Global.waveOut.Dispose();
                    }

                    string title = file.Tag.Title;
                    string artist = file.Tag.FirstPerformer;

                    if (string.IsNullOrEmpty(title))
                    {
                        title = System.IO.Path.GetFileNameWithoutExtension(cellValue);
                    }

                    Global.waveOut = new WaveOutEvent();
                    var audioFile = new AudioFileReader(cellValue);
                    Global.audioFile = audioFile;
                    Global.waveOut.Init(audioFile);
                    Global.waveOut.Play();

                    var albumArt = file.Tag.Pictures.FirstOrDefault();
                    mainWindow.UpdateAlbumArt(albumArt);

                    mainWindow.UpdateTitleAndArtist(title, artist);
                    mainWindow.UpdateAudioFile(audioFile);
                    mainWindow.ChangePlayPauseButton();
                }
                else
                {
                    MessageBox.Show("Please select a row in the DataGrid.");
                }
            }
            catch (FileNotFoundException) {
                var settingsPage = new Settings();
                settingsPage.UpdateMusicList();
                MessageBox.Show("File không tồn tại");
                DisplayPresetData();
            }
        }
        private void Add_DSP(object sender, RoutedEventArgs e)
        {
            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                var col = db.GetCollection<MusicFromFolder>("dsp");
                var selectedRow = (MusicFromFolder)MusicDataGrid.SelectedItem;
                var audio = new MusicFromFolder
                {
                    Title = selectedRow.Title,
                    Path = selectedRow.Path,
                };
                if (!col.Exists(x => x.Title == selectedRow.Title))
                {
                    col.Insert(audio);
                }
                else
                {
                    MessageBox.Show("The song is already in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.DisplayPresetData();
            mainWindow.GetSongAudio();
        }
        private void MusicDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Title")
            {
                e.Column.Width = new DataGridLength(730);
            }
            if (e.PropertyName == "Path")
            {
                e.Column.Visibility = Visibility.Collapsed;
            }

             ((DataGridTextColumn)e.Column).ElementStyle = FindResource("DataGridRowWrapStyle") as Style;
        }

        public void UpdatePlaylistForContextMenu()
        {
            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                var playlists = db.GetCollection<Playlist>("playlists").FindAll();
                foreach (var playlist in playlists)
                {
                    MenuItem menuItem = new MenuItem
                    {
                        Header = playlist.Name,
                    };
                    menuItem.Click += AddToPlaylistMenuItem_Click;
                    AddToPlaylistMenuItem.Items.Add(menuItem);
                }
            }
        }
        private void AddToPlaylistMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            string playlistName = menuItem.Header.ToString();
            MusicFromFolder selectedSong = (MusicFromFolder)MusicDataGrid.SelectedItem;

            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                var playlists = db.GetCollection<Playlist>("playlists");
                var playlist = playlists.FindOne(x => x.Name == playlistName);

                if (playlist.Songs.Any(x => x.Title == selectedSong.Title && x.Path == selectedSong.Path))
                {
                    MessageBox.Show("The song is already in the playlist.");
                }
                else
                {
                    playlist.Songs.Add(new Song { Title = selectedSong.Title, Path = selectedSong.Path });
                    playlists.Update(playlist);
                }
            }
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MusicList.Clear();
            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                var col = db.GetCollection<MusicFromFolder>("music");
                var music = col.Find(x => x.Title.Contains(SearchTextBox.Text));
                foreach (var m in music)
                {
                    MusicList.Add(m);
                }
            }
            MusicDataGrid.ItemsSource = MusicList;
        }
    }
}
