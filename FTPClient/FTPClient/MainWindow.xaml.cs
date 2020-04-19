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
using FTPUtil;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;


namespace FTPClient
{


    //public class Upload_files
    //{
    //    public string Name;
    //    public double Size;
    //    public int Percentage;
    //}

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //public ObservableCollection<Upload_files> Upload_files_list = new ObservableCollection<Upload_files>();

        private FTP folderFtp;//用于浏览服务端文件
        private FTP mainFtp;//用于文件传输

        private Queue<TransferCommand> readyQueue;//准备开始传输的队列

        private Queue<TransferCommand> waitingQueue;//暂停传输的队列

        private List<File> files = new List<File>();//显示在传输列表中的文件
        

        private String currentFolder = null;//记录文件树中，最新展开的目录路径
        private String currentServerFolder = null;//记录服务端文件树中，最新展开的目录路径

        private ManualResetEvent manual;//当传输队列为空时，使传输线程阻塞，节省资源的开销
        private Thread transferThread;//传输线程
        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MainWindow()
        {
            readyQueue = new Queue<TransferCommand>();
            waitingQueue = new Queue<TransferCommand>();
            manual = new ManualResetEvent(false);
            transferThread = new Thread(new ThreadStart(LoopTransfer));
            transferThread.Start();

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
            foreach (var drive in Directory.GetLogicalDrives())
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
            }
            //var drive = "ftp://192.168.139.1/";  
            //var drive = "C:\\";
            //var item = new TreeViewItem()
            //{
            //    // Set the header and the full path
            //    Header = drive,
            //    Tag = drive
            //};


            //// Add a dummy item
            //item.Items.Add(null);

            //// Listen out for item being expanded
            //item.Expanded += Folder_Expanded;

            //// Add it to the main treeView
            //FolderView.Items.Add(item);
            
            //listView.Items.Add(new File("OverWarch", 1120, 5.0));
            //ListViewItem[] lvs = new ListViewItem[2];
            //lvs[0] = new ListViewItem ();
            //lvs[1] = new ListViewItem();
            //this.listView.Items.Add(lvs);
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
            currentFolder = item.Tag.ToString();
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

        //right click local file item show menu
        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
           if(treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while(source != null && source.GetType() != typeof(T))
            {
                source = VisualTreeHelper.GetParent(source);
            }
            return source;
        }

        private void LoopTransfer()
        {
            while (true)
            {
                if (readyQueue.Count == 0)
                {
                    Console.WriteLine("wait one");
                    manual.WaitOne();
                }
                TransferCommand transfer = readyQueue.Peek();
                Console.WriteLine("start transfer: " + transfer.Source);
                transfer.Execute();
                readyQueue.Dequeue();
                Console.WriteLine("将需要删除文件对应传输队列");
                //TODO:移除传输界面的该项传输信息
                string size = CountSize((long)transfer.Size);
                //this.listView.Items.Remove();
                File file = new File(transfer.Source,CountSize((long)transfer.Size),0, false);
                //Thread thread = new Thread(new ParameterizedThreadStart(updateFiles));
                //thread.Start(file);
                //TaskScheduler taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                Task.Factory.StartNew(()=>
                {
                    for (int i = 0; i < files.Count(); i++)
                    {
                        File f = files[i];
                        if (f.Equals(file))
                        {
                            files.Remove(f);
                            Console.WriteLine("removed file in list");
                            listView.Dispatcher.Invoke(new Action(() =>
                            {
                                this.listView.Items.Remove(f);

                            }));
                            
                            Console.WriteLine("removed file in listView" + f.Name);
                        }
                    }
                    Thread.Sleep(500);
                });

            }
        }
        //线程方法，修改files和listView内容
        /*private void updateFiles(Object obj)
        {
            File file = (File)obj;
            Console.WriteLine("file in thread"+file.Name);

            while (true)
            {
                Action action1 = () =>
                  {
                      for (int i = 0; i < files.Count(); i++)
                      {
                          Console.WriteLine("in for loop");
                          File f = files[i];
                          if (f.Equals(file))
                          {
                              files.Remove(f);
                              Console.WriteLine("removed file"+f.Name);
                              this.listView.Items.Remove(f);
                              
                          }
                      }
                  };
                Thread.Sleep(500);
            }
        }*/

        //click button in Menu to upload file to server
        private void uptoServer_Click(object sender, RoutedEventArgs e)
        {
            string path="";
            try
            {
                var node = FolderView.SelectedItem as TreeViewItem;
                path = node.Tag.ToString();//选中文件的路径
            }catch(Exception excep)
            {
                MessageBox.Show(excep.Message);
            }

            if (currentServerFolder == null)
            {
                currentServerFolder = "/";
            }
            TransferCommand upload = new UploadCommand(mainFtp, path, currentServerFolder);
            readyQueue.Enqueue(upload);
            if(readyQueue.Count == 1)
            {
                manual.Set();
                manual.Reset();
            }
            //在传输队列视图中更新相关的控件
            long size = (long)upload.Size;

            File file = new File(path, CountSize(size), 0, true);
            this.listView.Items.Add(file);
            this.files.Add(file);
            //for(int i=0;i<files.Count();i++)
            //{
            //    File f = files[i];
            //    if (f.Equals(file))
            //    {
            //        files.Remove(f);
            //        this.listView.Items.Remove(f);
            //    }
            //}




            //MessageBox.Show(path);
            //this.listView.Items.Add(new File(path, CountSize(GetFileSize(path)), 0));
        }

        //click to download
        private void downtoLocal_Click(object sender, RoutedEventArgs e)
        {
            string path = "";
            try
            {
                var node = ServerFolderView.SelectedItem as TreeViewItem;
                path = node.Tag.ToString();//选中文件的路径
            }catch(Exception excep)
            {
                MessageBox.Show(excep.Message);
            }

            if (currentFolder == null)
            {
                currentFolder = "C:\\";
            }
            TransferCommand download = new DownloadCommand(mainFtp, path, currentFolder);
            readyQueue.Enqueue(download);
            if (readyQueue.Count == 1)
            {
                manual.Set();
                manual.Reset();
            }
            //在传输队列视图中更新相关的控件
            long size = (long)download.Size;
            this.listView.Items.Add(new File(path, CountSize(size), 0, false));

            //MessageBox.Show(path);
            //this.listView.Items.Add(new File(path, CountSize(GetFileSize(path)), 0));
        }


        private void conBtn(object sender, RoutedEventArgs e)//连接按钮
        {
            string host = hostNum.Text.ToString();
            string usrname = name.Text.ToString();
            string password = psw.Password;
            string port = portNum.Text.ToString();
            try
            {
                if (usrname.Equals("") || password.Equals(""))
                {
                    folderFtp = new FTP(host, Int32.Parse(port));
                    mainFtp = new FTP(host, Int32.Parse(port));
                }
                else
                {
                    folderFtp = new FTP(host, Int32.Parse(port), usrname, password);
                    mainFtp = new FTP(host, Int32.Parse(port), usrname, password);
                }
                InitServerFolder();
            }catch(Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        /*private void upClick(object sender, RoutedEventArgs e)//上传按钮
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

                this.listView.Items.Add(new File(fileDialog.FileName, CountSize(GetFileSize(fileDialog.FileName)), 0));
            }

            //Upload_files_list.Add(new Upload_files
            //{
            //    Name = fileDialog.FileName,
            //    Size = 50,
            //    Percentage = 50
            //});

            //System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            //if(folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    MessageBox.Show(folderBrowser.SelectedPath);
            //}
        }*/

        private void Item_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            File clickedItem = button.DataContext as File;
            UploadCommand upLoadCommand = null;
            DownloadCommand downLoadCommand = null; 


            if (button.Content.ToString() == "开始")//重新开始上传
            {
                button.Content = "暂停";
                clickedItem.Percentage += 10;
                if (clickedItem.IsUpLoad)
                {
                    upLoadCommand.Execute();
                }
                else
                {
                    downLoadCommand.Execute();
                }
            }            
            else if (button.Content.ToString() == "暂停")//暂停上传
            {
                button.Content = "开始";
                clickedItem.Percentage -= 10;
                if (clickedItem.IsUpLoad)//文件正在被上传
                {
                    upLoadCommand = (upLoadCommand == null ? new UploadCommand(mainFtp, clickedItem.Name, currentFolder):upLoadCommand);
                    upLoadCommand = (UploadContinue)upLoadCommand.Abort();
                }
                else//文件正在被下载
                {
                    downLoadCommand = (downLoadCommand == null? new DownloadCommand(mainFtp, clickedItem.Name, currentFolder):downLoadCommand);
                    downLoadCommand=(DownloadContinue)downLoadCommand.Abort();
                }
            }   
        }

        /*private void downClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择要下载的文件";
            fileDialog.InitialDirectory = "ftp://192.168.0.105/";//打开服务器根目录
            //fileDialog.Filter=
            fileDialog.RestoreDirectory = true;
            FileInfo fileInfo;
            if (fileDialog.ShowDialog() == true)
            {
                //textBox1.Text = System.IO.Path.GetFileNameWithoutExtension(fileDialog.FileName);
                System.IO.Path.GetFullPath(fileDialog.FileName); //绝对路径
                System.IO.Path.GetExtension(fileDialog.FileName); //文件扩展名
                System.IO.Path.GetFileNameWithoutExtension(fileDialog.FileName); //文件名没有扩展名
                System.IO.Path.GetFileName(fileDialog.FileName); //得到文件
                System.IO.Path.GetDirectoryName(fileDialog.FileName); //得到路径
                fileInfo = new FileInfo(fileDialog.FileName);

                this.listView.Items.Add(new File(fileDialog.FileName, CountSize(GetFileSize(fileDialog.FileName)), 0));
            }
            
        }*/
        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="sFullName"></param>
        /// <returns></returns>
        public static long GetFileSize(string sFullName)
        {
            long lSize = 0;
            lSize = new FileInfo(sFullName).Length;
            return lSize;
        }

        /// <summary>
        /// 计算文件大小函数(保留两位小数),Size为字节大小
        /// </summary>
        /// <param name="Size">初始文件大小</param>
        /// <returns></returns>
        public static string CountSize(long Size)
        {
            string m_strSize = "";
            long FactSize = 0;
            FactSize = Size;
            if (FactSize < 1024.00)
                m_strSize = FactSize.ToString("F2") + " Byte";
            else if (FactSize >= 1024.00 && FactSize < 1048576)
                m_strSize = (FactSize / 1024.00).ToString("F2") + " K";
            else if (FactSize >= 1048576 && FactSize < 1073741824)
                m_strSize = (FactSize / 1024.00 / 1024.00).ToString("F2") + " M";
            else if (FactSize >= 1073741824)
                m_strSize = (FactSize / 1024.00 / 1024.00 / 1024.00).ToString("F2") + " G";
            return m_strSize;
        }

        private void ServerFolder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem parent = (TreeViewItem)sender;
            currentServerFolder = parent.ToString();
            // If the item only contains the dummy data
            if (parent.Items.Count != 1 || parent.Items[0] != null)
                return;

            
            // Clear the dummy data
            parent.Items.Clear();

            // Get folder name
            String fullPath = (string)parent.Tag;

            //TODO:先试试同步的加载文件夹效果如何，不行再换多线程
            ListCommand cmd = new ListCommand(folderFtp, fullPath);
            cmd.Execute();
            
            foreach (List<String> dir in cmd.Directories)
            {
                TreeViewItem item = new TreeViewItem()
                {
                    Header = dir[3],
                    Tag = fullPath + "/" + dir[3]
                };
                item.Items.Add(null);
                item.Expanded += ServerFolder_Expanded;
                parent.Items.Add(item);
            }
            foreach (List<String> file in cmd.Files)
            {
                TreeViewItem item = new TreeViewItem()
                {
                    Header = file[3],
                    Tag = fullPath + "/" + file[3]
                };
                parent.Items.Add(item);
            }

        }

        private void InitServerFolder()
        {
            ListCommand cmd = new ListCommand(folderFtp, "/");
            cmd.Execute();
            ServerFolderView.Items.Clear();
            foreach(List<String> dir in cmd.Directories)
            {
                TreeViewItem item = new TreeViewItem()
                {
                    Header = dir[3],
                    Tag = "/"+dir[3]
                };
                item.Items.Add(null);
                item.Expanded += ServerFolder_Expanded;
                ServerFolderView.Items.Add(item);
            }
            foreach(List<String> file in cmd.Files)
            {
                TreeViewItem item = new TreeViewItem()
                {
                    Header = file[3],
                    Tag = "/" + file[3]
                };
                ServerFolderView.Items.Add(item);
            }
        }

        private void updateLists()
        {

        }
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        
    }
}
