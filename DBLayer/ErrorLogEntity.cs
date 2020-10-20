using SQLite.Net.Attributes;
namespace THTController.DBLayer
{
    /// <summary>
    /// جدول خطاهای سیستم
    /// </summary>
    public class ErrorLogEntity
    {
        [SQLite.Net.Attributes.PrimaryKey, AutoIncrement]
        public long ID { get; set; }
        public string Ip { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }  
    }
}
