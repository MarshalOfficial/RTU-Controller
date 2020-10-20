using SerialSample.DBLayer;
namespace THTController.DBLayer
{
    /// <summary>
    /// این کلاس نوع تشکیل دهنده صف اصلی برنامه در داشبورد می باشد
    /// یعنی در داشبورد صفی داریم شامل یک سری آبجکت از نوع شات
    /// حالا این آبجکت شات ممکنه توش درخواست زماندارش پر باشه یا لوکال فایر باشه یا از جنس فایر سایت
    /// </summary>
    public class Shot
    {
        public Shot(DeviceDashboardItemEntity dashboarditem)
        {
            DashboardItem = dashboarditem;
        }
        public Shot(DeviceInstructionFireEntity instructionFire)    
        {
            InstructionFire = instructionFire;
        }
        public Shot(LocalFireEntity localFire)
        {
            LocalFire = localFire;
        }
        public DeviceDashboardItemEntity DashboardItem { get; set; }
        public DeviceInstructionFireEntity InstructionFire { get; set; }
        public LocalFireEntity LocalFire { get; set; }  
    }

    /// <summary>
    /// این کلاس نوع فایرهای لوکال رو تعریف میکنه که چه پراپرتی هایی دارد
    /// </summary>
    public class LocalFireEntity
    {
        public int DeviceID { get; set; }
        public int InstructionID { get; set; }
        public string Value { get; set; }
        public int? ResultID { get; set; }
        public string Type { get; set; } // scenario or localfire
    }
}
