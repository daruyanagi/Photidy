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

    public class AppSettings : INotifyPropertyChanged
    {
        private List<string> _SrcFolders;
        public List<string> SrcFolders
        {
            get
            {
                return _SrcFolders;
            }
            set
            {
                SetPropety(ref _SrcFolders, value);
            }
        }

        private OperationMode _OperationMode;
        public OperationMode OperationMode
        {
            get
            {
                return _OperationMode;
            }
            set
            {
                SetPropety(ref _OperationMode, value);
            }
        }

        private string _DestFolder;
        public string DestFolder
        {
            get
            {
                return _DestFolder;
            }
            set
            {
                SetPropety(ref _DestFolder, value);
            }
        }

        private string _SearchPattern = "*.png;*.jpg";
        public string SearchPattern
        {
            get
            {
                return _SearchPattern;
            }
            set
            {
                SetPropety(ref _SearchPattern, value);
            }
        }

        private string _FileNamePattern = @"{yyyy}-{MM}-{dd}\{filename}";
        public string FileNamePattern
        {
            get
            {
                return _FileNamePattern;
            }
            set
            {
                SetPropety(ref _FileNamePattern, value);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetPropety<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
