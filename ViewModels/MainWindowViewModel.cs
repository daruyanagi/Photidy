using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Photidy.ViewModels
{
    using Photidy.Models;

    class MainWindowViewModel: NotifyPropertyCgangedBase
    {
        private AppSettings model;
        private readonly string path = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, "settings");

        public MainWindowViewModel()
        {
            model = Load();

            if (model.SrcFolders == null)
            {
                model.SrcFolders = new List<string>
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures),
                    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                };
            }

            AddSrcFoldersCommand = new RelayCommand(_ =>
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    var path = dialog.ShowDialog() == DialogResult.OK
                        ? dialog.SelectedPath
                        : string.Empty;
                    if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                    {
                        model.SrcFolders.Add(path);
                        OnPropertyChanged("SrcFolders");
                    }
                }
            });

            DeleteSrcFoldersCommand = new RelayCommand(_ =>
            {
                model.SrcFolders.RemoveAt(SelectedIndex);
                OnPropertyChanged("SrcFolders");
            },
            _ => SelectedItem != null);

            SelectDestFolderCommand = new RelayCommand(_ =>
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    var path = dialog.ShowDialog() == DialogResult.OK
                        ? dialog.SelectedPath
                        : string.Empty;
                    DestFolder = path;
                }
            });

            CopyCommand = new RelayCommand(_ =>
            {
                model.OperationMode = OperationMode.Copy;

                var window = new ProcessWindow();
                var view_model = new ProcessWindowViewModel(model);

                window.Closing += (s, a) =>
                {
                    view_model.Quit();
                };

                view_model.Done += (s, a) =>
                {
                    window.Close();
                };

                window.DataContext = view_model;
                window.Owner = App.Current.MainWindow;
                window.ShowDialog();
            },
            _ => !string.IsNullOrEmpty(DestFolder) && Directory.Exists(DestFolder));

            MoveCommand = new RelayCommand(_ =>
            {
                model.OperationMode = OperationMode.Move;

                var window = new ProcessWindow();
                var view_model = new ProcessWindowViewModel(model);

                window.Closing += (s, a) =>
                {
                    view_model.Quit();
                };

                view_model.Done += (s, a) =>
                {
                    window.Close();
                };

                window.DataContext = view_model;
                window.Owner = App.Current.MainWindow;
                window.ShowDialog();
            },
            _ => !string.IsNullOrEmpty(DestFolder) && Directory.Exists(DestFolder));
        }

        ~MainWindowViewModel()
        {
            Save();
        }

        public AppSettings Load()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(AppSettings));
                var reader = new StreamReader(path, new UTF8Encoding(false));
                model = (AppSettings)serializer.Deserialize(reader);
                reader.Close();
                return model;
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void Save()
        {
            var serializer = new XmlSerializer(typeof(AppSettings));
            var writer = new StreamWriter(path, false, new UTF8Encoding(false));
            serializer.Serialize(writer, model);
            writer.Close();
        }

        public List<string> SrcFolders
        {
            get
            {
                return model.SrcFolders.ToList();
            }
            set
            {
                if (model.SrcFolders == value) return;

                model.SrcFolders = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get
            {
                var n = Application.ProductName;
                var v = Application.ProductVersion;
                var c = Application.CompanyName;

                return string.Format("{0} v{1} by {2}", n, v, c);
            }
        }

        public string DestFolder
        {
            get
            {
                return model.DestFolder;
            }
            set
            {
                if (model.DestFolder == value) return;

                model.DestFolder = value;
                OnPropertyChanged();
            }
        }

        public string SearchPattern
        {
            get
            {
                return model.SearchPattern;
            }
            set
            {
                if (model.SearchPattern == value) return;

                model.SearchPattern = value;
                OnPropertyChanged();
            }
        }

        public string FilenameSample
        {
            get
            {
                try
                {
                    var path = Assembly.GetExecutingAssembly().Location;

                    path = Path.GetDirectoryName(path);
                    path = Path.Combine(path, "Sample.jpg");

                    return Utilities.GenerateNewFileNameByExifDateTime(@"C:\", path, FilenamePattern);
                }
                catch
                {
                    return "Invalid Pattern.";
                }
            }
        }

        public string FilenamePattern
        {
            get
            {
                return model.FileNamePattern;
            }
            set
            {
                if (model.FileNamePattern == value) return;

                model.FileNamePattern = value;
                OnPropertyChanged();
                OnPropertyChanged("FilenameSample");
            }
        }

        public int Total { get; set; } = 100;
        public int Processed { get; set; } = 0;
        public int SelectedIndex { get; set; }
        public string SelectedItem { get; set; }

        public RelayCommand AddSrcFoldersCommand { get; private set; }
        public RelayCommand DeleteSrcFoldersCommand { get; private set; }
        public RelayCommand SelectDestFolderCommand { get; private set; }
        public RelayCommand CopyCommand { get; private set; }
        public RelayCommand MoveCommand { get; private set; }
    }
}
