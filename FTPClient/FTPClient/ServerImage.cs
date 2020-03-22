using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FTPClient
{
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class ServerImage:IValueConverter
    {
        public static ServerImage Instance = new ServerImage();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Get the full path
            var path = (string)value;

            //If the path is null
            if (path == null)
                return null;

            // Get the name of the file/folder
            var name = MainWindow.GetFileFolderName(path);
            string ext = Path.GetExtension(path);
            string fileName = Path.GetFileNameWithoutExtension(path);

            Console.WriteLine(name);
            // By default, we persume a file
            var image = "Images/file.png";

            FileSystemInfo fsi = new FileInfo(path);
            //If the name is blank, we persume is a drive as we cannot have a blank file or folder name
            if (string.IsNullOrEmpty(name))
                image = "Images/drive.png";
            //else if (new FileInfo(path).Attributes.HasFlag(FileAttributes.Directory))
            //else if ((fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            else if(ext==""||fileName=="")
                image = "Images/folder.png";

            return new BitmapImage(new Uri($"pack://application:,,,/{image}"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((IValueConverter)Instance).ConvertBack(value, targetType, parameter, culture);
        }
    }
}
