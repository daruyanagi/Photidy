using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photidy
{
    using System.Drawing;
    using System.IO;

    public static class Utilities
    {
        public static string GenerateNewFileNameByExifDateTime(string base_dir, string filename, string pattern)
        {
            if (!File.Exists(filename)) throw new FileNotFoundException();

            using (var bitmap = new Bitmap(filename))
            {
                const int EXIF_DATETIME = 0x9003;

                int index = Array.IndexOf(bitmap.PropertyIdList, EXIF_DATETIME);

                if (index == -1) throw new Exception("Exif is not found.");
                
                var property = bitmap.PropertyItems[index];

                // 2015:09:05 22:31:45\0 形式で取得できるので、バラして DateTime にする
                var s = Encoding.ASCII.GetString(property.Value) as string;
                var p = s.Trim('\0').Split(' ', ':').Select(_ => int.Parse(_)).ToArray();
                var datetime = new DateTime(p[0], p[1], p[2], p[3], p[4], p[5]);
                
                pattern = pattern.Replace("{filename}", "{1}");
                pattern = pattern.Replace("{fileext}", "{2}");
                pattern = pattern.Replace("{y", "{0:y");
                pattern = pattern.Replace("{M", "{0:M");
                pattern = pattern.Replace("{d", "{0:d");
                pattern = pattern.Replace("{h", "{0:h");
                pattern = pattern.Replace("{m", "{0:m");
                pattern = pattern.Replace("{s", "{0:s");

                return Path.Combine(base_dir, string.Format(pattern, datetime, Path.GetFileName(filename), Path.GetExtension(filename)));
            }
        }
    }
}
