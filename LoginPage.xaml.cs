using SerialSample;
using SerialSample.DBLayer;
using System;
using System.Linq;
using System.Timers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
namespace THTController
{
    /// <summary>
    /// فرم لاگین
    /// استفاده شده هنگام ورود به تنظیمات
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        private string rootPage;
        private DatabaseHelperClass localDb;
        private string sqlip;
        private static ErrorLog errorLog;
        private static Timer _closetimer;
        public LoginPage()
        {
            this.InitializeComponent();
            //_closetimer = new Timer(300000);
            _closetimer = new Timer(60000);
            _closetimer.Elapsed += _closetimer_Elapsed;
        }

        private void _closetimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            cancelbtn_Click(null, null);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var param = e.Parameter as string[];
            rootPage = param[0];
            sqlip = param[1];
            localDb = new DatabaseHelperClass(sqlip);
            errorLog = new ErrorLog(sqlip);
        }

        /// <summary>
        /// دکمه ورود
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void loginbtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(tbxUserName.Text.Trim()) || string.IsNullOrWhiteSpace(tbxPassword.Text.Trim()))
                {
                    var dialog = new MessageDialog("نام کاربری یا رمز عبور را وارد نمایید");
                    dialog.Title = "خطا";
                    await dialog.ShowAsync();
                }
                var users = localDb.GetAllUsers();
                if(users != null && 
                   users.Count > 0 && 
                   users.FirstOrDefault(l=>l.UserName == tbxUserName.Text.Trim()) != null && 
                   users.FirstOrDefault(l => l.UserName == tbxUserName.Text.Trim()).UserPassword == tbxPassword.Text.Trim())
                {
                    this.Frame.Navigate(typeof(frmConfig), null);
                }
                else
                {
                    var dialogg = new MessageDialog("نام کاربری یا رمز عبور اشتباه است");
                    dialogg.Title = "خطا";
                    await dialogg.ShowAsync();
                }
            }
            catch(Exception ex)
            {
                errorLog.SaveLog(ex);
                throw;
            }
        }

        /// <summary>
        /// دکمه کنسل
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelbtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (rootPage)
                {
                    case "Dashboard":
                        this.Frame.Navigate(typeof(Dashboard), new SeriClass()
                        {
                            Baudrate = "9600",
                            Parity = "None",
                            Stopbits = "One",
                            Databits = "8",
                            Readtimeout = 1000,
                            Writeout = 1000
                        });
                        break;
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
