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

namespace KMusic.Pages
{
    /// <summary>
    /// Interaction logic for Music.xaml
    /// </summary>
    public partial class Music : Page
    {
        public static class Global
        {
            public static WaveOutEvent waveOut;
        }
        public Music()
        {
            InitializeComponent();
            DisplayPresetData();
            HideColumn();
        }
        public class MusicFromFolder
        {
            public String Title { get; set; }
            public String Path { get; set; }
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
            DataGrid dataGrid = sender as DataGrid;
            MusicFromFolder selectedRow = dataGrid.SelectedItem as MusicFromFolder;
            string cellValue = selectedRow.Path;
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;


            var file = TagLib.File.Create(cellValue);
            var albumArt = file.Tag.Pictures.FirstOrDefault();

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
                Global.waveOut.Init(audioFile);
                Global.waveOut.Play();

                mainWindow.UpdateTitleAndArtist(title, artist);
                mainWindow.UpdateAudioFile(audioFile);
                mainWindow.ChangePlayPauseButton();
            }
            else
            {
                MessageBox.Show("Please select a row in the DataGrid.");
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
                e.Column.Width = new DataGridLength(750);
            }
            if (e.PropertyName == "Path")
            {
                e.Column.Visibility = Visibility.Collapsed;
            }

             ((DataGridTextColumn)e.Column).ElementStyle = FindResource("DataGridRowWrapStyle") as Style;
        }

    }
}
