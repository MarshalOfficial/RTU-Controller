using SQLite.Net.Attributes;
using System;
namespace THTController.DBLayer
{
    /// <summary>
    /// جدول لاگ ها
    /// </summary>
    public class DashboardLogEntity
    {
        [SQLite.Net.Attributes.PrimaryKey, AutoIncrement]
        public long ID { get; set; }
        public int? DashboardItemID { get; set; }
        public int? InstructionID { get; set; }
        public string Value { get; set; }
        public DateTime SaveTime { get; set; }
        public int? ResultID { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsSended { get; set; }
        public string Description { get; set; } 
    }
    public class DashboardLogEntityUI : DashboardLogEntity
    {
        public string InstructionName { get; set; }
        public string SaveDateFa { get { return Date.Methods.GregorianToShamshiDateWithTime(SaveTime); } }
        public string CreateDateFa { get { return Date.Methods.GregorianToShamshiDateWithTime(CreateDate); } }
        public string ResultName { get; set; }

        public DashboardLogEntityUI(DashboardLogEntity parentToCopy)
        {
            this.ID = parentToCopy.ID;
            this.DashboardItemID = parentToCopy.DashboardItemID;
            this.InstructionID = parentToCopy.InstructionID;
            this.Value = parentToCopy.Value;
            this.SaveTime = parentToCopy.SaveTime;
            this.ResultID = parentToCopy.ResultID;
            this.CreateDate = parentToCopy.CreateDate;
            this.IsSended = parentToCopy.IsSended;
            this.InstructionName = string.Empty;
            this.ResultName = string.Empty;
            this.Description = parentToCopy.Description;
        }
    }
}