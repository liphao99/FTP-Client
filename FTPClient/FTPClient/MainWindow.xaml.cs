using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

namespace FTPClient
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

        }
        #endregion

        #region On Loaded

        /// <summary>
        /// when the application first opens
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            // Get every logical drive on the machine
            /*foreach (var drive in Directory.GetLogicalDrives())
            {
                // Creat a new item for it
                var item = new TreeViewItem()
                {
                    // Set the header and the full path
                    Header = drive,
                    Tag = drive
                };


                // Add a dummy item
                item.Items.Add(null);

                // Listen out for item being expanded
                item.Expanded += Folder_Expanded;

                // Add it to the main treeView
                FolderView.Items.Add(item);
            }*/
           // var drive = "ftp://192.168.139.1/";  
            var drive = "C:\\";
            var item = new TreeViewItem()
            {
                // Set the header and the full path
                Header = drive,
                Tag = drive
            };


            // Add a dummy item
            item.Items.Add(null);

            // Listen out for item being expanded
            item.Expanded += Folder_Expanded;

            // Add it to the main treeView
            FolderView.Items.Add(item);
        }

        #endregion
        #region Folder Expanded
        /// <summary>
        /// when a folder is expanded, find the sub folders/files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            #region Initail Checks

            var item = (TreeViewItem)sender;

            // If the item only contains the dummy data
            if (item.Items.Count != 1 || item.Items[0] != null)
                return;

            // Clear the dummy data
            item.Items.Clear();

            // Get folder name
            var fullPath = (string)item.Tag;

            #endregion

            #region Get Folders

            // Create a blank list for directories
            var directories = new List<string>();

            // Try and get directories from folder
            //ignoring any issues doing so
            try
            {
                var dirs = Directory.GetDirectories(fullPath);
                if (dirs.Length > 0)
                    directories.AddRange(dirs);
            }
            catch { }

            // For each directories
            directories.ForEach(directoryPath =>
            {
                // Create directory item
                var subItem = new TreeViewItem()
                {
                    // Set header as folder name
                    Header = GetFileFolderName(directoryPath),
                    // And tag as full path
                    Tag = directoryPath
                };

                // Add dummy item so we can expand folder
                subItem.Items.Add(null);

                // Handle expanding
                subItem.Expanded += Folder_Expanded;

                // Add the item to the parent
                item.Items.Add(subItem);

            });

            #endregion

            #region Get Files
            // Create a blank list for files
            var files = new List<string>();

            // Try and get files from folder
            //ignoring any issues doing so
            try
            {
                var fs = Directory.GetFiles(fullPath);
                if (fs.Length > 0)
                    files.AddRange(fs);
            }
            catch { }

            // For each file...
            files.ForEach(filePath =>
            {
                // Create file item
                var subItem = new TreeViewItem()
                {
                    // Set header as file name
                    Header = GetFileFolderName(filePath),
                    // And tag as full path
                    Tag = filePath
                };

                // Add the item to the parent
                item.Items.Add(subItem);

            });

            #endregion
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Find a file or a folder name from a full path
        /// </summary>
        /// <param name="directoryPath">The full path</param>
        /// <returns></returns>
        public static string GetFileFolderName(string path)
        {
            // C:\Something\a folder
            // C:\Something\a file.png

            if (string.IsNullOrEmpty(path))
                return string.Empty;

            // Make all slashes backslashes
            var nomalizedPath = path.Replace('/', '\\');

            // Find the last backslash in the path
             var lastIndex = nomalizedPath.LastIndexOf('\\');
            //var lastIndex = path.LastIndexOf('\\');
            // if we don't find a backflash, return the path itself
            if (lastIndex <= 0)
                return path;

            // Return the name after the last backflash
            return path.Substring(lastIndex + 1);
        }
        #endregion

        private void portNum_TextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void conBtn(object sender, RoutedEventArgs e)//连接按钮
        {
            string port = portNum.Text.ToString();
            MessageBox.Show(port);
        }

        private void upClick(object sender, RoutedEventArgs e)//上传按钮
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择要上传的文件";
            fileDialog.InitialDirectory = @"C:\";//设置打开目录的初始位置
            //fileDialog.Filter=
            fileDialog.RestoreDirectory = true;
            if (fileDialog.ShowDialog() == true)
            {
                //textBox1.Text = System.IO.Path.GetFileNameWithoutExtension(fileDialog.FileName);
                System.IO.Path.GetFullPath(fileDialog.FileName); //绝对路径
                System.IO.Path.GetExtension(fileDialog.FileName); //文件扩展名
                System.IO.Path.GetFileNameWithoutExtension(fileDialog.FileName); //文件名没有扩展名
                System.IO.Path.GetFileName(fileDialog.FileName); //得到文件
                System.IO.Path.GetDirectoryName(fileDialog.FileName); //得到路径
            }

            //System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            //if(folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    MessageBox.Show(folderBrowser.SelectedPath);
            //}
        }

        private void turnBack(object sender, RoutedEventArgs e)//返回上级目录
        {

        }

        private void downClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择要下载的文件";
            fileDialog.InitialDirectory = "ftp://192.168.139.1/";//打开服务器根目录
            //fileDialog.Filter=
            fileDialog.RestoreDirectory = true;
            if (fileDialog.ShowDialog() == true)
            {
                //textBox1.Text = System.IO.Path.GetFileNameWithoutExtension(fileDialog.FileName);
                System.IO.Path.GetFullPath(fileDialog.FileName); //绝对路径
                System.IO.Path.GetExtension(fileDialog.FileName); //文件扩展名
                System.IO.Path.GetFileNameWithoutExtension(fileDialog.FileName); //文件名没有扩展名
                System.IO.Path.GetFileName(fileDialog.FileName); //得到文件
                System.IO.Path.GetDirectoryName(fileDialog.FileName); //得到路径
            }
        }
    }
}
