
using SerialSample.DBLayer;
//using SQLite;
using SQLite.Net;
//using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using THTController;
using THTController.DBLayer;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace SerialSample
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static string DB_PATH = Path.Combine(Path.Combine(ApplicationData.Current.LocalFolder.Path, "THT.sqlite"));//DataBase Name 
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            if (!CheckFileExists("THT.sqlite").Result)
            {
                var plat = new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT();
                plat.SQLiteApi.Config(SQLite.Net.Interop.ConfigOption.MultiThread);
                using (var db = new SQLiteConnection(plat, DB_PATH))
                {
                    db.CreateTable<DeviceTypeEntity>();
                    db.CreateTable<DeviceEntity>();
                    db.CreateTable<InstructionEntity>();
                    db.CreateTable<ResultEntity>();
                    db.CreateTable<SiteEntity>();
                    db.CreateTable<UserEntity>();
                    db.CreateTable<DeviceDashboardItemEntity>();
                    db.CreateTable<DeviceInstructionFireEntity>();
                    db.CreateTable<SettingEntity>();
                    db.CreateTable<DashboardLogEntity>();
                    db.CreateTable<DashboardPriorityEntity>();
                    db.CreateTable<DashboardScenarioEntity>();
                    db.CreateTable<ErrorLogEntity>();
                    db.CreateTable<GPIOHistoryEntity>();
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
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            try
            {
#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    this.DebugSettings.EnableFrameRateCounter = true;
                }
#endif

                Frame rootFrame = Window.Current.Content as Frame;

                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (rootFrame == null)
                {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    rootFrame = new Frame();

                    rootFrame.NavigationFailed += OnNavigationFailed;

                    if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                    {
                        //TODO: Load state from previously suspended application
                    }

                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;
                }

                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    var users = new DBLayer.DatabaseHelperClass(string.Empty).GetAllSettings();
                    if (users != null && users.Count > 0)
                    {
                        rootFrame.Navigate(typeof(Dashboard), new SeriClass()
                        {
                            Baudrate = "9600",
                            Parity = "None",
                            Stopbits = "One",
                            Databits = "8",
                            Readtimeout = 1000,
                            Writeout = 1000
                        });
                    }
                    else
                    {
                        rootFrame.Navigate(typeof(frmConfig), e.Arguments);
                    }
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
            catch (Exception ex)
            {
                var Error = ex.Message + "#" + (ex.InnerException?.Message ?? "") + "#" + (ex.InnerException?.InnerException?.Message ?? "");
                var stacktrace = ex.StackTrace + "#" + (ex.InnerException?.StackTrace ?? "") + "#" + (ex.InnerException?.InnerException?.StackTrace ?? "");
                var dialog = new MessageDialog(Error + Environment.NewLine + stacktrace);
                dialog.Title = "خطای ثبت تنظیمات";
                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
