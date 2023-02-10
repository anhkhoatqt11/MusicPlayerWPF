using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
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
        }

        public class PathFile
        {
            public LiteDB.ObjectId _id { get; set; }
            public string Path { get; set; }
            public string Type { get; set; }
        }

        private void StoreMusic(object sender, RoutedEventArgs e)
        {
            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string DirectoryPath = dlg.SelectedPath;
                    var colpath = db.GetCollection<PathFile>("path");
                    var existingPath = colpath.FindOne(x => x.Path == DirectoryPath);
                    if (existingPath == null)
                    {
                        var PatchF = new PathFile
                        {
                            Path = dlg.SelectedPath,
                            Type = "Audio",
                        };
                        colpath.Insert(PatchF);
                        var col = db.GetCollection<MusicFromFolder>("music");
                        GetFilesRecursiveMP3(DirectoryPath, col);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("The selected path already exists in the database.");
                    }
                }
            }
            DisplayPathData();
        }

        private void GetFilesRecursiveMP3(string directory, ILiteCollection<MusicFromFolder> col)
        {
            try
            {
                string[] supportedMusicExtensions = new[] { ".mp3", ".wma", ".m4a", ".flac", ".wav" };
                string[] files = Directory.GetFiles(directory);
                foreach (string file in files)
                {
                    string extension = System.IO.Path.GetExtension(file).ToLower();
                    if (supportedMusicExtensions.Contains(extension))
                    {
                        string fileName = System.IO.Path.GetFileName(file);
                        var audio = new MusicFromFolder
                        {
                            Title = fileName,
                            Path = file
                        };
                        col.Insert(audio);
                    }
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
                if (dlg.ShowDialog() == DialogResult.OK) { 
                    string DirectoryPath = dlg.SelectedPath;
                var colpath = db.GetCollection<PathFile>("path");
                var PatchF = new PathFile
                {
                    Path = dlg.SelectedPath,
                    Type = "Video",
                };
                colpath.Insert(PatchF);
                var col = db.GetCollection<MusicFromFolder>("video");
                GetFilesRecursiveMP4(DirectoryPath, col);
            }
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
                        Path = file,
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

        private void UpdateMusicListFromSavedPath(object sender, RoutedEventArgs e)
        {
            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                var pathCollection = db.GetCollection<PathFile>("path");
                var paths = pathCollection.FindAll();

                var musicCollection = db.GetCollection<MusicFromFolder>("music");

                // Remove all existing music records
                var musicfind = musicCollection.FindAll();
                foreach (var item in musicfind)
                {
                    musicCollection.Delete(item._id);
                }


                // Iterate through all saved paths
                foreach (var path in paths)
                {
                    if (Directory.Exists(path.Path))
                    {
                        // Get all mp3 files in the directory
                        var files = Directory.GetFiles(path.Path, "*.mp3", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            string fileName = System.IO.Path.GetFileName(file);
                            var music = new MusicFromFolder
                            {
                                Title = fileName,
                                Path = file
                            };
                            musicCollection.Insert(music);
                        }
                    }
                    else
                    {
                        // Remove the path from the database if it no longer exists
                        pathCollection.Delete(path._id);
                    }
                }
            }
        }


        public void UpdateMusicList()
        {
            using (var db = new LiteDatabase(@"C:\Temp\MyData.db"))
            {
                var pathCollection = db.GetCollection<PathFile>("path");
                var paths = pathCollection.FindAll();

                var musicCollection = db.GetCollection<MusicFromFolder>("music");

                // Remove all existing music records
                var musicfind = musicCollection.FindAll();
                foreach (var item in musicfind)
                {
                    musicCollection.Delete(item._id);
                }


                // Iterate through all saved paths
                foreach (var path in paths)
                {
                    if (Directory.Exists(path.Path))
                    {
                        // Get all mp3 files in the directory
                        var files = Directory.GetFiles(path.Path, "*.mp3", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            string fileName = System.IO.Path.GetFileName(file);
                            var music = new MusicFromFolder
                            {
                                Title = fileName,
                                Path = file
                            };
                            musicCollection.Insert(music);
                        }
                    }
                    else
                    {
                        // Remove the path from the database if it no longer exists
                        pathCollection.Delete(path._id);
                    }
                }
            }
        }

        private void PathListDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "_id")
            {
                e.Column.Visibility = Visibility.Collapsed;
            }
            if (e.PropertyName == "Path")
            {
                e.Column.Width = new DataGridLength(650);
            }


     ((DataGridTextColumn)e.Column).ElementStyle = FindResource("DataGridRowWrapStyle") as Style;
        }
    }
}