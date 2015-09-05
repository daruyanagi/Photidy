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

namespace Photidy
{
    using System.IO;
    using System.Drawing;
    using System.Drawing.Imaging;
    using ViewModels;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel();
        }

        private DateTime? GetDateTime(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException();

            using (var bitmap = new Bitmap(filename))
            {
                const int EXIF_DATETIME = 0x9003;

                int index = Array.IndexOf(bitmap.PropertyIdList, EXIF_DATETIME);
                if (index == -1) return null; // Photo does not have datetime Exif.

                var property = bitmap.PropertyItems[index];

                var value = Encoding.ASCII.GetString(property.Value) as string;
                value = value.Trim('\0');
                var p = value.Split(' ', ':').Select(_ => int.Parse(_)).ToArray();

                return new DateTime(p[0], p[1], p[2], p[3], p[4], p[5]);
            }
        }

        private string GetFilePathFromDateTime(string filename)
        {

            if (!File.Exists(filename))
                throw new FileNotFoundException();

            var dest = @"C:\Users\Hidetoshi\OneDrive\画像\";
            var datetime = GetDateTime(filename);

            if (datetime == null)
            {
                return Path.Combine(dest, Path.GetFileName(filename));
            }
            else
            {
                return Path.Combine(dest, string.Format(@"{0:yyyy-mm-dd}\{1}", datetime.Value, Path.GetFileName(filename)));
            }
        }

        private string[] GetFiles(string[] sources)
        {
            var list = new List<string>();

            foreach (var source in sources)
            {
                list.AddRange(Directory.GetFileSystemEntries(source, "*", SearchOption.AllDirectories));
            }

            return list.ToArray();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var sources = new string[] { @"C:\Users\Hidetoshi\OneDrive\画像\カメラ ロール\", };

            //textBlock1.Text = GetFilePathFromDateTime(@"C:\Users\Hidetoshi\OneDrive\画像\カメラ ロール\20150325_060743227_iOS.jpg").ToString();
            //textBlock2.Text = GetFiles(sources).Count().ToString();
        }
    }
}
