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
using Ookii.Dialogs;
using LiteDB;
using System.IO;

namespace KMusic.Pages
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
        }

        public class MusicFromFolder
        {
            public String Title { get; set; }
            public String Path { get; set; }
        }    

        private void StoreMusic(object sender, RoutedEventArgs e)
        {
            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                dlg.ShowDialog();
                string DirectoryPath = System.IO.Path.GetDirectoryName(dlg.SelectedPath);
                string[] A = Directory.GetFiles(DirectoryPath, "*.mp3", SearchOption.AllDirectories);
                string[] fName = new string[A.Count()];
                var col = db.GetCollection<MusicFromFolder>("music");

                for (int i = 0; i < A.Count(); i++)
                {
                    fName[i] = System.IO.Path.GetFileName(A[i]);

                    var audio = new MusicFromFolder
                    {
                        Title = fName[i],
                        Path = A[i]
                    };

                    col.Insert(audio);
                        
                }
            }
            DisplayPresetData();
        }

        private void StoreVideo(object sender, RoutedEventArgs e)
        {
            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                dlg.ShowDialog();
                string DirectoryPath = System.IO.Path.GetDirectoryName(dlg.SelectedPath);
                string[] A = Directory.GetFiles(DirectoryPath, "*.mp4", SearchOption.AllDirectories);
                string[] fName = new string[A.Count()];
                var col = db.GetCollection<MusicFromFolder>("video");

                for (int i = 0; i < A.Count(); i++)
                {
                    fName[i] = System.IO.Path.GetFileName(A[i]);

                    var audio = new MusicFromFolder
                    {
                        Title = fName[i],
                        Path = A[i]
                    };

                    col.Insert(audio);

                }
            }
            DisplayPresetData();
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
            
        }
    }
}
