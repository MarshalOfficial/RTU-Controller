using SerialSample.DBLayer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using THTController;
using THTController.DBLayer;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace SerialSample
{
    /// <summary>
    /// فرم تنظیمات
    /// </summary>
    public sealed partial class frmConfig : Page
    {
        public string baudrate, parity, stopbits, databits;
        public int readtimeout, writeout;

        public frmConfig()
        {
            this.InitializeComponent();
            SetSerialSettingDefault();
            lblIpAddresss.Text = Extension.LocalIPAddress;
            BindCombo();
            BindSetting();
        }
        private async void BindCombo()
        {
            try
            {
                var localdb = new DatabaseHelperClass(tbxSqlIP.Text.Trim());
                cmbCurrentSite.Items.Clear();
                var lstsites = localdb.GetAllSites();
                if (lstsites != null && lstsites.Count > 0)
                {
                    foreach (var item in lstsites)
                    {
                        cmbCurrentSite.Items.Add(item.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                //ErrorLog.SaveLog(ex);
                var dialog = new MessageDialog(ex.Message + " * " + ((ex.InnerException != null) ? ex.InnerException.Message : ""));
                dialog.Title = "خطای  لود مجتمع ها";
                await dialog.ShowAsync();
            }
        }

        private async void BindSetting()
        {
            try
            {
                var localdb = new DatabaseHelperClass(tbxSqlIP.Text.Trim());
                var lstsetting = localdb.GetAllSettings();
                if (lstsetting != null && lstsetting.Count == 1 && cmbCurrentSite.Items != null && cmbCurrentSite.Items.Count > 0)
                {
                    cmbCurrentSite.SelectedItem = localdb.GetSite(lstsetting[0].SiteID).Name;
                    tbxSqlIP.Text = lstsetting[0].SQLServerIP;
                }
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message + " * " + ((ex.InnerException != null) ? ex.InnerException.Message : ""));
                dialog.Title = "خطای  لود تنظیمات در کنترلها";
                await dialog.ShowAsync();
            }
        }
        /// <summary>
        /// دکمه ثبت تنظیمات
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSaveSetting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbCurrentSite.Items != null &&
                   cmbCurrentSite.Items.Count > 0 &&
                   cmbCurrentSite.SelectedValue != null &&
                   !string.IsNullOrWhiteSpace(cmbCurrentSite.SelectedValue.ToString()) &&
                   !string.IsNullOrWhiteSpace(tbxSqlIP.Text.Trim()))
                {
                    var localdb = new DatabaseHelperClass(tbxSqlIP.Text.Trim());
                    var lstsetting = localdb.GetAllSettings();
                    if (lstsetting != null && lstsetting.Count == 1)
                    {
                        var obj = lstsetting[0];
                        obj.SiteID = localdb.GetAllSites().FirstOrDefault(l => l.Name == cmbCurrentSite.SelectedValue.ToString()).ID;
                        obj.SQLServerIP = tbxSqlIP.Text.Trim();
                        localdb.UpdateSetting(obj);
                    }
                    else
                    {
                        localdb.InsertSetting(new SettingEntity()
                        {
                            SiteID = localdb.GetAllSites().FirstOrDefault(l => l.Name == cmbCurrentSite.SelectedValue.ToString()).ID,
                            SQLServerIP = tbxSqlIP.Text.Trim()
                        });
                    }
                    var dialog = new MessageDialog("ثبت با موفقیت انجام شد");
                    dialog.Title = "ثبت";
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                //ErrorLog.SaveLog(ex);
                var dialog = new MessageDialog(ex.Message + " * " + ((ex.InnerException != null) ? ex.InnerException.Message : ""));
                dialog.Title = "خطای ثبت تنظیمات";
                await dialog.ShowAsync();
            }
        }

        private void SetSerialSettingDefault()
        {
            cmbBaudRate.SelectedIndex = cmbDataBits.SelectedIndex = cmbParity.SelectedIndex = cmbStopBits.SelectedIndex = 0;
            //tbxReadTimeOut.Text = tbxWriteTimeOut.Text = "1000";
        }

        /// <summary>
        /// دکمه ورود به داشبورد
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuBtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button_Click_1(null, null);
                this.Frame.Navigate(typeof(Dashboard), new SeriClass()
                {
                    Baudrate = baudrate,
                    Parity = parity,
                    Stopbits = stopbits,
                    Databits = databits,
                    Readtimeout = readtimeout,
                    Writeout = writeout
                });
            }
            catch (Exception ex)
            {
                //ErrorLog.SaveLog(ex);
            }
        }
        /// <summary>
        /// دکمه ورود به فرم تست ارسال
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuBtnTestForm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button_Click_1(null, null);
                this.Frame.Navigate(typeof(MainPage), new SeriClass()
                {
                    Baudrate = baudrate,
                    Parity = parity,
                    Stopbits = stopbits,
                    Databits = databits,
                    Readtimeout = readtimeout,
                    Writeout = writeout
                });
            }
            catch (Exception ex)
            {
                //ErrorLog.SaveLog(ex);
            }
        }
        /// <summary>
        /// دکمه بروزرسانی اطلاعات از مرکز
        /// که تمامی اطلاعات اولیه را از مرکز دانلود و در دیتابیس اسکیوالایت کنترلر ذخیره میکند
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void menuBtnDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new MessageDialog("اطلاعات بروزرسانی می شود ، آیا اطمینان دارید؟");
                dialog.Title = "توجه";
                dialog.Commands.Add(new UICommand { Label = "بله", Id = 0 });
                dialog.Commands.Add(new UICommand { Label = "خیر", Id = 1 });
                var res = await dialog.ShowAsync();
                if ((int)res.Id == 0)
                {
                    if (!string.IsNullOrWhiteSpace(tbxSqlIP.Text.Trim()))
                    {
                        if (Extension.IsSqlServerAvailable(tbxSqlIP.Text.Trim()))
                        {
                            LoadingIndicator.IsActive = true;
                            await Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                            {
                                DTS.Init(tbxSqlIP.Text.Trim());
                                DTS.FullDownload();
                            });
                            //await Task.Run(() => DTS.FullDownload((string.IsNullOrWhiteSpace(tbxSqlIP.Text.Trim()) ? "" : tbxSqlIP.Text.Trim())));
                        }
                        else
                        {
                            var dialogg = new MessageDialog("اتصال به پایگاه داده مرکز برقرار نیست");
                            dialogg.Title = "خطا در اتصال به پایگاه داده";
                            await dialogg.ShowAsync();
                        }
                    }
                    else
                    {
                        var dialogg = new MessageDialog("آی پی آدرس سرور مرکز بایستی پر باشد");
                        dialogg.Title = "توجه";
                        await dialogg.ShowAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                //ErrorLog.SaveLog(ex);
                var dialog = new MessageDialog(ex.Message + " * " + ((ex.InnerException != null) ? ex.InnerException.Message : ""));
                dialog.Title = "خطای انتقال";
                await dialog.ShowAsync();
            }
            finally
            {
                LoadingIndicator.IsActive = false;
                BindCombo();
                BindSetting();
            }
        }
        /// <summary>
        /// دکمه شات دان سیستم
        /// عملا شات دان ویندوز
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void menuBtnShutdown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new MessageDialog("دستگاه خاموش می شود ، آیا اطمینان دارید؟");
                dialog.Title = "توجه";
                dialog.Commands.Add(new UICommand { Label = "بله", Id = 0 });
                dialog.Commands.Add(new UICommand { Label = "خیر", Id = 1 });
                var res = await dialog.ShowAsync();
                if ((int)res.Id == 0)
                {
                    Windows.System.ShutdownManager.BeginShutdown(Windows.System.ShutdownKind.Shutdown, TimeSpan.FromSeconds(1));
                }
            }
            catch (Exception ex)
            {
                //ErrorLog.SaveLog(ex);
            }
        }

        private void MainCommandBar_Closed(object sender, object e)
        {
            MainCommandBar.IsOpen = true;
        }
        /// <summary>
        /// دکمه ریستارت سیستم
        /// عملا ریست ویندوز
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void menuBtnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new MessageDialog("دستگاه ریستارت می شود ، آیا اطمینان دارید؟");
                dialog.Title = "توجه";
                dialog.Commands.Add(new UICommand { Label = "بله", Id = 0 });
                dialog.Commands.Add(new UICommand { Label = "خیر", Id = 1 });
                var res = await dialog.ShowAsync();
                if ((int)res.Id == 0)
                {
                    Windows.System.ShutdownManager.BeginShutdown(Windows.System.ShutdownKind.Restart, TimeSpan.FromSeconds(1));
                }
            }
            catch (Exception ex)
            {

            }
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SetSerialSettingDefault();
        }

        private int Toint(string i)
        {
            try
            {
                return int.Parse(i);
            }
            catch (Exception)
            {
                return 1000;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            readtimeout = 1000;
            writeout = 1000;

            switch (cmbBaudRate.SelectionBoxItem)
            {
                case "9600":
                    baudrate = "9600";
                    break;
                case "14400":
                    baudrate = "14400";
                    break;
                case "19200":
                    baudrate = "19200";
                    break;
                case "28800":
                    baudrate = "28800";
                    break;
                case "38400":
                    baudrate = "38400";
                    break;
                case "56000":
                    baudrate = "56000";
                    break;
                case "57600":
                    baudrate = "57600";
                    break;
                case "115200":
                    baudrate = "115200";
                    break;
            }
            switch (cmbParity.SelectionBoxItem)
            {
                case "None":
                    parity = "None";
                    break;
                case "Even":
                    parity = "Even";
                    break;
                case "Mark":
                    parity = "Mark";
                    break;
                case "Odd":
                    parity = "Odd";
                    break;
                case "Space":
                    parity = "Space";
                    break;
            }
            switch (cmbStopBits.SelectionBoxItem)
            {
                case "One":
                    stopbits = "One";
                    break;
                case "OnePointFive":
                    stopbits = "OnePointFive";
                    break;
                case "Two":
                    stopbits = "Two";
                    break;
            }
            switch (cmbDataBits.SelectionBoxItem)
            {
                case "8":
                    databits = "8";
                    break;
                case "9":
                    databits = "9";
                    break;
            }
        }


    }
}