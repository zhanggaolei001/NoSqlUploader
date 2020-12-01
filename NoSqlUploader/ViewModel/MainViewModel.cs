using Couchbase;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PropertyTools.DataAnnotations;

namespace NoSqlUploader.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            CouchBaseSettingViewModel = new CouchBaseSettingViewModel();
            MongoDbSettingViewModel = new MongoDbSettingViewModel();
            LevelDbSettingViewModel = new LevelDbSettingViewModel();
            NoSqlSettings = new List<INoSql>() { CouchBaseSettingViewModel, MongoDbSettingViewModel, LevelDbSettingViewModel };
            SelectedNoSqlSetting = NoSqlSettings.FirstOrDefault();
            this.DirectoryPath = Properties.Settings.Default.FileBaseDir;
        }
        [PropertyTools.DataAnnotations.Browsable(false)]
        public List<INoSql> NoSqlSettings { get; set; }
        [PropertyTools.DataAnnotations.Browsable(false)]
        public INoSql SelectedNoSqlSetting
        {
            get => selectedNosqlSetting;
            set
            {
                selectedNosqlSetting = value;
                RaisePropertyChanged("SelectedNosqlSetting");
            }
        }

        private string fileBaseDir;
        [System.ComponentModel.Category("上传文件设定")]
        [PropertyTools.DataAnnotations.DisplayName("文件夹地址：")]
        [DirectoryPath]
        public string DirectoryPath
        {
            get { return fileBaseDir; }
            set
            {
                if (fileBaseDir != value)
                {
                    MainViewModel.files = null;
                    fileBaseDir = value;
                    RaisePropertyChanged("DirectoryPath");
                }
            }
        }
        [PropertyTools.DataAnnotations.Browsable(false)]
        MongoDbSettingViewModel MongoDbSettingViewModel { get; set; }
        [PropertyTools.DataAnnotations.Browsable(false)]
        CouchBaseSettingViewModel CouchBaseSettingViewModel { get; set; }
        LevelDbSettingViewModel LevelDbSettingViewModel { get; set; }
        private RelayCommand coneectCommand;
        [PropertyTools.DataAnnotations.Browsable(false)]
        public RelayCommand ConnectCommand
        {
            get
            {
                return coneectCommand
                    ?? (coneectCommand = new RelayCommand(
                    () =>
                    {
                        if (Connect())
                        {
                            MessageBox.Show("连接成功");
                        }
                        else
                        {
                            MessageBox.Show("连接失败！！！");
                        }
                    }));
            }
        }

        private bool Connect()
        {
            try
            {
                return SelectedNoSqlSetting.Connect();
            }
            catch (Exception e)
            {
                Info = e.Message;
                return false;
            }
        }

        private string info;
        [PropertyTools.DataAnnotations.Browsable(false)]
        public string Info
        {
            get { return info; }
            set
            {

                info += (DateTime.Now.ToShortDateString() + ":" + value + "\r\n");
                if (info.Length > 50000)
                {
                    info = info.Substring(5000);
                }
                RaisePropertyChanged("Info");
            }
        }
        private int left;
        [PropertyTools.DataAnnotations.Browsable(false)]
        public int Left
        {
            get { return left; }
            private set
            {
                left = value;
                RaisePropertyChanged("Left");
            }
        }
        private int offSet;
        [System.ComponentModel.Category("上传文件设定")]
        [PropertyTools.DataAnnotations.DisplayName("跳过已上传:")]
        public int OffSet
        {
            get { return offSet; }
            set
            {
                offSet = value;
                RaisePropertyChanged("OffSet");
            }
        }
        private int total;
        [System.ComponentModel.Category("上传文件设定")]
        [PropertyTools.DataAnnotations.DisplayName("文件总数量:")]
        public int Total
        {
            get { return total; }
            private set
            {
                total = value;
                RaisePropertyChanged("Total");
            }
        }
        private bool reScan;
        [System.ComponentModel.Category("上传文件设定")]
        [PropertyTools.DataAnnotations.DisplayName("是否重新扫描:")]
        public bool Rescan
        {
            get { return reScan; }
            set
            {
                reScan = value;
                RaisePropertyChanged("Rescan");
            }
        }
        private bool saveIndex;
        [System.ComponentModel.Category("上传文件设定")]
        [PropertyTools.DataAnnotations.DisplayName("是否保存上传记录:")]

        public bool SaveIndex
        {
            get { return saveIndex; }
            set { saveIndex = value; }
        }

        public static List<FileInfo> files;
        private RelayCommand uploadCommand;
        [PropertyTools.DataAnnotations.Browsable(false)]
        /// <summary>
        /// Gets the CopyFileToCouchBaseCommand.
        /// </summary>
        public RelayCommand UploadCommand
        {
            get
            {
                return uploadCommand
                    ?? (uploadCommand = new RelayCommand(
                    () =>
                    {
                        try
                        {
                            this.Run = true;
                            Task.Factory.StartNew(() =>
                            {
                                if (!SelectedNoSqlSetting.Connected)
                                {
                                    if (!SelectedNoSqlSetting.Connect())
                                    {
                                        Info = $"{SelectedNoSqlSetting.Name}连接失败!";
                                        return;
                                    }
                                }

                                string path = this.DirectoryPath;

                                if (files == null || Rescan)
                                {
                                    files = new List<FileInfo>();
                                    Info = "扫描目标文件夹...";
                                    GetAllFilesAndSubFiles(path, ref files);
                                }

                                Total = files.Count;
                                Left = files.Count;
                                Info = $"共有{Total}个文件,剩余{Total - OffSet}.";

                                var pathLength = path.Length + 1;
                                if (SaveIndex)
                                {
                                    var logPath = "uploaded\\";
                                    if (!Directory.Exists(logPath))
                                    {
                                        Directory.CreateDirectory(logPath);
                                    }
                                    var logFileName = $"{logPath}\\{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.log";
                                    Info = $"保存扫描结果:文件:{logFileName}...";
                                    using (StreamWriter streamWriter = new StreamWriter(logFileName, true, Encoding.UTF8))
                                    {
                                        foreach (var file in files)
                                        {
                                            var contents = file.FullName.Substring(pathLength).Replace("\\", "/") + "\r\n";
                                            streamWriter.Write(contents);
                                        }
                                    }
                                    Info = $"保存完成";
                                }
                                var fileQueue = new Queue<FileInfo>(files);
                                for (int i = 0; i < OffSet; i++)
                                {
                                    fileQueue.Dequeue();
                                }

                                for (int i = 0; i < 1; i++)
                                {
                                    int punishSleep = 100;
                                    FileInfo file = null;
                                    while (fileQueue.Count > 0 && Run)
                                    {
                                        file = fileQueue.Dequeue();
                                        try
                                        {
                                            var document = new Document<Byte[]>
                                            {
                                                Id = file.FullName.Substring(pathLength)
                                                    .Replace("\\", "/"), //此处处理:ID为相对path加filename
                                                Content = File.ReadAllBytes(file.FullName)
                                            };
                                            Console.WriteLine(document.Id);
                                            Info = document.Id;
                                            Left = fileQueue.Count;
                                            var key = file.FullName.Substring(pathLength).Replace("\\", "/");//此处处理:ID为相对path加filename
                                            var value = File.ReadAllBytes(file.FullName);
                                            while (!SelectedNoSqlSetting.SetKeyValue(key, value) && Run)
                                            {
                                                punishSleep += 100;
                                                Info = file.Name + $"上传失败，{punishSleep / 1000.0}s后尝试重新上传";
                                                Thread.Sleep(punishSleep);
                                            }

                                            if (punishSleep > 100)
                                            {
                                                punishSleep -= 100;
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            var d = DateTime.Now;
                                            var logpath =
                                                $"error_log\\{d.Year}\\{d.Month}\\{d.Day}\\{d.Hour}_{d.Minute}";
                                            Log(logpath, $"上传发生异常：{file.Name},[{e.Message}]\r\n{e.StackTrace}");
                                        }
                                    }
                                    Info = "over";
                                }
                                Run = false;
                            });

                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, e.StackTrace);
                            Run = false;
                        }
                    }));
            }
        }


        private void GetAllFilesAndSubFiles(string path, ref List<FileInfo> files)
        {
            Info = $"扫描文件夹:{path}";
            DirectoryInfo root = new DirectoryInfo(path);
            files.AddRange(root.GetFiles());
            Total = files.Count;
            var subDirs = root.GetDirectories();
            foreach (var subDir in subDirs)
            {
                GetAllFilesAndSubFiles(subDir.FullName, ref files);
            }
        }

        [System.ComponentModel.Browsable(false)]
        public bool Run
        {
            get => _run;
            set
            {
                _run = value;
                CanStart = !value;
                RaisePropertyChanged("Run");
            }
        }
        private bool canStart = true;
        [PropertyTools.DataAnnotations.Browsable(false)]
        public bool CanStart
        {
            get { return canStart; }
            set { canStart = value; RaisePropertyChanged("CanStart"); }
        }

        private RelayCommand _stopCommand;

        [PropertyTools.DataAnnotations.Browsable(false)]
        /// <summary>
        /// Gets the StopCommand.
        /// </summary>
        public RelayCommand StopCommand
        {
            get
            {
                return _stopCommand
                    ?? (_stopCommand = new RelayCommand(
                    () =>
                    {
                        if (Run)
                        {
                            Run = false;
                            this.OffSet = Total - Left - 2;
                        }
                        else
                        {
                            Info = "未在执行,无需停止.";
                        }

                    }));
            }
        }

        private INoSql selectedNosqlSetting;
        private bool _run = false;


        void Log(string logpath, string msg)
        {
            Info = msg;
            var d = DateTime.Now;

            FileInfo fi = new FileInfo(logpath);
            var di = fi.Directory;
            if (!di.Exists)
                di.Create();
            StreamWriter sw = new StreamWriter(logpath, true);
            sw.WriteLine(msg);
            sw.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}