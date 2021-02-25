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

        [System.ComponentModel.Category("MongoDB�����趨")]
        [PropertyTools.DataAnnotations.DisplayName("�����ַ:")] 
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
            return db.GetCollection<FileData>(name);  //���뼯�ϵ�����
        }
        [System.ComponentModel.Category("MongoDB�����趨")]
        [PropertyTools.DataAnnotations.DisplayName("Db����:")]
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

        [System.ComponentModel.Category("MongoDB�����趨")]
        [PropertyTools.DataAnnotations.DisplayName("Collection����:")]
        public string CollectionName { get; set; } = "Files";
        [System.ComponentModel.Category("MongoDB�����趨")]
        [PropertyTools.DataAnnotations.DisplayName("���ļ�ģʽ:")]
        public bool BigFileMode { get; set; }


        public bool SetKeyValue(string key, byte[] value)
        {
            if (BigFileMode)
            {
                var bucket = new GridFSBucket(db); //����ǳ�ʼ��gridFs�洢��
                var id = bucket.UploadFromBytes(key, value); //source�ֽ�����
                return !string.IsNullOrEmpty(id.ToString());
            }
            else
            {
                try
                {
                    var col = db.GetCollection<FileData>(CollectionName);
                    col.InsertOne(new FileData() { Key = key, Value = value }); //����һ������
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
            //��ѯȫ������������� var result1 = col.FindAllAs<Users>();
        }
    }
}