
namespace SerialSample.DBLayer
{
    /// <summary>
    /// جدول نوع دستگاه ها
    /// </summary>
    public class DeviceTypeEntity
    {
        [SQLite.Net.Attributes.PrimaryKey]
        public int ID { get; set; }
        public string Name { get; set; }
        public string ModelNo { get; set; } 
    }
}
