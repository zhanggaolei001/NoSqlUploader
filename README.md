# NoSqlUploader
一个文件上传到NoSQL数据库的工具
目前支持MongoDB,LevelDB,CouchBase.
主要对不同NoSQL数据库的配置及操作进行了封装.
上传时会火力全开,吃满IO,如果出错,会等待1秒,连续出错等待时间会以指数速度增加(暂称惩罚性等待),当上传成功后,还原到1秒.这个主要解决CouchBase或者MongoDB在缓存吃满后持久化速度跟不上接收程序新请求时的情况.
UI使用WPF开发,引用了Mahaps.Matro,PropertyTool.wpf,以及几个NoSQL数据库的驱动.
