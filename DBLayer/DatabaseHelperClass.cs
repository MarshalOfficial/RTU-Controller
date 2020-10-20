using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using THTController;
using THTController.DBLayer;
using SQLite.Net;
using System.Threading;

namespace SerialSample.DBLayer
{
    /// <summary>
    /// این کلاس جهت کار با دیتابیس اسکیولایت می باشد
    /// تمامی اینزرت و ادیت و حذف و سلکت جداول دیتابیس لوکال داخل این کلاس می باشد
    /// </summary>
    public class DatabaseHelperClass
    {
        private static ErrorLog errorLog;
        public DatabaseHelperClass(string _sqlip)
        {
            errorLog = new ErrorLog(_sqlip);
        }
        //ReaderWriterLockSlim
        public void CreateDatabase(string DB_PATH)
        {
            if (!CheckFileExists(DB_PATH).Result)
            {
                var plat = new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT();
                plat.SQLiteApi.Config(SQLite.Net.Interop.ConfigOption.MultiThread);
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(plat, DB_PATH))
                {
                    conn.CreateTable<DeviceTypeEntity>();
                    conn.CreateTable<DeviceEntity>();
                    conn.CreateTable<InstructionEntity>();
                    conn.CreateTable<ResultEntity>();
                    conn.CreateTable<SiteEntity>();
                    conn.CreateTable<UserEntity>();
                    conn.CreateTable<DeviceDashboardItemEntity>();
                    conn.CreateTable<DeviceInstructionFireEntity>();
                    conn.CreateTable<SettingEntity>();
                    conn.CreateTable<DashboardLogEntity>();
                    conn.CreateTable<DashboardPriorityEntity>();
                    conn.CreateTable<DashboardScenarioEntity>();
                    conn.CreateTable<ErrorLogEntity>();
                    conn.CreateTable<GPIOHistoryEntity>();
                }
            }
        }
        private async Task<bool> CheckFileExists(string fileName)
        {
            try
            {
                var store = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region [DeviceTypeEntity]
        public void InsertDeviceType(DeviceTypeEntity obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    conn.RunInTransaction(() =>
                    {
                        conn.Insert(obj);
                    });
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public DeviceTypeEntity GetDeviceType(int DeviceTypeID)
        {
            try
            {

                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var existingconact = conn.Query<DeviceTypeEntity>("select * from DeviceTypeEntity where ID =" + DeviceTypeID).FirstOrDefault();
                    return existingconact;
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }
        }
        public List<DeviceTypeEntity> GetAllDeviceTypes()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var myCollection = conn.Table<DeviceTypeEntity>().ToList<DeviceTypeEntity>();
                    return myCollection;
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }

        }

        public void UpdateDeviceType(DeviceTypeEntity Obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<DeviceTypeEntity>("select * from DeviceTypeEntity where ID =" + Obj.ID).FirstOrDefault();
                    if (existingconact != null)
                    {

                        conn.RunInTransaction(() =>
                        {
                            conn.Update(Obj);
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteAllDeviceType()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    conn.DropTable<DeviceTypeEntity>();
                    conn.CreateTable<DeviceTypeEntity>();
                    conn.Dispose();
                    conn.Close();

                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteDeviceType(int Id)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<DeviceTypeEntity>("select * from DeviceTypeEntity where ID =" + Id).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Delete(existingconact);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }
        #endregion

        #region [SiteEntity]
        public void InsertSite(SiteEntity obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    conn.RunInTransaction(() =>
                    {
                        conn.Insert(obj);
                    });
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public SiteEntity GetSite(int SiteID)
        {
            try
            {

                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var existingconact = conn.Query<SiteEntity>("select * from SiteEntity where ID =" + SiteID).FirstOrDefault();
                    return existingconact;
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }
        }
        public List<SiteEntity> GetAllSites()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var myCollection = conn.Table<SiteEntity>().ToList<SiteEntity>();
                    return myCollection;
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }

        }

        public void UpdateSite(SiteEntity Obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<SiteEntity>("select * from SiteEntity where ID =" + Obj.ID).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Update(Obj);
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteAllSites()
        {
            try
            {

                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    conn.DropTable<SiteEntity>();
                    conn.CreateTable<SiteEntity>();
                    conn.Dispose();
                    conn.Close();

                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteSite(int Id)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<DeviceTypeEntity>("select * from SiteEntity where ID =" + Id).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Delete(existingconact);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }
        #endregion

        #region [DeviceEntity]
        public void InsertDevice(DeviceEntity obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    conn.RunInTransaction(() =>
                    {
                        conn.Insert(obj);
                    });
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public DeviceEntity GetDevice(int deviceid)
        {
            try
            {

                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var existingconact = conn.Query<DeviceEntity>("select * from DeviceEntity where ID =" + deviceid).FirstOrDefault();
                    return existingconact;
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }
        }
        public List<DeviceEntity> GetAllDevices()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var myCollection = conn.Table<DeviceEntity>().ToList<DeviceEntity>();
                    return myCollection;
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }

        }
        public string GetAllDeviceIDs()
        {
            try
            {
                var ss = GetSetting(0).SiteID;
                var lst = GetAllDevices().Where(l => l.LocationID == ss).ToList();
                var value = "";
                foreach (var item in lst)
                {
                    value += item.ID + ",";
                }
                return (value != null && value.Length > 0) ? value.TrimEnd(',') : value;
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return string.Empty;
            }
        }
        public void UpdateDevice(DeviceEntity Obj)
        {
            try
            {

                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<DeviceEntity>("select * from DeviceEntity where ID =" + Obj.ID).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Update(Obj);
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteAllDevices()
        {
            try
            {

                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    conn.DropTable<DeviceEntity>();
                    conn.CreateTable<DeviceEntity>();
                    conn.Dispose();
                    conn.Close();

                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteDevice(int Id)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<DeviceEntity>("select * from DeviceEntity where ID =" + Id).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Delete(existingconact);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }
        #endregion

        #region [UserEntity]
        public void InsertUser(UserEntity obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    conn.RunInTransaction(() =>
                    {
                        conn.Insert(obj);
                    });
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public UserEntity GetUser(int userid)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var existingconact = conn.Query<UserEntity>("select * from UserEntity where ID =" + userid).FirstOrDefault();
                    return existingconact;
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }
        }
        public List<UserEntity> GetAllUsers()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var myCollection = conn.Table<UserEntity>().ToList<UserEntity>();
                    return myCollection;
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }

        }

        public void UpdateUser(UserEntity Obj)
        {
            try
            {

                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<UserEntity>("select * from UserEntity where ID =" + Obj.ID).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Update(Obj);
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteAllUsers()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    conn.DropTable<UserEntity>();
                    conn.CreateTable<UserEntity>();
                    conn.Dispose();
                    conn.Close();

                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteUser(int Id)
        {
            try
            {

                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<UserEntity>("select * from UserEntity where ID =" + Id).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Delete(existingconact);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }
        #endregion

        #region [InstructionEntity]
        public void InsertInstruction(InstructionEntity obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    conn.RunInTransaction(() =>
                    {
                        conn.Insert(obj);
                    });
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public InstructionEntity GetInstruction(int id)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var existingconact = conn.Query<InstructionEntity>("select * from InstructionEntity where ID =" + id).FirstOrDefault();
                    return existingconact;
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }
        }
        public List<InstructionEntity> GetAllInstructions()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var myCollection = conn.Table<InstructionEntity>().ToList<InstructionEntity>();
                    return myCollection;
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }

        }

        public void UpdateInstruction(InstructionEntity Obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<InstructionEntity>("select * from InstructionEntity where ID =" + Obj.ID).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Update(Obj);
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteAllInstructions()
        {
            try
            {


                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    conn.DropTable<InstructionEntity>();
                    conn.CreateTable<InstructionEntity>();
                    conn.Dispose();
                    conn.Close();

                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteInstruction(int Id)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<InstructionEntity>("select * from InstructionEntity where ID =" + Id).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Delete(existingconact);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }
        #endregion

        #region [ResultEntity]
        public void InsertResult(ResultEntity obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    conn.RunInTransaction(() =>
                    {
                        conn.Insert(obj);
                    });
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public ResultEntity GetResult(int id)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var existingconact = conn.Query<ResultEntity>("select * from ResultEntity where ID =" + id).FirstOrDefault();
                    return existingconact;
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }
        }
        public List<ResultEntity> GetAllResults()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var myCollection = conn.Table<ResultEntity>().ToList<ResultEntity>();
                    return myCollection;
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }

        }

        public void UpdateResult(ResultEntity Obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<ResultEntity>("select * from ResultEntity where ID =" + Obj.ID).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Update(Obj);
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteAllResults()
        {
            try
            {

                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    conn.DropTable<ResultEntity>();
                    conn.CreateTable<ResultEntity>();
                    conn.Dispose();
                    conn.Close();

                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteResult(int Id)
        {
            try
            {


                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<ResultEntity>("select * from ResultEntity where ID =" + Id).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Delete(existingconact);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }
        #endregion

        #region [DeviceDashboardItemEntity]
        public void InsertDeviceDashboardItem(DeviceDashboardItemEntity obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    conn.RunInTransaction(() =>
                    {
                        conn.Insert(obj);
                    });
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public DeviceDashboardItemEntity GetDeviceDashboardItem(int id)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var existingconact = conn.Query<DeviceDashboardItemEntity>("select * from DeviceDashboardItemEntity where ID =" + id).FirstOrDefault();
                    return existingconact;
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }
        }
        public List<DeviceDashboardItemEntity> GetAllDeviceDashboardItems()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var myCollection = conn.Table<DeviceDashboardItemEntity>().ToList<DeviceDashboardItemEntity>();
                    return myCollection;
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }

        }

        public void UpdateDeviceDashboardItem(DeviceDashboardItemEntity Obj)
        {
            try
            {


                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<DeviceDashboardItemEntity>("select * from DeviceDashboardItemEntity where ID =" + Obj.ID).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Update(Obj);
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteAllDeviceDashboardItems()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    conn.DropTable<DeviceDashboardItemEntity>();
                    conn.CreateTable<DeviceDashboardItemEntity>();
                    conn.Dispose();
                    conn.Close();

                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteDeviceDashboardItem(int Id)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<DeviceDashboardItemEntity>("select * from DeviceDashboardItemEntity where ID =" + Id).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Delete(existingconact);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }
        #endregion

        #region [InstructionFireEntity]
        public void InsertInstructionFire(DeviceInstructionFireEntity obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    conn.RunInTransaction(() =>
                    {
                        conn.Insert(obj);
                    });
                }
            }
            catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public DeviceInstructionFireEntity GetDeviceInstructionFire(int id)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var existingconact = conn.Query<DeviceInstructionFireEntity>("select * from DeviceInstructionFireEntity where ID =" + id).FirstOrDefault();
                    return existingconact;
                }
            }catch(Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }
        }
        public List<DeviceInstructionFireEntity> GetAllDeviceInstructionFires()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var myCollection = conn.Table<DeviceInstructionFireEntity>().ToList<DeviceInstructionFireEntity>();
                    return myCollection;
                }
            }
            catch(Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }

        }

        public void UpdateDeviceInstructionFire(DeviceInstructionFireEntity Obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<DeviceInstructionFireEntity>("select * from DeviceInstructionFireEntity where ID =" + Obj.ID).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Update(Obj);
                        });
                    }

                }
            }
            catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        //public void DeleteAllDeviceInstructionFires()
        //{
        //    using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
        //    {

        //        conn.DropTable<DeviceInstructionFireEntity>();
        //        conn.CreateTable<DeviceInstructionFireEntity>();
        //        conn.Dispose();
        //        conn.Close();

        //    }
        //}

        //public void DeleteDeviceInstructionFire(int Id)
        //{
        //    using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
        //    {

        //        var existingconact = conn.Query<DeviceInstructionFireEntity>("select * from DeviceInstructionFireEntity where ID =" + Id).FirstOrDefault();
        //        if (existingconact != null)
        //        {
        //            conn.RunInTransaction(() =>
        //            {
        //                conn.Delete(existingconact);
        //            });
        //        }
        //    }
        //}
        #endregion

        #region [SettingEntity]
        public void InsertSetting(SettingEntity obj)
        {
            try
            {


                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    conn.RunInTransaction(() =>
                    {
                        conn.Insert(obj);
                    });
                }
            }catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public SettingEntity GetSetting(int id)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var existingconact = conn.Query<SettingEntity>("select * from SettingEntity where ID =" + id).FirstOrDefault();
                    return existingconact;
                }
            }catch(Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }
        }
        public List<SettingEntity> GetAllSettings()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var myCollection = conn.Table<SettingEntity>().ToList<SettingEntity>();
                    return myCollection;
                }
            }
            catch(Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }

        }

        public void UpdateSetting(SettingEntity Obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<SettingEntity>("select * from SettingEntity where ID =" + Obj.ID).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Update(Obj);
                        });
                    }

                }
            }catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteAllSettings()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    conn.DropTable<SettingEntity>();
                    conn.CreateTable<SettingEntity>();
                    conn.Dispose();
                    conn.Close();

                }
            }catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteSetting(int Id)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<SettingEntity>("select * from SettingEntity where ID =" + Id).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Delete(existingconact);
                        });
                    }
                }
            }catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }
        #endregion

        #region [DashboardPriorityEntity]
        public void InsertDashboardPriority(DashboardPriorityEntity obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    conn.RunInTransaction(() =>
                    {
                        conn.Insert(obj);
                    });
                }
            }catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public DashboardPriorityEntity GetDashboardPriority(int id)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var existingconact = conn.Query<DashboardPriorityEntity>("select * from DashboardPriorityEntity where ID =" + id).FirstOrDefault();
                    return existingconact;
                }
            }
            catch(Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }
        }
        public List<DashboardPriorityEntity> GetAllDashboardPriority()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var myCollection = conn.Table<DashboardPriorityEntity>().ToList<DashboardPriorityEntity>();
                    return myCollection;
                }
            }
            catch(Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }

        }

        public void UpdateDashboardPriority(DashboardPriorityEntity Obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<DashboardPriorityEntity>("select * from DashboardPriorityEntity where ID =" + Obj.ID).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Update(Obj);
                        });
                    }

                }
            }catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteAllDashboardPriority()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    conn.DropTable<DashboardPriorityEntity>();
                    conn.CreateTable<DashboardPriorityEntity>();
                    conn.Dispose();
                    conn.Close();

                }
            }catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteDashboardPriority(int Id)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<DashboardPriorityEntity>("select * from DashboardPriorityEntity where ID =" + Id).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Delete(existingconact);
                        });
                    }
                }
            }catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }
        #endregion

        #region [DashboardLogEntity]
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(initialCount: 1);
        public async void InsertDashboardLogAsync(DashboardLogEntity obj)
        {
            try
            {
                await semaphoreSlim.WaitAsync();
                try
                {
                    var plat = new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT();
                    plat.SQLiteApi.Config(SQLite.Net.Interop.ConfigOption.Serialized);
                    using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(plat, App.DB_PATH))
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Insert(obj);
                        });
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }
        public async Task<List<DashboardLogEntity>> GetAllDashboardLog()
        {
            try
            {
                await semaphoreSlim.WaitAsync();
                try
                {
                    var plat = new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT();
                    plat.SQLiteApi.Config(SQLite.Net.Interop.ConfigOption.Serialized);
                    using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(plat, App.DB_PATH))
                    {
                        var myCollection = conn.Table<DashboardLogEntity>().Take(500).OrderByDescending(l=>l.ID).ToList();
                        return myCollection;
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }
        }
        public async Task<List<DashboardLogEntity>> GetAllUnsendDashboardLog()
        {
            try
            {
                await semaphoreSlim.WaitAsync();
                try
                {
                    var plat = new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT();
                    plat.SQLiteApi.Config(SQLite.Net.Interop.ConfigOption.Serialized);
                    using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(plat, App.DB_PATH))
                    {
                        var myCollection = conn.Table<DashboardLogEntity>().ToList<DashboardLogEntity>();
                        return myCollection?.Where(l => l.IsSended == false).ToList();
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
            catch(Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }

        }
        public async Task<List<DashboardLogEntity>> GetAllDashboardLogForDelete()   
        {
            try
            {
                await semaphoreSlim.WaitAsync();
                try
                {
                    var plat = new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT();
                    plat.SQLiteApi.Config(SQLite.Net.Interop.ConfigOption.Serialized);
                    using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(plat, App.DB_PATH))
                    {
                        var myCollection = conn.Table<DashboardLogEntity>().ToList<DashboardLogEntity>();
                        return myCollection?.Where(l => l.IsSended == true && l.SaveTime <= DateTime.UtcNow.AddDays(-1)).ToList();
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }

        }
        public async void UpdateDashboardLog(DashboardLogEntity Obj)
        {
            try
            {
                await semaphoreSlim.WaitAsync();
                try
                {
                    var plat = new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT();
                    plat.SQLiteApi.Config(SQLite.Net.Interop.ConfigOption.Serialized);
                    using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(plat, App.DB_PATH))
                    {

                        var existingconact = conn.Query<DashboardLogEntity>("select * from DashboardLogEntity where ID =" + Obj.ID).FirstOrDefault();
                        if (existingconact != null)
                        {
                            conn.RunInTransaction(() =>
                            {
                                conn.Update(Obj);
                            });
                        }
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        // todo handle multithread later
        public void DeleteAllDashboardLog()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    conn.DropTable<DashboardLogEntity>();
                    conn.CreateTable<DashboardLogEntity>();
                    conn.Dispose();
                    conn.Close();

                }
            }
            catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }
        public DashboardLogEntity GetDashboardLog(long id)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var existingconact = conn.Query<DashboardLogEntity>("select * from DashboardLogEntity where ID =" + id).FirstOrDefault();
                    return existingconact;
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }
        }
        public void DeleteDashboardLog(long Id)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<DashboardLogEntity>("select * from DashboardLogEntity where ID =" + Id).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Delete(existingconact);
                        });
                    }
                }
            }catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }
        #endregion

        #region [DashboardScenario]
        public void InsertDashboardScenario(DashboardScenarioEntity obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    conn.RunInTransaction(() =>
                    {
                        conn.Insert(obj);
                    });
                }
            }catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public DashboardScenarioEntity GetDashboardScenario(int id)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var existingconact = conn.Query<DashboardScenarioEntity>("select * from DashboardScenarioEntity where ID =" + id).FirstOrDefault();
                    return existingconact;
                }
            }catch(Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }
        }
        public List<DashboardScenarioEntity> GetAllDashboardScenario()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var myCollection = conn.Table<DashboardScenarioEntity>().ToList<DashboardScenarioEntity>();
                    return myCollection;
                }
            }
            catch(Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }

        }

        public void UpdateDashboardScenario(DashboardScenarioEntity Obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<DashboardScenarioEntity>("select * from DashboardScenarioEntity where ID =" + Obj.ID).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Update(Obj);
                        });
                    }

                }
            }catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteAllDashboardScenario()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    conn.DropTable<DashboardScenarioEntity>();
                    conn.CreateTable<DashboardScenarioEntity>();
                    conn.Dispose();
                    conn.Close();

                }
            }
            catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteDashboardScenario(int Id)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<DashboardScenarioEntity>("select * from DashboardScenarioEntity where ID =" + Id).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Delete(existingconact);
                        });
                    }
                }
            }catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }
        #endregion

        #region [ErrorLogEntity]
        public void InsertErrorLog(ErrorLogEntity obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    conn.RunInTransaction(() =>
                    {
                        conn.Insert(obj);
                    });
                }
            }
            catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public ErrorLogEntity GetErrorLog(int id)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var existingconact = conn.Query<ErrorLogEntity>("select * from ErrorLogEntity where ID =" + id).FirstOrDefault();
                    return existingconact;
                }
            }
            catch(Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }
        }
        public List<ErrorLogEntity> GetAllErrorLogs()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var myCollection = conn.Table<ErrorLogEntity>().ToList<ErrorLogEntity>();
                    return myCollection;
                }
            }
            catch(Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }

        }

        public void UpdateErrorLog(ErrorLogEntity Obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<ErrorLogEntity>("select * from ErrorLogEntity where ID =" + Obj.ID).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Update(Obj);
                        });
                    }

                }
            }catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteAllErrorLogs()
        {
            try
            {


                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    conn.DropTable<ErrorLogEntity>();
                    conn.CreateTable<ErrorLogEntity>();
                    conn.Dispose();
                    conn.Close();

                }
            }catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteErrorLog(int Id)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<ErrorLogEntity>("select * from ErrorLogEntity where ID =" + Id).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Delete(existingconact);
                        });
                    }
                }
            }
            catch(Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }
        #endregion

        #region [GPIOHistoryEntity]
        private void InsertGPIOHistory(GPIOHistoryEntity obj)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    conn.RunInTransaction(() =>
                    {
                        conn.Insert(obj);
                    });
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        private GPIOHistoryEntity GetGPIOHistory(int GPIO)
        {
            try
            {

                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var existingconact = conn.Query<GPIOHistoryEntity>("select * from GPIOHistoryEntity where GPIO =" + GPIO).FirstOrDefault();
                    return existingconact;
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }
        }
        public List<GPIOHistoryEntity> GetAllGPIO()
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {
                    var myCollection = conn.Table<GPIOHistoryEntity>().ToList<GPIOHistoryEntity>();
                    return myCollection;
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                return null;
            }

        }
            
        public void InsertOrUpdateGPIOHistory(GPIOHistoryEntity Obj)
        {
            try
            {
                if (GetGPIOHistory(Obj.GPIO) == null)
                {
                    InsertGPIOHistory(Obj);
                }
                else
                {
                    using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                    {

                        var existingconact = conn.Query<GPIOHistoryEntity>("select * from GPIOHistoryEntity where GPIO =" + Obj.GPIO).FirstOrDefault();
                        if (existingconact != null)
                        {
                            conn.RunInTransaction(() =>
                            {
                                conn.Update(Obj);
                            });
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteAllGPIO()
        {
            try
            {

                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    conn.DropTable<GPIOHistoryEntity>();
                    conn.CreateTable<GPIOHistoryEntity>();
                    conn.Dispose();
                    conn.Close();

                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }

        public void DeleteGPIOHistory(int gpio)
        {
            try
            {
                using (SQLite.Net.SQLiteConnection conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), App.DB_PATH))
                {

                    var existingconact = conn.Query<GPIOHistoryEntity>("select * from GPIOHistoryEntity where GPIO =" + gpio).FirstOrDefault();
                    if (existingconact != null)
                    {
                        conn.RunInTransaction(() =>
                        {
                            conn.Delete(existingconact);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
            }
        }
        #endregion
    }
}