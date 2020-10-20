using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialSample.DBLayer
{
    /// <summary>
    /// وظیفه این کلاس استاتیک دانلود اطلاعات از سرور مرکزی به کنترلر است
    /// اطلاعات از قبیل لیست مجتمع ها ، دستگاه ها ، دستورالعمل ها ، کاربران و دیگر اطلاعات اولیه مورد نیاز 
    /// </summary>
    public static class DTS
    {
        private static DatabaseHelperClass localdb;
        private static SqlServerRepository sqlsrv;

        public static void Init(string _sqlip)
        {
            try
            {
                sqlsrv = new SqlServerRepository(_sqlip);
                localdb = new DatabaseHelperClass(_sqlip);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool FullDownload()
        {
            try
            {
                GetAllUsers();
                GetAllSites();
                GetAllDeviceTypes();
                GetAllDashboardPriority();
                GetAllDevices();
                GetAllInstruction();
                GetAllResults();
                GetAllDeviceDashboardItems();
                GetAllDashboardScenario();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public static void GetAllSites()
        {
            try
            {
                var lst = sqlsrv.DownloadAllSites();
                if(lst != null && lst.Count > 0)
                {
                    localdb.DeleteAllSites();
                    foreach(var item in lst)
                    {
                        localdb.InsertSite(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void GetAllDeviceTypes()  
        {
            try
            {
                var lst = sqlsrv.DownloadAllDeviceTypes();
                if (lst != null && lst.Count > 0)
                {
                    localdb.DeleteAllDeviceType();
                    foreach (var item in lst)
                    {
                        localdb.InsertDeviceType(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void GetAllDevices()
        {
            try
            {
                var lst = sqlsrv.DownloadAllDevices();
                if (lst != null && lst.Count > 0)
                {
                    localdb.DeleteAllDevices();
                    foreach (var item in lst)
                    {
                        localdb.InsertDevice(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void GetAllUsers()    
        {
            try
            {
                var lst = sqlsrv.DownloadAllUsers();
                if (lst != null && lst.Count > 0)
                {
                    localdb.DeleteAllUsers();
                    foreach (var item in lst)
                    {
                        localdb.InsertUser(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void GetAllInstruction()  
        {
            try
            {
                var lst = sqlsrv.DownloadAllInstructions();
                if (lst != null && lst.Count > 0)
                {
                    localdb.DeleteAllInstructions();
                    foreach (var item in lst)
                    {
                        localdb.InsertInstruction(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void GetAllResults()  
        {
            try
            {
                var lst = sqlsrv.DownloadAllResults();
                if (lst != null && lst.Count > 0)
                {
                    localdb.DeleteAllResults();
                    foreach (var item in lst)
                    {
                        localdb.InsertResult(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void GetAllDeviceDashboardItems() 
        {
            try
            {
                var lst = sqlsrv.DownloadAllDeviceDashboardsItems();
                localdb.DeleteAllDeviceDashboardItems();
                if (lst != null && lst.Count > 0)
                {
                    foreach (var item in lst)
                    {
                        localdb.InsertDeviceDashboardItem(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void GetAllDashboardPriority()    
        {
            try
            {
                var lst = sqlsrv.DownloadAllDashboardPriority();
                localdb.DeleteAllDashboardPriority();
                if (lst != null && lst.Count > 0)
                {
                    foreach (var item in lst)
                    {
                        localdb.InsertDashboardPriority(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static void GetAllDashboardScenario()
        {
            try
            {
                var lst = sqlsrv.DownloadAllDashboardScenario();
                localdb.DeleteAllDashboardScenario();
                if (lst != null && lst.Count > 0)
                {
                    foreach (var item in lst)
                    {
                        localdb.InsertDashboardScenario(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
