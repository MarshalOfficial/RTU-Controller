using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialSample.DBLayer
{
    /// <summary>
    /// جدول دستورالعمل ها
    /// </summary>
    public class InstructionEntity
    {
        [SQLite.Net.Attributes.PrimaryKey]
        public int ID { get; set; }
        public int DeviceType { get; set; }
        public string Memo { get; set; }
        public int FunctionCode { get; set; }
        public int CRC1 { get; set; }
        public int CRC2 { get; set; }
        public string Data { get; set; }
        public string Formula { get; set; }
        public int? ChildInstructionID { get; set; }
        public int? UntilMin { get; set; }
        public int? ChildType { get; set; }
        public int? GPIO { get; set; }
        public int? GPIOValue { get; set; }
        public bool IEEE { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string UnitName { get; set; }
        public string SecondFormula { get; set; }
        public int? ParentID { get; set; }      
        public int? FromByte { get; set; }
        public int? ToByte { get; set; }
        public bool IsEffectOnBalance { get; set; }
        public int? ConnectedDeviceIDPositive { get; set; }
        public int? ConnectedDeviceInstructionIDPositive { get; set; }
        public int? ConnectedDeviceIDNegative { get; set; }
        public int? ConnectedDeviceInstructionIDNegative { get; set; }
    }
}