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
    }
}
