using System;
namespace THTController.DBLayer
{
    /// <summary>
    /// جدول سناریوها
    /// </summary>
    public class DashboardScenarioEntity
    {
        [SQLite.Net.Attributes.PrimaryKey]
        public int ID { get; set; }
        public int DashboardItemID { get; set; }
        public int ConnectedDeviceID { get; set; }
        public int InstructionID { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
        public TimeSpan? FromTime { get; set; } 
        public TimeSpan? ToTime { get; set; }   
    }
}