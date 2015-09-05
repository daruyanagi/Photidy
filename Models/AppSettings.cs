using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Photidy.Models
{
    public enum OperationMode
    {
        Copy,
        Move,
    }

    public class AppSettings
    {
        public List<string> SrcFolders { get; set; }

        public OperationMode OperationMode { get; set; }
        
        public string DestFolder { get; set; }
        
        public string SearchPattern { get; set; } = "*.png;*.jpg";
        
        public string FileNamePattern { get; set; } = @"{yyyy}-{MM}-{dd}\{filename}";
    }
}
