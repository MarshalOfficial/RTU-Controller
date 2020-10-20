using System;
namespace SerialSample.DBLayer
{
    /// <summary>
    /// جدول درخواست های زماندار
    /// </summary>
    public class DeviceDashboardItemEntity
    {
        [SQLite.Net.Attributes.PrimaryKey]
        public int ID { get; set; }
        public int DeviceID { get; set; }
        public int InstructionID { get; set; }
        public string Value { get; set; }
        public DateTime? SaveTime { get; set; }
        public int? Priority { get; set; }  
        public int? ResultID { get; set; }   
    }
}
