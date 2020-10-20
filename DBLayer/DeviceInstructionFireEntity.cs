using System;
namespace SerialSample.DBLayer
{
    /// <summary>
    /// جدول فایرهای سایت
    /// </summary>
    public class DeviceInstructionFireEntity
    {
        [SQLite.Net.Attributes.PrimaryKey]
        public int ID { get; set; }
        public int DeviceID { get; set; }
        public int InstructionID { get; set; }
        public int UserID { get; set; }
        public DateTime? SendTime { get; set; }
        public bool IsProcess { get; set; }
        public string Value { get; set; }
        public DateTime? ResultTime { get; set; }
        public int? ResultID { get; set; }  
    }
}
