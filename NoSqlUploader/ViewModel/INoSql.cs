using System.IO;

namespace NoSqlUploader.ViewModel
{
    public interface INoSql
    {
        string Name { get; set; }
        bool Connect();
        bool Connected { get; } 
        bool SetKeyValue(string key,byte[] value);
        byte[] GetValue(string key);
    }
}