using SerialSample.DBLayer;
using System.Collections.Generic;
using System.Linq;

namespace THTController.DBLayer
{
    /// <summary>
    /// این کلاس جهت نگهداری تمامی اطلاعات اولیه مورد نیاز کنترلر موقع اجرا می باشد
    /// به این دلیل که در فرم داشبورد تردهای پردازشی همزمان مختلفی داریم که همه نیاز به دسترسی به اطلاعات دیتابیس لوکال رسپری و اطلاعات اولیه رو دارند
    /// و دسترسی به دیتابیس اسکیوالایت از چند ترد خطا میده و اطلاعات اولیه هم قرار نیست تغییر کنند 
    /// پس در لود فرم داشبورد همه اطلاعات اولیه را لود میکنیم و در آبجکتی از جنس کلاس زیر نگهداری میکنیم در طول باز بودن فرم داشبورد
    /// </summary>
    public class CacheEntity
    {
        public List<UserEntity> Users { get; set; }
        public List<SiteEntity> Sites { get; set; }
        public List<DeviceTypeEntity> DeviceTypes { get; set; }
        public List<DashboardPriorityEntity> DashboardPriorities { get; set; }
        public List<DeviceEntity> Devices { get; set; }
        public List<InstructionEntity> Instructions { get; set; }
        public List<ResultEntity> Results { get; set; }
        public List<DeviceDashboardItemEntity> DashboardItems { get; set; }
        public List<DashboardScenarioEntity> DashboardScenarios { get; set; }
        public SettingEntity Setting { get; set; }
        public string GetAllDeviceIDs()
        {
            var lst = Devices.Where(l => l.LocationID == Setting.SiteID).ToList();
            var value = "";
            foreach (var item in lst)
            {
                value += item.ID + ",";
            }
            return (value != null && value.Length > 0) ? value.TrimEnd(',') : value;
        }
        public List<DashboardLogEntity> DashboardLogs { get; set; }
    }
}