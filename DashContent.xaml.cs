using System;
using System.Collections.Generic;
using System.Linq;
using THTController.DBLayer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace THTController
{
    public delegate void LocalFireClicked(object sender, EventArgs e);  
    /// <summary>
    /// یوزر کنترل داشبورد
    /// تب کنترل نمایش لاگ ها 
    /// </summary>
    public sealed partial class DashContent : UserControl
    {
        public List<DashboardLogEntityUI> Logs;
        public event LocalFireClicked _LocalFireClicked;    
        CacheEntity _localCache;
        int _deviceID;
        ErrorLog errorLog;
        public DashContent(int deviceID, CacheEntity localCache)
        {
            try
            {
                Logs = new List<DashboardLogEntityUI>();
                this.InitializeComponent();
                _deviceID = deviceID;
                _localCache = localCache;
                errorLog = new ErrorLog(localCache.Setting.SQLServerIP);
            }
            catch(Exception ex)
            {
                errorLog.SaveLog(ex);
                throw ex;
            }
        }
        public void Refresh(CacheEntity localCache)
        {
            try
            {
                _localCache = localCache;
                tbxDeviceResult.Text = string.Empty;
                BindInstructionCombo();
                BindDashboardLogs(localCache);
            }
            catch(Exception ex)
            {
                errorLog.SaveLog(ex);
                throw ex;
            }
        }
        public void SetLocalFireResult(string result)
        {
            tbxDeviceResult.Text = result;
        }
        private void BindInstructionCombo()
        {
            try
            {
                cmbCurrentDeviceInstructions.Items.Clear();
                var ins = _localCache.Instructions.Where(l => l.DeviceType == _localCache.Devices.FirstOrDefault(j => j.ID == _deviceID).DeviceType).ToList();
                if (ins != null && ins.Count > 0)
                {
                    foreach (var item in ins)
                    {
                        cmbCurrentDeviceInstructions.Items.Add(item.Memo);
                    }
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                throw;
            }
        }
        private void BindDashboardLogs(CacheEntity localCache)
        {
            try
            {
                var cache = localCache.DashboardLogs;
                var result = new List<DashboardLogEntity>();
                var currentdevins = _localCache.Instructions.Where(l => l.DeviceType == _localCache.Devices.FirstOrDefault(j => j.ID == _deviceID).DeviceType).Select(p => p.ID).ToList();
                foreach(var ll in cache.Where(l=>l.DashboardItemID > 0))
                {
                    if (currentdevins.Contains(_localCache.DashboardItems.FirstOrDefault(d=>d.ID==ll.DashboardItemID).InstructionID))
                    {
                        ll.InstructionID = _localCache.DashboardItems.FirstOrDefault(d => d.ID == ll.DashboardItemID).InstructionID;
                        result.Add(ll);
                    }
                }
                foreach (var ll in cache.Where(l => l.InstructionID > 0))
                {
                    if (currentdevins.Contains(ll.InstructionID.Value))
                    {
                        result.Add(ll);
                    }
                }
                var logs = new List<DashboardLogEntityUI>();
                foreach(var item in result)
                {
                    logs.Add(new DashboardLogEntityUI(item));
                }
                if (logs != null && logs.Count > 0)
                {
                    logs.ForEach(l => l.ResultName = ((l.ResultID.HasValue && l.ResultID > 0) ? _localCache.Results.FirstOrDefault(j => j.ID == l.ResultID).Memo : string.Empty));
                    logs.ForEach(l => l.InstructionName = ((l.InstructionID.HasValue && l.InstructionID > 0) ? _localCache.Instructions.FirstOrDefault(j => j.ID == l.InstructionID).Memo : string.Empty));
                }
                Logs = logs;
                var Logss = Logs.OrderByDescending(p=>p.SaveTime).DistinctBy(l => new {l.InstructionID, l.DashboardItemID}).ToList();
                dgvLogs.ItemsSource = Logss.OrderBy(l => l.DashboardItemID);//.DistinctBy(l => new { l.SaveTime, l.Value, l.InstructionID, l.DashboardItemID });
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                throw;
            }
        }

        private void btnLocalFire_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(cmbCurrentDeviceInstructions.Items != null &&
                   cmbCurrentDeviceInstructions.Items.Count > 0 &&
                   cmbCurrentDeviceInstructions.SelectedValue != null &&
                   !string.IsNullOrWhiteSpace(cmbCurrentDeviceInstructions.SelectedValue.ToString()))
                {
                    var obj = new LocalFireEntity()
                    {
                        DeviceID = _deviceID,
                        InstructionID = _localCache.Instructions.FirstOrDefault(l => l.DeviceType == _localCache.Devices.FirstOrDefault(j => j.ID == _deviceID).DeviceType && l.Memo == cmbCurrentDeviceInstructions.SelectedValue.ToString()).ID,
                        Type = "LocalFire"
                    };
                    if(obj != null)
                    {
                        _LocalFireClicked(obj, null);
                    }
                }
            }
            catch (Exception ex)
            {
                errorLog.SaveLog(ex);
                throw;
            }
        }
    }
}