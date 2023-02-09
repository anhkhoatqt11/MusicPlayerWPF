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
            DisplayPathData();
        }


        public class MusicFromFolder
        {
            public LiteDB.ObjectId _id { get; set; }
            public String Title { get; set; }
            public String Path { get; set; }
            public bool IsShuffle { get; set; } = false;
            public bool IsLoop { get; set; } = false;
        }

        public class PathFile
        {
            public LiteDB.ObjectId _id { get; set; }
            public string Path { get; set; }
        }

        private void StoreMusic(object sender, RoutedEventArgs e)
        {
            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                dlg.ShowDialog();
                string DirectoryPath = dlg.SelectedPath;
                var colpath = db.GetCollection<PathFile>("path");
                var PatchF = new PathFile
                {
                    Path = dlg.SelectedPath,
                };
                colpath.Insert(PatchF);
                var col = db.GetCollection<MusicFromFolder>("music");
                GetFilesRecursiveMP3(DirectoryPath, col);
            }
            DisplayPathData();
        }

        private void GetFilesRecursiveMP3(string directory, ILiteCollection<MusicFromFolder> col)
        {
            try
            {
                string[] files = Directory.GetFiles(directory, "*.mp3");
                foreach (string file in files)
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    var audio = new MusicFromFolder
                    {
                        Title = fileName,
                        Path = file
                    };
                    col.Insert(audio);
                }
                string[] directories = Directory.GetDirectories(directory, "*");
                foreach (string subDirectory in directories)
                {
                    if (!IsSystemFolder(subDirectory))
                    {
                        GetFilesRecursiveMP3(subDirectory, col);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Do nothing, just skip this directory
            }
            catch (ArgumentException)
            {
                // Do nothing, just skip this directory
            }
        }



        private void StoreVideo(object sender, RoutedEventArgs e)
        {
            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                dlg.ShowDialog();
                string DirectoryPath = dlg.SelectedPath;
                var colpath = db.GetCollection<PathFile>("path");
                var PatchF = new PathFile
                {
                    Path = dlg.SelectedPath,
                };
                colpath.Insert(PatchF);
                var col = db.GetCollection<MusicFromFolder>("video");
                GetFilesRecursiveMP4(DirectoryPath, col);
            }
            DisplayPathData();
        }

        private void GetFilesRecursiveMP4(string directory, ILiteCollection<MusicFromFolder> col)
        {
            try
            {
                string[] files = Directory.GetFiles(directory, "*.mp4");
                foreach (string file in files)
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    var audio = new MusicFromFolder
                    {
                        Title = fileName,
                        Path = file
                    };
                    col.Insert(audio);
                }
                string[] directories = Directory.GetDirectories(directory, "*");
                foreach (string subDirectory in directories)
                {
                    if (!IsSystemFolder(subDirectory))
                    {
                        GetFilesRecursiveMP4(subDirectory, col);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Do nothing, just skip this directory
            }
            catch (ArgumentException)
            {
                // Do nothing, just skip this directory
            }
        }


        private bool IsSystemFolder(string directory)
        {
            string[] systemFolders = new string[] { "System Volume Information", "$Recycle.Bin", "Program Files" };
            string directoryName = System.IO.Path.GetFileName(directory);
            return systemFolders.Contains(directoryName);
        }

        //private void StoreVideo(object sender, RoutedEventArgs e)
        //{
        //    using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
        //    {
        //        FolderBrowserDialog dlg = new FolderBrowserDialog();
        //        dlg.ShowDialog();

        //        string DirectoryPath = dlg.SelectedPath;
        //        var colpath = db.GetCollection<PathFile>("path");
        //        var PatchF = new PathFile
        //        {
        //            Path = dlg.SelectedPath,
        //        };
        //        colpath.Insert(PatchF);
        //        if (DirectoryPath != null)
        //        {
        //            string[] A = Directory.GetFiles(DirectoryPath, "*.mp4", SearchOption.AllDirectories);
        //            string[] fName = new string[A.Count()];
        //            var col = db.GetCollection<MusicFromFolder>("video");

        //            for (int i = 0; i < A.Count(); i++)
        //            {
        //                fName[i] = System.IO.Path.GetFileName(A[i]);

        //                var audio = new MusicFromFolder
        //                {
        //                    Title = fName[i],
        //                    Path = A[i]
        //                };

        //                col.Insert(audio);

        //            }
        //        }
        //    }
        //}

        private List<PathFile> GetAllPathFolder()
        {
            var list = new List<PathFile>();
            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                var col = db.GetCollection<PathFile>("path");
                foreach (PathFile _id in col.FindAll())
                {
                    list.Add(_id);
                }
            }
            return list;
        }

        public void DisplayPathData()
        {
            PathList.ItemsSource = GetAllPathFolder();
        }

        private void DelPath(object sender, RoutedEventArgs e)
        {
            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                var selectedRow = (PathFile)PathList.SelectedItem;
                string DirectoryPath = selectedRow.Path;

                var colMusic = db.GetCollection<MusicFromFolder>("music");
                var colVideo = db.GetCollection<MusicFromFolder>("video");
                var colPath = db.GetCollection<PathFile>("path");
                var queryMusic = colMusic.Find(x => x.Path.StartsWith(DirectoryPath));
                var queryVideo = colVideo.Find(x => x.Path.StartsWith(DirectoryPath));
                var queryPath = colPath.Find(x => x.Path.StartsWith(DirectoryPath));

                foreach (var audio in queryMusic)
                {
                    colMusic.Delete(audio._id);
                }
                foreach (var video in queryVideo)
                {
                    colVideo.Delete(video._id);
                }

                foreach (var path in queryPath)
                {
                    colPath.Delete(path._id);
                }
            }
            DisplayPathData();
        }
    }
}