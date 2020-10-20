namespace THTController.DBLayer
{
    /// <summary>
    /// جدول اولویت زمانی درخواست های زمان دار
    /// مثلا 3 ثانیه 10 ثانیه 900 ثانیه و هر ردیف دیگری
    /// </summary>
    public class DashboardPriorityEntity
    {
        [SQLite.Net.Attributes.PrimaryKey]
        public long ID { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }  
    }
}
