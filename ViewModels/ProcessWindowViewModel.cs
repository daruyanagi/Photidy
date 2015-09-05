
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photidy.ViewModels
{
    using Photidy.Models;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Windows;

    public class ProcessWindowViewModel: NotifyPropertyCgangedBase
    {
        private AppSettings model { get; set; }
        private string pattern;
        private BackgroundWorker worker;

        public ProcessWindowViewModel(AppSettings model)
        {
            this.model = model;
            this.pattern = model.FileNamePattern;

            pattern = pattern.Replace("{filename}", "{1}");
            pattern = pattern.Replace("{fileext}", "{2}");
            pattern = pattern.Replace("{y", "{0:y");
            pattern = pattern.Replace("{M", "{0:M");
            pattern = pattern.Replace("{d", "{0:d");
            pattern = pattern.Replace("{h", "{0:h");
            pattern = pattern.Replace("{m", "{0:m");
            pattern = pattern.Replace("{s", "{0:s");

            var files = GetFiles(model.SrcFolders.ToArray());

            Total = files.Length;
            Processed = 0;

            worker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true,
            };

            worker.DoWork += (sender, args) =>
            {
                foreach (var old_filename in files)
                {
                    if ((sender as BackgroundWorker).CancellationPending)
                    {
                        args.Cancel = true; return;
                    }

                    try
                    {
                        Processed++;

                        var new_filename = GenerateNewFileNameByExifDateTime(model.DestFolder, old_filename);

                        Source = old_filename;
                        Destination = new_filename;

                        if (old_filename != new_filename)
                        {
                            var dir = Path.GetDirectoryName(new_filename);

                            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                            switch (model.OperationMode)
                            {
                                case OperationMode.Copy:
                                    File.Copy(old_filename, new_filename);
                                    break;

                                case OperationMode.Move:
                                    File.Move(old_filename, new_filename);
                                    break;
                            }
                        }
                        else
                        {
                            throw new Exception("Exif is not found.");
                        }

                        System.Diagnostics.Debug.WriteLine(Processed);
                        System.Diagnostics.Debug.WriteLine(old_filename + " -> " + new_filename);
                    }
                    catch (Exception e)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            errors.Insert(0, e.Message + ": " + old_filename);
                        });
                    }
                }
            };
                
            worker.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Cancelled)
                {
                    // MessageBox.Show(model.OperationMode.ToString() + " is cancelled.", "Complete!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(model.OperationMode.ToString() + " is done.", "Complete!", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                if (Done != null) Done(this, EventArgs.Empty);
            };

            worker.RunWorkerAsync();
        }

        public void Quit()
        {
            worker.CancelAsync();
            // worker.Dispose();
        }

        private string[] GetFiles(string[] sources)
        {
            var list = new List<string>();
            var extensions = model.SearchPattern.Split(';').Select(_ => _.Trim()).ToArray();

            foreach (var source in sources)
            {
                try
                {
                    foreach (var extension in extensions)
                    {
                        list.AddRange(Directory.GetFiles(source, extension, SearchOption.AllDirectories));
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    MessageBox.Show(e.Message);
                }
            }

            return list.ToArray();
        }

        private string GenerateNewFileNameByExifDateTime(string base_dir, string filename)
        {

            if (!File.Exists(filename))
                throw new FileNotFoundException();

            using (var bitmap = new Bitmap(filename))
            {
                const int EXIF_DATETIME = 0x9003;

                int index = Array.IndexOf(bitmap.PropertyIdList, EXIF_DATETIME);

                if (index == -1) // Exif データがなかった -> そのままのファイル名を返す
                {
                    return filename;
                    // return Path.Combine(base_dir, Path.GetFileName(filename));
                }
                else // Exif データがあった -> Filename Pattern で加工したファイル名を返す
                {
                    var property = bitmap.PropertyItems[index];

                    // 2015:09:05 22:31:45\0 形式で取得できるので、バラして DateTime にする
                    var s = Encoding.ASCII.GetString(property.Value) as string;
                    var p = s.Trim('\0').Split(' ', ':').Select(_ => int.Parse(_)).ToArray();
                    var datetime = new DateTime(p[0], p[1], p[2], p[3], p[4], p[5]);

                    return Path.Combine(base_dir, string.Format(pattern, datetime, Path.GetFileName(filename), Path.GetExtension(filename)));
                }
            }
        }

        private ObservableCollection<string> errors = new ObservableCollection<string>();

        public ObservableCollection<string> Errors
        {
            get { return errors; }
            set { SetPropety(ref errors, value); }
        }

        public event EventHandler Done;

        public string Title
        {
            get { return string.Format("Processing ({0} / {1})", processed, total); }
        }

        private int total = 100;

        public int Total
        {
            get { return total; }
            set { SetPropety(ref total, value); OnPropertyChanged("Title"); }
        }

        private int processed = 100;

        public int Processed
        {
            get { return processed; }
            set { SetPropety(ref processed, value); OnPropertyChanged("Title"); }
        }

        private string source = string.Empty;

        public string Source
        {
            get { return source; }
            set { SetPropety(ref source, value); }
        }

        private string destination = string.Empty;

        public string Destination
        {
            get { return destination; }
            set { SetPropety(ref destination, value); }
        }
    }
}
