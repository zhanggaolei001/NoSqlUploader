using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.CompilerServices;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace NoSqlUploader.ViewModel
{
    public class FileData
    {
        public string Key { get; set; }
        public byte[] Value { get; set; }
    }
    public class MongoDBSettingViewModel : INotifyPropertyChanged, INoSql
    {
        private string url = @"mongodb://localhost";
        //  @"mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass%20Community&ssl=false";
        private string dbName = "test";

        [System.ComponentModel.Category("MongoDB连接设定")]
        [PropertyTools.DataAnnotations.DisplayName("服务地址:")] 
        public string Url
        {
            get { return url; }
            set
            {
                url = value;
                RaisePropertyChanged("Url");
            }
        }
        private IMongoCollection<FileData> GetUserCollection(string name)
        {
            return db.GetCollection<FileData>(name);  //传入集合的名字
        }
        [System.ComponentModel.Category("MongoDB连接设定")]
        [PropertyTools.DataAnnotations.DisplayName("Db名称:")]
        public string DbName
        {
            get { return dbName; }
            set
            {
                dbName = value;
                RaisePropertyChanged("DbName");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        [PropertyTools.DataAnnotations.Browsable(false)]
        public string Name { get; set; } = "MongoDb";
        [PropertyTools.DataAnnotations.Browsable(false)]
        public MongoClient Client { get; set; }

        private IMongoDatabase db;
        public bool Connect()
        {
            try
            {
                Client = new MongoClient(Url);
                //Client.Settings.ConnectTimeout = new TimeSpan(1000);
                Client.StartSession();
                db = Client.GetDatabase(DbName);

                if (db != null)
                {
                    Connected = true;
                }
                else
                {
                    Connected = false;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e; 
            }
            return Connected;
        }

        [PropertyTools.DataAnnotations.Browsable(false)]
        public bool Connected { get; set; }

        [System.ComponentModel.Category("MongoDB连接设定")]
        [PropertyTools.DataAnnotations.DisplayName("Collection名称:")]
        public string CollectionName { get; set; } = "Files";
        [System.ComponentModel.Category("MongoDB连接设定")]
        [PropertyTools.DataAnnotations.DisplayName("大文件模式:")]
        public bool BigFileMode { get; set; }


        public bool SetKeyValue(string key, byte[] value)
        {
            if (BigFileMode)
            {
                var bucket = new GridFSBucket(db); //这个是初始化gridFs存储的
                var id = bucket.UploadFromBytes(key, value); //source字节数组
                return !string.IsNullOrEmpty(id.ToString());
            }
            else
            {
                try
                {
                    var col = db.GetCollection<FileData>(CollectionName);
                    col.InsertOne(new FileData() { Key = key, Value = value }); //插入一条数据
                    return true;
                }
                catch
                {
                    throw;
                }
            }
        }

        public byte[] GetValue(string key)
        {
            var col = db.GetCollection<FileData>(CollectionName);
            var data = col.Find((t) => t.Key == key).Limit(1);
            return data.First().Value;
            //查询全部集合里的数据 var result1 = col.FindAllAs<Users>();
        }
    }
}