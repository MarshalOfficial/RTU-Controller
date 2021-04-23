
namespace SerialSample.DBLayer
{
    /// <summary>
    /// جدول دستگاه ها
    /// </summary>
    public class DeviceEntity
    {
        [SQLite.Net.Attributes.PrimaryKey]
        public int ID { get; set; }
        public string Name { get; set; }
        public int DeviceType { get; set; }
        public int LocationID { get; set; }
        public string Address { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string GoogleAddress { get; set; }
        public int RowNow { get; set; }
        public int? ConnectedDeviceID { get; set; }
        public decimal? Balance { get; set; }
    }
}
    