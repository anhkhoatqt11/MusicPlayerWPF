using LiteDB;
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
using static KMusic.Pages.Playlists;

namespace KMusic.Pages
{
    /// <summary>
    /// Interaction logic for CreatePlaylist.xaml
    /// </summary>
    public partial class CreatePlaylist : Page
    {
        public CreatePlaylist()
        {
            InitializeComponent();
        }
        private void SavePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            Playlist playlist = new Playlist
            {
                Name = NameTextBox.Text,
            };

            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                var playlists = db.GetCollection<Playlist>("playlists");
                playlists.Insert(playlist);
            }

            NavigationService.Navigate(new Uri("/Pages/Playlists.xaml", UriKind.Relative));
        }
        private void ReturnToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Playlists.xaml", UriKind.Relative));

        }
    }
}
