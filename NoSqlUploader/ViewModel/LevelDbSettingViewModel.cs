using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using LevelDB;
using PropertyTools.DataAnnotations;

namespace NoSqlUploader.ViewModel
{
    public class LevelDbSettingViewModel : INotifyPropertyChanged, INoSql
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string baseDirPath;
        [System.ComponentModel.Category("NoSql连接设定")]
        [System.ComponentModel.DisplayName("LeveldbConnection路径")]
        [DirectoryPath]
        public string DirectoryPath
        {
            get { return baseDirPath; }
            set
            {
                if (baseDirPath != value)
                {
                    baseDirPath = value;
                    if (db!=null)
                    {
                        db.Close();
                    }
                    db = null;

                    RaisePropertyChanged("DirectoryPath");
                }

            }
        }
        [PropertyTools.DataAnnotations.Browsable(false)]
        public string Name { get; set; } = "LevelDb";
        private DB db;
        public bool Connect()
        {
            if (db != null)
            {
                return true;
            }
            try
            {
                var options = new Options { CreateIfMissing = true };
                db = new DB(options, baseDirPath);
                Connected = db != null;
            }
            catch (Exception e)
            {
                Connected = false;
            }
            return Connected;
        }
        [PropertyTools.DataAnnotations.Browsable(false)]
        public bool Connected { get; private set; }


        public bool SetKeyValue(string key, byte[] value)
        {
            try
            {
                var keyBytes = Encoding.UTF8.GetBytes(key);
                db.Put(keyBytes, value);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return true;
        }

        public byte[] GetValue(string key)
        {
            return Encoding.UTF8.GetBytes(db.Get(key));
        }
    }
}