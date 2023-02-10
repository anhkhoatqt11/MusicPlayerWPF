using LiteDB;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
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
using static KMusic.Pages.Music;
using static KMusic.Pages.Playlists;

namespace KMusic.Pages
{

    public partial class PlaylistDetail : Page
    {
        private Playlist _selectedPlaylist;
        public string PlaylistName { get; set; }
        public PlaylistDetail(Playlist selectedPlaylist)
        {
            InitializeComponent();
            _selectedPlaylist = selectedPlaylist;
            PlaylistName = selectedPlaylist.Name;
            this.DataContext = this;
        }

        private void PlaylistDetail_Loaded(object sender, RoutedEventArgs e)
        {
            PlaylistDataGrid.ItemsSource = _selectedPlaylist.Songs;
        }
        private void PlaylistDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
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

        private void DeleteFromPlaylist_Click(object sender, RoutedEventArgs e)
        {
            Song selectedSong = (Song)PlaylistDataGrid.SelectedItem;
            _selectedPlaylist.Songs.Remove(selectedSong);

            using (var db = new LiteDatabase(@"C:/Temp/MyData.db"))
            {
                var playlists = db.GetCollection<Playlist>("playlists");
                var playlist = playlists.FindOne(x => x.Name == _selectedPlaylist.Name);
                playlist.Songs.Remove(selectedSong);
                playlists.Update(playlist);
            }

            PlaylistDataGrid.Items.Refresh();
        }

        private void Return_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Playlists.xaml", UriKind.Relative));
        }

        private void PlaylistDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                Song selectedSong = dataGrid.SelectedItem as Song;
                string cellValue = selectedSong.Path;
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

                var file = TagLib.File.Create(cellValue);

                if (selectedSong != null)
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
                    MessageBox.Show("Please select a song in the DataGrid.");
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("File không tồn tại");
            }
        }

    }
}
