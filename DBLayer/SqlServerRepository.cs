using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using THTController;
using THTController.DBLayer;

namespace SerialSample.DBLayer
{
    /// <summary>
    /// این کلاس جهت کار با دیتابیس اسکیوال سرور مرکز می باشد
    /// عملیات هایی از قبیل زیر :
    /// تمامی توابع دانلود جداول
    /// اینزرت لاگ ها روی سرور مرکزی
    /// اینزرت خطاها روی سرور جدول رس ارور
    /// و ...
    /// </summary>
    public class SqlServerRepository
    {
        private string cs;
        private static ErrorLog errorLog;
        public SqlServerRepository(string _sqlip)
        {
            cs = secret.GetConnectionString(_sqlip);
            errorLog = new ErrorLog(_sqlip);
        }

        public List<SiteEntity> DownloadAllSites()
        {
            const string GetProductsQuery = " select [ID],isnull([Name],''),isnull([Address],''),isnull([X],''),isnull([Y],''),isnull([GoogleAddress],''),isnull([IconID],0) from [dbo].[Sites](nolock) Where IsDeleted=0 ";

            var sites = new List<SiteEntity>();
            try
            {
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetProductsQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var site = new SiteEntity
                                    {
                                        ID = reader.GetInt32(0),
                                        Name = reader.GetString(1),
                                        Address = reader.GetString(2),
                                        X = reader.GetString(3),
                                        Y = reader.GetString(4),
                                        GoogleAddress = reader.GetString(5),
                                        IconID = reader.GetInt32(6)
                                    };
                                    sites.Add(site);
                                }
                            }
                        }
                        conn.Close();
                    }
                }
                return sites;
            }
            catch (Exception eSql)
            {
                errorLog.SaveLog(eSql);
                throw eSql;
            }
        }

        public List<DeviceTypeEntity> DownloadAllDeviceTypes()
        {
            const string GetProductsQuery = " SELECT [ID],isnull([Name],''),isnull([ModelNo],'') FROM [dbo].[DeviceTypes](nolock) Where IsDeleted=0 ";

            var sites = new List<DeviceTypeEntity>();
            try
            {
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetProductsQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var site = new DeviceTypeEntity
                                    {
                                        ID = reader.GetInt32(0),
                                        Name = reader.GetString(1),
                                        ModelNo = reader.GetString(2)
                                    };
                                    sites.Add(site);
                                }
                            }
                        }
                        conn.Close();
                    }
                }
                return sites;
            }
            catch (Exception eSql)
            {
                errorLog.SaveLog(eSql);
                throw eSql;
            }
        }

        public List<DeviceEntity> DownloadAllDevices()
        {
            const string GetProductsQuery = @" SELECT [ID],[DeviceType],[LocationID],[RowNo],isnull([Name],''),isnull([Address],''),isnull([ConnectedDeviceID],0) " +
                                             " FROM [dbo].[Devices](nolock) Where IsDeleted=0 ";

            var sites = new List<DeviceEntity>();
            try
            {
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetProductsQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var site = new DeviceEntity
                                    {
                                        ID = reader.GetInt32(0),
                                        DeviceType = reader.GetInt32(1),
                                        LocationID = reader.GetInt32(2),
                                        RowNow = reader.GetInt32(3),
                                        Name = reader.GetString(4),
                                        Address = reader.GetString(5),
                                        ConnectedDeviceID = reader.GetInt32(6),
                                    };
                                    sites.Add(site);
                                }
                            }
                        }
                        conn.Close();
                    }
                }
                return sites;
            }
            catch (Exception eSql)
            {
                errorLog.SaveLog(eSql);
                throw eSql;
            }
        }

        public List<UserEntity> DownloadAllUsers()
        {
            const string GetProductsQuery = @" SELECT [ID],isnull([UserName],''),isnull([UserPassword],''),isnull([FullName],''),isnull([Mobile],''),isnull([Address],''),isnull([RoleID],0),[IsActive] FROM [dbo].[Users](nolock) Where IsDeleted=0 ";

            var sites = new List<UserEntity>();
            try
            {
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetProductsQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var site = new UserEntity
                                    {
                                        ID = reader.GetInt32(0),
                                        UserName = reader.GetString(1),
                                        UserPassword = reader.GetString(2),
                                        FullName = reader.GetString(3),
                                        Mobile = reader.GetString(4),
                                        Address = reader.GetString(5),
                                        RoleID = reader.GetInt32(6),
                                        IsActive = reader.GetBoolean(7)
                                    };
                                    sites.Add(site);
                                }
                            }
                        }
                        conn.Close();
                    }
                }
                return sites;
            }
            catch (Exception eSql)
            {
                errorLog.SaveLog(eSql);
                throw eSql;
            }
        }

        public List<InstructionEntity> DownloadAllInstructions()
        {
            const string GetProductsQuery = @" SELECT  [ID],[DeviceType],isnull([Memo],''),isnull([FunctionCode],0),isnull([CRC1],0),isnull([CRC2],0),
                                                       isnull([Data],''),isnull([Formula],''),isnull([ChildInstructionID],0),isnull([UntilMin],0),
                                                       isnull([ChildType],0),isnull(GPIO,0),isnull(GPIOValue,0),isnull([IEEE],0),isnull([UnitName],''),
                                                       isnull([MinValue],0),isnull([MaxValue],0),isnull([SecondFormula],''),isnull(ParentID,0),
                                                       isnull(FromByte,0),isnull(ToByte,0)
                                               FROM [dbo].[Instructions](nolock) Where IsDeleted=0 ";

            var sites = new List<InstructionEntity>();
            try
            {
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetProductsQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var site = new InstructionEntity
                                    {
                                        ID = reader.GetInt32(0),
                                        DeviceType = reader.GetInt32(1),
                                        Memo = reader.GetString(2),
                                        FunctionCode = reader.GetInt32(3),
                                        CRC1 = reader.GetInt32(4),
                                        CRC2 = reader.GetInt32(5),
                                        Data = reader.GetString(6),
                                        Formula = reader.GetString(7),
                                        ChildInstructionID = reader.GetInt32(8),
                                        UntilMin = reader.GetInt32(9),
                                        ChildType = reader.GetInt32(10),
                                        GPIO = reader.GetInt32(11),
                                        GPIOValue = reader.GetInt32(12),
                                        IEEE = reader.GetBoolean(13),
                                        UnitName = reader.GetString(14),
                                        MinValue = reader.GetDecimal(15),
                                        MaxValue = reader.GetDecimal(16),
                                        SecondFormula = reader.GetString(17),
                                        ParentID = reader.GetInt32(18),
                                        FromByte = reader.GetInt32(19),
                                        ToByte = reader.GetInt32(20)
                                    };
                                    sites.Add(site);
                                }
                            }
                        }
                        conn.Close();
                    }
                }
                return sites;
            }
            catch (Exception eSql)
            {
                errorLog.SaveLog(eSql);
                throw eSql;
            }
        }

        public List<ResultEntity> DownloadAllResults()
        {
            const string GetProductsQuery = @" SELECT [ID],[DeviceType],isnull([Memo],''),isnull([FunctionCode],0),isnull([CRC1],0),isnull([CRC2],0),isnull([Data],'')
                                               FROM [dbo].[Results](nolock) Where IsDeleted=0 ";

            var sites = new List<ResultEntity>();
            try
            {
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetProductsQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var site = new ResultEntity
                                    {
                                        ID = reader.GetInt32(0),
                                        DeviceType = reader.GetInt32(1),
                                        Memo = reader.GetString(2),
                                        FunctionCode = reader.GetInt32(3),
                                        CRC1 = reader.GetInt32(4),
                                        CRC2 = reader.GetInt32(5),
                                        Data = reader.GetString(6)
                                    };
                                    sites.Add(site);
                                }
                            }
                        }
                        conn.Close();
                    }
                }
                return sites;
            }
            catch (Exception eSql)
            {
                errorLog.SaveLog(eSql);
                throw eSql;
            }
        }

        public List<DeviceDashboardItemEntity> DownloadAllDeviceDashboardsItems()
        {
            const string GetProductsQuery = @" SELECT [ID],[DeviceID],[InstructionID],isnull([Value],''),isnull([SaveTime],getdate()),isnull([Priority],0) 
                                                FROM [dbo].[DeviceDashboardItems](nolock) ";

            var sites = new List<DeviceDashboardItemEntity>();
            try
            {
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetProductsQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var site = new DeviceDashboardItemEntity
                                    {
                                        ID = reader.GetInt32(0),
                                        DeviceID = reader.GetInt32(1),
                                        InstructionID = reader.GetInt32(2),
                                        Value = reader.GetString(3),
                                        SaveTime = reader.GetDateTime(4),
                                        Priority = reader.GetInt32(5)
                                    };
                                    sites.Add(site);
                                }
                            }
                        }
                        conn.Close();
                    }
                }
                return sites;
            }
            catch (Exception eSql)
            {
                errorLog.SaveLog(eSql);
                throw eSql;
            }
        }

        public List<DeviceInstructionFireEntity> DownloadAllInstructionFires(string deviceid)
        {
            string GetProductsQuery = @" SELECT [ID],[DeviceID],[InstructionID],[UserID],isnull([SendTime],getdate()),isnull([Value],''),isnull([ResultTime],getdate()),isnull([ResultID],0)
                                         FROM [dbo].[DeviceInstructionFire](nolock) Where DeviceID IN (" + deviceid + ") AND LEN(ISNULL([Value],''))=0 ";

            var sites = new List<DeviceInstructionFireEntity>();
            try
            {
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetProductsQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var site = new DeviceInstructionFireEntity
                                    {
                                        ID = reader.GetInt32(0),
                                        DeviceID = reader.GetInt32(1),
                                        InstructionID = reader.GetInt32(2),
                                        UserID = reader.GetInt32(3),
                                        SendTime = reader.GetDateTime(4),
                                        Value = reader.GetString(5),
                                        ResultTime = reader.GetDateTime(6),
                                        ResultID = reader.GetInt32(7)
                                    };
                                    sites.Add(site);
                                }
                            }
                        }
                        conn.Close();
                    }
                }
                return sites;
            }
            catch (Exception eSql)
            {
                errorLog.SaveLog(eSql);
                throw eSql;
            }
        }

        public void UpdateInstructionFireOnServer(int id, string value, int resultID)
        {
            string GetProductsQuery = string.Format(@" UPDATE [dbo].[DeviceInstructionFire] SET [Value] = '{0}',[ResultTime] = getdate(),[ResultID]={2} WHERE ID = {1} ", value, id, resultID);
            try
            {
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetProductsQuery;
                            cmd.ExecuteNonQuery();
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception eSql)
            {
                errorLog.SaveLog(eSql);
                throw eSql;
            }
        }

        public DateTime GetServerTime()
        {
            string GetProductsQuery = @" SELECT GETDATE() ";
            try
            {
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetProductsQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    return reader.GetDateTime(0);

                                }
                            }
                        }
                        conn.Close();
                    }
                    return DateTime.Now;
                }
            }
            catch (Exception eSql)
            {
                errorLog.SaveLog(eSql);
                throw eSql;
            }
        }

        public List<DashboardPriorityEntity> DownloadAllDashboardPriority()
        {
            string GetProductsQuery = @" SELECT [ID],[Name],[Value] FROM [dbo].[DashboardPriority](nolock) ";

            var sites = new List<DashboardPriorityEntity>();
            try
            {
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetProductsQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var site = new DashboardPriorityEntity
                                    {
                                        ID = reader.GetInt32(0),
                                        Name = reader.GetString(1),
                                        Value = reader.GetDecimal(2)
                                    };
                                    sites.Add(site);
                                }
                            }
                        }
                        conn.Close();
                    }
                }
                return sites;
            }
            catch (Exception eSql)
            {
                errorLog.SaveLog(eSql);
                throw eSql;
            }
        }

        public List<DashboardScenarioEntity> DownloadAllDashboardScenario()
        {
            string GetProductsQuery = @" SELECT [ID],[DashboardItemID],[ConnectedDeviceID],[InstructionID],
                                                isnull([MinValue],''),isnull([MaxValue],''),isnull([FromTime],''),isnull([ToTime],'')
                                         FROM [dbo].[DashboardScenario](nolock) ";

            var sites = new List<DashboardScenarioEntity>();
            try
            {
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetProductsQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var site = new DashboardScenarioEntity
                                    {
                                        ID = reader.GetInt32(0),
                                        DashboardItemID = reader.GetInt32(1),
                                        ConnectedDeviceID = reader.GetInt32(2),
                                        InstructionID = reader.GetInt32(3),
                                        MinValue = reader.GetString(4),
                                        MaxValue = reader.GetString(5),
                                        FromTime = reader.GetTimeSpan(6),
                                        ToTime = reader.GetTimeSpan(7)
                                    };
                                    sites.Add(site);
                                }
                            }
                        }
                        conn.Close();
                    }
                }
                return sites;
            }
            catch (Exception eSql)
            {
                errorLog.SaveLog(eSql);
                throw eSql;
            }
        }

        public void UploadDashboardLogsToServer(List<DashboardLogEntity> lst)
        {
            try
            {
                string GetProductsQuery = "";
                foreach (var item in lst)
                {
                    GetProductsQuery += string.Format(@" EXEC [dbo].[InsertDashboardLog]
		                                                    @DashboardItemID ={0},
		                                                    @InstructionID ={1},
		                                                    @Value ='{2}',
		                                                    @SaveTime ='{3}',
		                                                    @ResultID ={4},
		                                                    @Description ='{5}' ",
                        item.DashboardItemID ?? 0,
                        item.InstructionID ?? 0,
                        item.Value,
                        item.SaveTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        item.ResultID ?? 0,
                        item.Description);
                    GetProductsQuery += Environment.NewLine;
                }

                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetProductsQuery;
                            cmd.ExecuteNonQuery();
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception eSql)
            {
                errorLog.SaveLog(eSql);
                throw eSql;
            }
        }

        public void InsertErrorOnSqlServer(string ip, string error, string stacktrace)
        {
            try
            {
                string GetProductsQuery = "";
                GetProductsQuery += string.Format(@"  Exec [dbo].[InsertRasError] @OperatorUserID=0,@IP='{0}',@Error='{1}',@StackTrace='{2}'  ", ip, error, stacktrace);
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetProductsQuery;
                            cmd.ExecuteNonQuery();
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception eSql)
            {
                throw eSql;
            }
        }

        public bool CheckForHardUpdate(int siteid)
        {
            var localip = Extension.LocalIPAddress;
            if (string.IsNullOrWhiteSpace(localip)) return false;
            if (siteid <= 0) return false;
            string GetProductsQuery = string.Format(@" select [HardUpdate] from [dbo].[Ras](nolock) Where [SiteID]={0} and [IP] like '{1}%' ", siteid, localip);
            try
            {
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetProductsQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    return reader.GetBoolean(0);

                                }
                            }
                        }
                        conn.Close();
                    }
                    return false;
                }
            }
            catch (Exception eSql)
            {
                errorLog.SaveLog(eSql);
                throw eSql;
            }
        }

        public void UpdateRasHardUpdateStatus(int siteid)
        {
            try
            {
                var localip = Extension.LocalIPAddress;
                if (string.IsNullOrWhiteSpace(localip)) return;
                if (siteid <= 0) return;
                string GetProductsQuery = string.Format(@" update [dbo].[Ras] set [HardUpdate]=0 Where [SiteID]={0} and [IP] like '{1}%' ", siteid, localip);
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetProductsQuery;
                            cmd.ExecuteNonQuery();
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception eSql)
            {
                throw eSql;
            }
        }
    }
}