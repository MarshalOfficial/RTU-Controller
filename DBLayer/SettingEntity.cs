using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace THTController.DBLayer
{
    /// <summary>
    /// جدول تنظیمات
    /// </summary>
    public class SettingEntity
    {
        [SQLite.Net.Attributes.PrimaryKey]
        public int ID { get; set; }
        public int SiteID { get; set; }
        public string SQLServerIP { get; set; } 
    }
}
