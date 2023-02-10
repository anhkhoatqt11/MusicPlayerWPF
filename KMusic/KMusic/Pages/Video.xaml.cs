using LiteDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for Video.xaml
    /// </summary>
    public partial class Video : Page
    {
        public Video()
        {
            InitializeComponent();
            DisplayPresetData();
        }
        public class VideoFromFolder
        {
            public String Title { get; set; }
            public String Path { get; set; }
        }

        public ObservableCollection<VideoFromFolder> VideoList { get; set; } = new ObservableCollection<VideoFromFolder>();

        private List<VideoFromFolder> GetAll()
        {
            var list = new List<VideoFromFolder>();
            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                var col = db.GetCollection<VideoFromFolder>("video");
                foreach (VideoFromFolder _id in col.FindAll())
                {
                    list.Add(_id);
                }
            }
            return list;
        }

        public void DisplayPresetData()
        {
            VideoDataGrid.ItemsSource = GetAll();
        }

        private void VideoDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = VideoDataGrid.SelectedItem as VideoFromFolder;
            if (selectedItem != null)
            {
                var videoPlayerWindow = new VideoPlayerWindow(selectedItem.Path);
                videoPlayerWindow.Show();
            }
        }

        private void VideoDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
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

        private void SearchTextBoxVideo_TextChanged(object sender, TextChangedEventArgs e)
        {
            VideoList.Clear();
            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                var col = db.GetCollection<VideoFromFolder>("video");
                var video = col.Find(x => x.Title.Contains(SearchTextBoxVideo.Text));
                foreach (var m in video)
                {
                    VideoList.Add(m);
                }
            }
            VideoDataGrid.ItemsSource = VideoList;
        }
    }
}
