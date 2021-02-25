using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using Couchbase;
using Couchbase.Authentication;
using Couchbase.Configuration.Client;
using Couchbase.Core;

namespace NoSqlUploader.ViewModel
{
    public class CouchBaseSettingViewModel : INotifyPropertyChanged, INoSql
    {
        [PropertyTools.DataAnnotations.Browsable(false)]
        public bool Connected { get; set; }

        public bool SetKeyValue(string key, byte[] value)
        {
            var document = new Document<Byte[]>
            {
                Id = key,
                Content = value
            };
            return Bucket.Upsert(document).Success;
        }

        public byte[] GetValue(string key)
        {
            var r = Bucket.Query<KeyValuePair<string, byte[]>>(key);
            return r.Rows[0].Value;
        }

        public CouchBaseSettingViewModel()
        {
            GetDefaultData();
        }
        private IBucket bucket;
        [PropertyTools.DataAnnotations.Browsable(false)]
        public IBucket Bucket
        {
            get { return bucket; }
            set { bucket = value; }
        }

        private string url;
        [System.ComponentModel.Category("CouchBase连接设定")]
        [PropertyTools.DataAnnotations.DisplayName("服务地址:")]
        [DataType(DataType.Url)]
        public string Url
        {
            get { return url; }
            set
            {
                url = value;
                RaisePropertyChanged("Url");
            }
        }
        private string bucketName;
        [System.ComponentModel.Category("CouchBase连接设定")]
        [PropertyTools.DataAnnotations.DisplayName("Bucket名称:")]
        public string BucketName
        {
            get { return bucketName; }
            set
            {
                bucketName = value;
                RaisePropertyChanged("BucketName");
            }
        }
        private string userName;
        [System.ComponentModel.Category("CouchBase连接设定")]
        [PropertyTools.DataAnnotations.DisplayName("用户名:")]
        public string UserName
        {
            get { return userName; }
            set
            {
                userName = value;
                RaisePropertyChanged("UserName");
            }
        }
        private string password;
        [System.ComponentModel.Category("CouchBase连接设定")]
        [PropertyTools.DataAnnotations.DisplayName("密码:")]
        [DataType(DataType.Password)]
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                RaisePropertyChanged("Password");
            }
        }


        private void GetDefaultData()
        {
            try
            {
                this.BucketName = Properties.Settings.Default.BucketName;
                this.UserName = Properties.Settings.Default.UserName;
                this.Password = Properties.Settings.Default.Password;
                this.Url = Properties.Settings.Default.Url;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        [PropertyTools.DataAnnotations.Browsable(false)]
        public string Name { get; set; } = "CouchBase";

        public bool Connect()
        {
            try
            {
                this.Bucket = null;
                var cluster = new Cluster(new ClientConfiguration
                {
                    Servers = new List<Uri> { new Uri(Url) }
                });

                var authenticator = new PasswordAuthenticator(UserName, Password);
                cluster.Authenticate(authenticator);
                Bucket = cluster.OpenBucket(BucketName, Password);
                Connected = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace, e.Message);
                Connected = false;
            }
            return Connected;
        }
    }
}