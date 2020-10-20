using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialSample.DBLayer
{

    /// <summary>
    /// جدول پاسخ ها
    /// </summary>
    public class ResultEntity
    {
        [SQLite.Net.Attributes.PrimaryKey]
        public int ID { get; set; }
        public int DeviceType { get; set; }
        public string Memo { get; set; }
        public int FunctionCode { get; set; }
        public int CRC1 { get; set; }
        public int CRC2 { get; set; }
        public string Data { get; set; }
    }
}
