using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPClient
{
    class File:INotifyPropertyChanged
    {
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this,new PropertyChangedEventArgs("Name"));
                }
            }
        }
        private string size;
        public string Size
        {
            get { return size; }
            set
            {
                size = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Size"));
                }
            }
        }
        private double percentage;

        public double Percentage
        {
            get { return percentage; }
            set
            {
                percentage = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Percentage"));
                }
            }
        }
        private Boolean isUpLoad;
        public Boolean IsUpLoad
        {
            get { return isUpLoad; }
            set {
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("IsUpLoad"));
                }
            }
        }
        
        public File(string name, string size, double percentage,Boolean isupload)
        {
            this.Name = name;
            this.Size = size;
            this.Percentage = percentage;
            this.IsUpLoad = isupload;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public override Boolean Equals(Object obj)
        {
            File other = (File)obj;
            if (other.Name == this.Name)
                return true;
            return false;
        }
    }
}
