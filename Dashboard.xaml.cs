using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Windows.Devices.Gpio;
using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;
using SerialSample.DBLayer;
using THTController;
using Windows.UI.Popups;
using System.Runtime.InteropServices;
using THTController.Date;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using THTController.DBLayer;
using Windows.UI.Xaml.Media.Imaging;
using System.Data;
using System.Diagnostics;
using Newtonsoft.Json;

namespace SerialSample
{
    /// <summary>
    /// ابرفرم داشبورد
    /// تمامی پردازش های ارسال دریافت سریال و همچنین تمامی پردازش های زمان دار از قبیل ارسال لاگ ها به مرکز و دانلود ساعت سرور و ... در این فرم انجام می شود
    /// در کل 95 درصد فشار و پردازش برنامه در همین فرم می باشد
    /// </summary>
    public sealed partial class Dashboard : Page
    {
        #region [Variables]
        //private int red_state = 1;
        //private bool green_state = false;
        //private const int RED = 23;
        //private const int GREEN = 24;
        /// <summary>
        /// جی پی آی اوها پایه های سخت افزاری ورودی خروجی تعبیه شده روی بورد رسپری می باشند
        /// که از پایه های به شماره ذیل در پروژه استفاده کردیم
        /// ی جا باید پایه رو 1 کنیم و یه جا 0 ش کنیم
        /// </summary>
        private const int GPIO_12 = 12;
        private const int GPIO_13 = 13;
        private const int GPIO_16 = 16;
        private const int GPIO_19 = 19;
        private const int GPIO_20 = 20;
        private const int GPIO_21 = 21;
        private const int GPIO_25 = 25;
        private const int GPIO_26 = 26;
        private GpioPin GPIOPIN_12;
        private GpioPin GPIOPIN_13;
        private GpioPin GPIOPIN_16;
        private GpioPin GPIOPIN_19;
        private GpioPin GPIOPIN_20;
        private GpioPin GPIOPIN_21;
        private GpioPin GPIOPIN_25;
        private GpioPin GPIOPIN_26;
        //private GpioPin redPin;
        //private GpioPin greenPin;
        /// <summary>
        /// پورت سریال دستگاه
        /// که از همین پورت سریال و ارتباط سریال برای ارتباط با دستگاه های سخت افزاری دیگر استفاده میکنیم
        /// عملا کامندهای هر دستگاه موجود در شبکه سریال رسپری را روی همین پورت ارسال میکنیم و جواب را هم از دستگاه روی همین پورت سریال میگیریم
        /// </summary>
        private SerialDevice serialPort = null;
        /// <summary>
        /// آبجکت نوشتن روی پورت سریال
        /// </summary>
        private DataWriter dataWriteObject = null;
        /// <summary>
        /// آبجکت خواندن از پورت سریال
        /// </summary>
        private DataReader dataReaderObject = null;
        /// <summary>
        /// لیست پورت های سریال موجود و آماده به کار دستگاه در این لیست نگهداری می شود
        /// که برای رسپری یک پورت فعال و اماده بکار وجود دارد
        /// </summary>
        private ObservableCollection<DeviceInformation> listOfDevices;
        /// <summary>
        /// آبکت کنسل کردن ارتباط سریال حین خواندن
        /// </summary>
        private CancellationTokenSource ReadCancellationTokenSource;
        /// <summary>
        /// این چهار متغیر برای نگهداری ویژگی های پورت سریال می باشد اعم از بادریت و پریتی و ....
        /// </summary>
        private string baudrate, parity, stopbits, databits;
        /// <summary>
        /// تایم اوت رید و رایت ارتباط سریال
        /// </summary>
        private int readtimeout, writeout;
        /// <summary>
        /// این متغیر رشته ای برای نگهداری کامندی که میخاهیم روی پورت سریال بفرستیم می باشد
        /// </summary>
        public static string OutString;
        /// <summary>
        /// این متغیر رشته ای برای نگهداری بایت های دریافت شده از پورت سریال می باشد
        /// </summary>
        public static string InString;
        /// <summary>
        /// این متغیر برای لاک کردن عملیات ارسال وقتی ی کامند فرستادیم برای دستگاهی و منتظر دریافت جواب ازش هستیم می باشد
        /// چون درخواست ها روی ی صف اماده ارسال به پورت سریال می باشد بنابرین وقتی ی درخواست رفته باید تا مشخص شدن جوابش یا تایم اوت خوردن جوابش صبر کنیم بعد این متغیر رو ترو کنیم
        /// و با ترو شدن این لاک ، ترد ارسال درخواست ها روی پورت سریال ، درخواست بعدی رو میفرسته
        /// </summary>
        private static bool SLOCK; // lock when a data send and wait for result or TimeOut after 5 s when device not response to us.
        /// <summary>
        /// کلاس کار با دیتابیس اسکیوال سرور مرکز همون وب سایت
        /// </summary>
        private static SqlServerRepository sqlDb;
        /// <summary>
        /// کلاس کار با دیتابیس لوکال رسپری که از جنس اسکیوالایت می باشد
        /// </summary>
        private static DatabaseHelperClass localDb;
        /// <summary>
        /// این ترد برای پردازش دانلود ساعت سرور مرکزی و ست کردن روی ویندوز رسپری استفاده میشه
        /// </summary>
        private Thread DateTimeThread; //  to download sql server datetime on local machine
        /// <summary>
        /// این ترد برای نمایش تاریخ و ساعت روی فرم داشبورد استفاده می شود
        /// </summary>
        private Thread ClockThread; // to show clock on dashboard form
        /// <summary>
        /// این ترد جهت دانلود درخواست های کاربر سایت که ما بهش میگیم فایرهای سایت استفاده می شود
        /// </summary>
        private Thread FireCheckThread; // to process fires
        /// <summary>
        /// این ترد جهت ارسال درخواست های موجود در صف روی پورت سریال می باشد
        /// </summary>
        private Thread SenderThread; // to send shots
        /// <summary>
        /// این ترد جهت آپدیت اطلاعات روی داشبورد می باشد
        /// </summary>
        private Thread DashboardUIThread;

        private Thread DeviceBalanceDownloadThread;

        /// <summary>
        /// این لیست تردها جهت ریختن درخواست های زماندار توی صف اصلی می باشد
        /// چرا لیست گرفتیم چون درخواست های زماندار با اولویت های مختلفی وجود داره و بستگی داره کاربر سایت چیا تعریف کرده باشه
        /// مثلا اگر درخواست های زماندارمون با اولویت های 3 ثانیه 5 ثانیه و 500 ثانیه باشه
        /// پس اینجا به طور اتومات توی لیست ذیل سه تا ترد نیو میشه با تایم های مورد نظر که توی تایم مشخص درخواست های زماندار مبتنی با همون تایم رو اتومات توی صف اضافه خواهد کرد
        /// </summary>
        private List<Thread> DashboardThreads; // to process dashboarditems with diffrent priority
        /// <summary>
        /// تایمر زیر برای محاسبه تایم اوت دستگاه ها می باشد
        /// یعنی وقتی رسپری کامندی رو میفرسته روی پورت سریال و دستگاه مفعول جواب نداد اگر 5 ثانیه طول کشید که این 5 توسط تایمر ذیل کانتر میشه
        /// اونوقت جواب تایم اوت ثبت خواهیم کرد 
        /// </summary>
        private static System.Timers.Timer SlockTimer; // stopwatch to check devices timeout and put Timeout value for requests
        /// <summary>
        /// صف درخواست ها
        /// درخواست ها هم از جنس کلاس شات هست
        /// که ممکنه درخواستمون از یکی از سه نوع ذیل باشه
        /// یا درخواست زمانداره که اتومات و طبق تایم مشخص شده اضافه شده به صف
        /// یا درخواست کاربر سایت هست که به شکل فایر بوده و دانلود شده و به صف اضافه شده
        /// یا درخواست کاربر لوکال رسپریه که از روی مانتیور هفت اینچی و فرم یوای خوده برنامه ثبت شده
        /// </summary>
        private static Queue<Shot> ShotQueue; // to handle all shots into serial port
        /// <summary>
        /// تایمر ذیل جهت آپلود لاگ ها روی دیتابیس اسکیوال سرور مرکزی می باشد
        /// عملا اینزرت به اسکیوال سرور سایت
        /// </summary>
        private Thread UploadLogThread; // to send DashboardItemLog To Sql server   
        /// <summary>
        /// کش انتیتی شامل لیستی از تمام اطلاعات اولیه دیتابیس می باشد اعم از لیست دستگاه ها ، درخواست های زماندار و دستورالعمل ها و ...
        /// چرا همچین ابجکتی گرفتیم و توی رم نگه میداریم به چند علت
        /// اول اینکه دیتابیس اسکیولایت نمیتونه توسط چندین ترد دسترسی داشته باشیم بهش فقط یه ترد میتونه بهش دسترسی داشته باشه و ما توی ترد های مختلف نیاز داریم بهش
        /// پس اطلاعات رو توی لود اولیه فرم داشبورد میاریم توی این ابجکت کش انتیتی که توی رم باشه و دیگه نیاز به اسکیولایت خارکسده نداشته باشیم
        /// دوم اینکه از نظر سرعت آی او هم به نفعمون شد دسترسی به رم اجرایی تا حافظه زن جنده خیلی بهتره
        /// </summary>
        private static CacheEntity LocalCache;
        /// <summary>
        /// کلاس ذیل جهت لاگ کردن اکسپشن هایی که میخوریم توی دیتابیس اسکیوال سرور مرکز می باشد
        /// </summary>
        private static ErrorLog _errorLog;
        #endregion

        #region [Init]
        /// <summary>
        /// اونت وقتی اجرا میشه که وارد فرم جاری میشیم
        /// اطلاعات تنظیمات پورت سریالو که بهش پاس دادیم میشونیم تو متغیرای مورد نظر
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var obj = e.Parameter as SeriClass;
            baudrate = obj.Baudrate;
            parity = obj.Parity;
            stopbits = obj.Stopbits;
            databits = obj.Databits;
            readtimeout = obj.Readtimeout;
            writeout = obj.Writeout;
        }
        /// <summary>
        /// سازنده فرم
        /// </summary>
        public Dashboard()
        {
            this.InitializeComponent();
            listOfDevices = new ObservableCollection<DeviceInformation>();
            // تمامی اطلاعات اولیه مورد نیاز از دیتابیس اسکیوالایت به آبجکت کش انتیتی اینزرت و نگهداری می شود
            InitCache();
            // آی پی رسپری برای نمایش در پایین سمت چپ فرم
            lblIpAddresss.Text = Extension.LocalIPAddress;
            _errorLog = new ErrorLog(LocalCache.Setting.SQLServerIP);
            sqlDb = new SqlServerRepository(LocalCache.Setting.SQLServerIP);
            localDb = new DatabaseHelperClass(LocalCache.Setting.SQLServerIP);
            lblCurrentSiteName.Text = (LocalCache.Sites != null && LocalCache.Sites.Count > 0)
                ? LocalCache.Sites.FirstOrDefault(l => l.ID == LocalCache.Setting.SiteID).Name
                : "";
            ListAvailablePorts();
            InitGPIO();
            ShotQueue = new Queue<Shot>();
            //
            BindPivotItems();
            //
            SLOCK = false;
            SlockTimer = new System.Timers.Timer(5000);
            SlockTimer.Elapsed += (sender, e) => HandleTimer();
            FireCheckThread = new Thread(new ThreadStart(CheckInstructionFire))
            {
                IsBackground = true,
                Name = "Instruction Fire Processing Thread"
            };
            FireCheckThread.Start();
            DateTimeThread = new Thread(new ThreadStart(SetSystemDate))
            {
                IsBackground = true,
                Name = "GetDateTime Processing Thread"
            };
            DateTimeThread.Start();
            ClockThread = new Thread(new ThreadStart(UpdateClock))
            {
                IsBackground = true,
                Name = "CLOCK Processing Thread"
            };
            ClockThread.Start();
            SenderThread = new Thread(new ThreadStart(SendProcessing))
            {
                IsBackground = true,
                Name = "Shots Sender Thread"
            };
            SenderThread.Start();
            DashboardUIThread = new Thread(new ThreadStart(UpdateDashboardUIGrid))
            {
                IsBackground = true,
                Name = "DashboardUI Thread"
            };
            DashboardUIThread.Start();
            DashboardThreads = new List<Thread>();
            foreach (var priority in LocalCache.DashboardPriorities)
            {
                ThreadStart starter = delegate { CheckDashboardItems(priority.Value); };
                var th = new Thread(starter)
                {
                    IsBackground = true,
                    Name = "Dashboard Processing Thread " + priority.Value.ToString()
                };
                DashboardThreads.Add(th);
            }
            DashboardThreads.ForEach(l => l.Start());
            UploadLogThread = new Thread(new ThreadStart(UploadDashboardLogsToServer))
            {
                IsBackground = true,
                Name = "upload logs to sql Thread"
            };
            UploadLogThread.Start();

            DeviceBalanceDownloadThread = new Thread(new ThreadStart(DownloadDeviceChargeLog))
            {
                IsBackground = true,
                Name = "get device balance chatge log Thread"
            };
            DeviceBalanceDownloadThread.Start();
        }
        private void InitCache()
        {
            try
            {
                var db = new DatabaseHelperClass(string.Empty);
                LocalCache = new CacheEntity
                {
                    Users = db.GetAllUsers(),
                    Sites = db.GetAllSites(),
                    DeviceTypes = db.GetAllDeviceTypes(),
                    DashboardPriorities = db.GetAllDashboardPriority(),
                    Devices = db.GetAllDevices(),
                    Instructions = db.GetAllInstructions(),
                    Results = db.GetAllResults(),
                    DashboardItems = db.GetAllDeviceDashboardItems(),
                    DashboardScenarios = db.GetAllDashboardScenario(),
                    Setting = db.GetSetting(0),
                    DashboardLogs = new List<DashboardLogEntity>()
                };
            }
            catch (Exception ex)
            {
                ShowError(ex); _errorLog.SaveLog(ex);
                throw ex;
            }
        }
        #endregion

        #region [GPIO]
        /// <summary>
        /// در این تابع مقادیر پایه های فیزیکی خروجی ورودی بورد را به آخرین مقداری که از قبل داشتن ست میکنیم
        /// چون وقتی ست میکنیم اینها رو در دیتابیس لوکال اخرین وضعیتشون رو ست میکنیم
        /// وقتی هم میخایم اینیت کنیم از دیتابیس میخونیم اخرین مقدارشو ست میکنیم
        /// </summary>
        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();
            var gpiohistorylst = localDb.GetAllGPIO();
            if (GPIOPIN_12 == null)
            {
                GPIOPIN_12 = gpio.OpenPin(GPIO_12);
                GPIOPIN_12.SetDriveMode(GpioPinDriveMode.Output);
            }

            if (gpiohistorylst != null && gpiohistorylst.Find(l => l.GPIO == 12) != null &&
                DateTime.Now.Subtract(gpiohistorylst.Find(l => l.GPIO == 12).SaveTime).TotalMinutes <= 15)
            {
                GPIOPIN_12.Write(gpiohistorylst.Find(l => l.GPIO == 12).GPIOValue == 1
                    ? GpioPinValue.High
                    : GpioPinValue.Low);
                localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                {
                    GPIO = 12,
                    GPIOValue = gpiohistorylst.Find(l => l.GPIO == 12).GPIOValue,
                    SaveTime = DateTime.Now
                });
            }
            else
            {
                GPIOPIN_12.Write(GpioPinValue.High);
                localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                {
                    GPIO = 12,
                    GPIOValue = 1,
                    SaveTime = DateTime.Now
                });
            }
            

            if (GPIOPIN_13 == null)
            {
                GPIOPIN_13 = gpio.OpenPin(GPIO_13);
                GPIOPIN_13.SetDriveMode(GpioPinDriveMode.Output);
            }
            if (gpiohistorylst != null && gpiohistorylst.Find(l => l.GPIO == 13) != null &&
                DateTime.Now.Subtract(gpiohistorylst.Find(l => l.GPIO == 13).SaveTime).TotalMinutes <= 15)
            {
                GPIOPIN_13.Write(gpiohistorylst.Find(l => l.GPIO == 13).GPIOValue == 1
                    ? GpioPinValue.High
                    : GpioPinValue.Low);
                localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                {
                    GPIO = 13,
                    GPIOValue = gpiohistorylst.Find(l => l.GPIO == 13).GPIOValue,
                    SaveTime = DateTime.Now
                });
            }
            else
            {
                GPIOPIN_13.Write(GpioPinValue.High);
                localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                {
                    GPIO = 13,
                    GPIOValue = 1,
                    SaveTime = DateTime.Now
                });
            }


            if (GPIOPIN_16 == null)
            {
                GPIOPIN_16 = gpio.OpenPin(GPIO_16);
                GPIOPIN_16.SetDriveMode(GpioPinDriveMode.Output);
            }
            if (gpiohistorylst != null && gpiohistorylst.Find(l => l.GPIO == 16) != null &&
                DateTime.Now.Subtract(gpiohistorylst.Find(l => l.GPIO == 16).SaveTime).TotalMinutes <= 15)
            {
                GPIOPIN_16.Write(gpiohistorylst.Find(l => l.GPIO == 16).GPIOValue == 1
                    ? GpioPinValue.High
                    : GpioPinValue.Low);
                localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                {
                    GPIO = 16,
                    GPIOValue = gpiohistorylst.Find(l => l.GPIO == 16).GPIOValue,
                    SaveTime = DateTime.Now
                });
            }
            else
            {
                GPIOPIN_16.Write(GpioPinValue.High);
                localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                {
                    GPIO = 16,
                    GPIOValue = 1,
                    SaveTime = DateTime.Now
                });
            }

            if (GPIOPIN_19 == null)
            {
                GPIOPIN_19 = gpio.OpenPin(GPIO_19);
                GPIOPIN_19.SetDriveMode(GpioPinDriveMode.Output);
            }
            if (gpiohistorylst != null && gpiohistorylst.Find(l => l.GPIO == 19) != null &&
                DateTime.Now.Subtract(gpiohistorylst.Find(l => l.GPIO == 19).SaveTime).TotalMinutes <= 15)
            {
                GPIOPIN_19.Write(gpiohistorylst.Find(l => l.GPIO == 19).GPIOValue == 1
                    ? GpioPinValue.High
                    : GpioPinValue.Low);
                localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                {
                    GPIO = 19,
                    GPIOValue = gpiohistorylst.Find(l => l.GPIO == 19).GPIOValue,
                    SaveTime = DateTime.Now
                });
            }
            else
            {
                GPIOPIN_19.Write(GpioPinValue.High);
                localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                {
                    GPIO = 19,
                    GPIOValue = 1,
                    SaveTime = DateTime.Now
                });
            }

            if (GPIOPIN_20 == null)
            {
                GPIOPIN_20 = gpio.OpenPin(GPIO_20);
                GPIOPIN_20.SetDriveMode(GpioPinDriveMode.Output);
            }
            if (gpiohistorylst != null && gpiohistorylst.Find(l => l.GPIO == 20) != null &&
                DateTime.Now.Subtract(gpiohistorylst.Find(l => l.GPIO == 20).SaveTime).TotalMinutes <= 15)
            {
                GPIOPIN_20.Write(gpiohistorylst.Find(l => l.GPIO == 20).GPIOValue == 1
                    ? GpioPinValue.High
                    : GpioPinValue.Low);
                localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                {
                    GPIO = 20,
                    GPIOValue = gpiohistorylst.Find(l => l.GPIO == 20).GPIOValue,
                    SaveTime = DateTime.Now
                });
            }
            else
            {
                GPIOPIN_20.Write(GpioPinValue.High);
                localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                {
                    GPIO = 20,
                    GPIOValue = 1,
                    SaveTime = DateTime.Now
                });
            }

            if (GPIOPIN_21 == null)
            {
                GPIOPIN_21 = gpio.OpenPin(GPIO_21);
                GPIOPIN_21.SetDriveMode(GpioPinDriveMode.Output);
            }
            if (gpiohistorylst != null && gpiohistorylst.Find(l => l.GPIO == 21) != null &&
                DateTime.Now.Subtract(gpiohistorylst.Find(l => l.GPIO == 21).SaveTime).TotalMinutes <= 15)
            {
                GPIOPIN_21.Write(gpiohistorylst.Find(l => l.GPIO == 21).GPIOValue == 1
                    ? GpioPinValue.High
                    : GpioPinValue.Low);
                localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                {
                    GPIO = 21,
                    GPIOValue = gpiohistorylst.Find(l => l.GPIO == 21).GPIOValue,
                    SaveTime = DateTime.Now
                });
            }
            else
            {
                GPIOPIN_21.Write(GpioPinValue.High);
                localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                {
                    GPIO = 21,
                    GPIOValue = 1,
                    SaveTime = DateTime.Now
                });
            }

            if (GPIOPIN_25 == null)
            {
                GPIOPIN_25 = gpio.OpenPin(GPIO_25);
                GPIOPIN_25.SetDriveMode(GpioPinDriveMode.Output);
            }
            if (gpiohistorylst != null && gpiohistorylst.Find(l => l.GPIO == 25) != null &&
                DateTime.Now.Subtract(gpiohistorylst.Find(l => l.GPIO == 25).SaveTime).TotalMinutes <= 15)
            {
                GPIOPIN_25.Write(gpiohistorylst.Find(l => l.GPIO == 25).GPIOValue == 1
                    ? GpioPinValue.High
                    : GpioPinValue.Low);
                localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                {
                    GPIO = 25,
                    GPIOValue = gpiohistorylst.Find(l => l.GPIO == 25).GPIOValue,
                    SaveTime = DateTime.Now
                });
            }
            else
            {
                GPIOPIN_25.Write(GpioPinValue.High);
                localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                {
                    GPIO = 25,
                    GPIOValue = 1,
                    SaveTime = DateTime.Now
                });
            }

            if (GPIOPIN_26 == null)
            {
                GPIOPIN_26 = gpio.OpenPin(GPIO_26);
                GPIOPIN_26.SetDriveMode(GpioPinDriveMode.Output);
            }
            if (gpiohistorylst != null && gpiohistorylst.Find(l => l.GPIO == 26) != null &&
                DateTime.Now.Subtract(gpiohistorylst.Find(l => l.GPIO == 26).SaveTime).TotalMinutes <= 15)
            {
                GPIOPIN_26.Write(gpiohistorylst.Find(l => l.GPIO == 26).GPIOValue == 1
                    ? GpioPinValue.High
                    : GpioPinValue.Low);
                localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                {
                    GPIO = 26,
                    GPIOValue = gpiohistorylst.Find(l => l.GPIO == 26).GPIOValue,
                    SaveTime = DateTime.Now
                });
            }
            else
            {
                GPIOPIN_26.Write(GpioPinValue.High);
                localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                {
                    GPIO = 26,
                    GPIOValue = 1,
                    SaveTime = DateTime.Now
                });
            }

        }
        //public void BeforeSend(TimeSpan delay)
        //{
        //    //redPin.Write(GpioPinValue.High);
        //    //greenPin.Write(GpioPinValue.Low);
        //    Task.Delay(delay).Wait();
        //}
        //public void AfterSend(TimeSpan delay)
        //{
        //    Task.Delay(delay).Wait();
        //    //redPin.Write(GpioPinValue.Low);
        //    //greenPin.Write(GpioPinValue.High);
        //}
        #endregion

        #region [InitCOMPort]
        private async void ListAvailablePorts()
        {
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);
                for (int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Add(dis[i]);
                }
                DeviceListSource.Source = listOfDevices;
                comPortInput_Click(null, null);
            }
            catch (Exception ex)
            {
                ShowError(ex); _errorLog.SaveLog(ex);
                throw ex;
            }
        }

        private async void comPortInput_Click(object sender, RoutedEventArgs e)
        {
            var selection = listOfDevices;

            if (selection.Count <= 0)
            {
                return;
            }
            DeviceInformation entry = (DeviceInformation)selection[0];
            try
            {
                serialPort = await SerialDevice.FromIdAsync(entry.Id);
                if (serialPort == null) return;
                serialPort.WriteTimeout = TimeSpan.FromMilliseconds(writeout);
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(readtimeout);
                serialPort.BaudRate = uint.Parse(baudrate);
                switch (parity)
                {
                    case "None":
                        serialPort.Parity = SerialParity.None;
                        break;
                    case "Even":
                        serialPort.Parity = SerialParity.Even;
                        break;
                    case "Mark":
                        serialPort.Parity = SerialParity.Mark;
                        break;
                    case "Odd":
                        serialPort.Parity = SerialParity.Odd;
                        break;
                    case "Space":
                        serialPort.Parity = SerialParity.Space;
                        break;
                }
                switch (stopbits)
                {
                    case "One":
                        serialPort.StopBits = SerialStopBitCount.One;
                        break;
                    case "OnePointFive":
                        serialPort.StopBits = SerialStopBitCount.OnePointFive;
                        break;
                    case "Two":
                        serialPort.StopBits = SerialStopBitCount.Two;
                        break;
                }
                serialPort.DataBits = ushort.Parse(databits);
                serialPort.Handshake = SerialHandshake.None;
                //rcvdText.Text = "منتظر دریافت اطلاعات...";
                ReadCancellationTokenSource = new CancellationTokenSource();
                //sendTextButton.IsEnabled = true;
                Listen();
            }
            catch (Exception ex)
            {
                ShowError(ex); _errorLog.SaveLog(ex);
                throw ex;
                //status.Text = ex.Message;
                //sendTextButton.IsEnabled = false;
            }
        }
        private async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
                    dataReaderObject = new DataReader(serialPort.InputStream);
                    while (true)
                    {
                        await ReadAsync(ReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (TaskCanceledException tce)
            {
                //status.Text = "Reading task was cancelled, closing device and cleaning up";
                CloseDevice();
            }
            catch (Exception ex)
            {
                //ShowError(ex);
                _errorLog.SaveLog(ex);
                if(!ex.Message.ToLower().Contains("a task was canceled"))
                {
                    CoreApplication.Exit();
                }
                //if (ex.Message.ToLower().Contains("index was outside the bounds") || ex.Message.ToLower().Contains("indexoutofrangeexception"))
                //{
                //    CoreApplication.Exit();
                //}
                //throw ex;
                //status.Text = ex.Message;
            }
            finally
            {
                // Cleanup once complete
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }
        #endregion

        #region [SendToCOM]
        /// <summary>
        /// توابع ارسال به پورت کام
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void sendTextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (serialPort != null)
                {
                    dataWriteObject = new DataWriter(serialPort.OutputStream);
                    await WriteAsync();
                }
                else
                {
                    //status.Text = "Select a device and connect";
                }
            }
            catch (Exception ex)
            {
                ShowError(ex); _errorLog.SaveLog(ex);
                throw ex;
                //status.Text = "sendTextButton_Click: " + ex.Message;
            }
            finally
            {
                // Cleanup once complete
                if (dataWriteObject != null)
                {
                    dataWriteObject.DetachStream();
                    dataWriteObject = null;
                }
            }
        }
        /// <summary>
        /// تابع اصلی نوشتن کامندها روی پورت سریال
        /// رشته اماده شده در فرمت ذیل می باشد
        /// 2-8-5-6-1-8-5-56-256-129
        /// که اسپلیت می شود و بایت به بایت به پورت ارسال می شود
        /// هر بایت هم بایستی بین 0 تا 256 باشد تا مقدارش معتبر باشد
        /// </summary>
        /// <returns></returns>
        private async Task WriteAsync()
        {
            try
            {
                Task<UInt32> storeAsyncTask;
                // اسپلیت رشته با کاراکتر جداکننده دش
                var items = OutString.Split('-');
                foreach (var i in items)
                {
                    // ولیدیت میشه بین 0 تا 256 باشه
                    if (ValidateData(i))
                    {
                        continue;
                    }
                    else
                    {
                        throw new Exception("رشته ارسالی حاوی اطلاعات نامعتبر است مقدار بایستی به بایت باشد");
                    }
                }
                // حلقه روی بایت ها و نوشتن روی پورت سریال
                foreach (var i in items)
                {
                    dataWriteObject.WriteByte(Convert.ToByte(i));
                }

                //BeforeSend(TimeSpan.FromMilliseconds(0));
                storeAsyncTask = dataWriteObject.StoreAsync().AsTask();
                //AfterSend(TimeSpan.FromMilliseconds(0));
                Task.Delay(TimeSpan.FromMilliseconds(0)).Wait();
                UInt32 bytesWritten = await storeAsyncTask;
                // اگر بایت ها به درستی ارسال شد و اوکی بود 
                // رشته برای نگهداری کامند ارسالی رو خالی میکنیم که درخواست بعدی باز بنویسه روش و ادامه داستان
                if (bytesWritten > 0)
                {
                    //status.Text = sendText1.Text + ", ";
                    //status.Text += "اطلاعات با موفقیت ارسال شد";
                    Debug.WriteLine("Send:" + OutString);
                    OutString = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex); _errorLog.SaveLog(ex);
                throw ex;
            }
        }

        private static bool ValidateData(string bytedata)
        {
            try
            {
                if ((!string.IsNullOrWhiteSpace(bytedata) && Enumerable.Range(0, 256).Contains(Int16.Parse(bytedata))))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _errorLog.SaveLog(ex);
                return false;
            }
        }
        #endregion

        #region [DetachForm]
        private void CloseFormProcess()
        {
            //DateTimeThread.Abort();
            //ClockThread.Abort();
            //FireCheckThread.Abort();
            //SenderThread.Abort();
            //DashboardThreads.ForEach(l => l.Abort()); 
            //UploadLogThread.Abort();
            CancelReadTask();
            CloseDevice();
            //greenPin.Dispose();
            //redPin.Dispose();
            GPIOPIN_12.Dispose();
            GPIOPIN_13.Dispose();
            GPIOPIN_16.Dispose();
            GPIOPIN_19.Dispose();
            GPIOPIN_20.Dispose();
            GPIOPIN_21.Dispose();
            GPIOPIN_25.Dispose();
            GPIOPIN_26.Dispose();
        }
        private void menuBtnGoToSetting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CloseFormProcess();
                //this.Frame.Navigate(typeof(frmConfig), new SeriClass());
                this.Frame.Navigate(typeof(LoginPage), new string[] { "Dashboard", LocalCache.Setting.SQLServerIP });
            }
            catch (Exception ex)
            {
                //ShowError(ex);_errorLog.SaveLog(ex);
                //throw ex;
            }
        }
        #endregion

        #region [ReadFromCOM]
        /// <summary>
        /// از این سه تا دیتاتیبل جلوتر برای محاسبه فرمول ریاضی استفاده کردیم یعنی فقط برای تابع کامپیوت ازشون استفاده شده
        /// و استفاده ذخیره دیتا و این کسر شرا نشده 
        /// حالا چرا محاسبه فرمول مجبور شدیم با دیتاتیبل هندل کنیم چون کتابخونه های تردپارتی و همچنین یه کلاس که الانم توی پروژه هست ولی استفاده نشد بکنیم روی پلتفرم آی او تی ساپورت نمیشه
        /// CodedomCalculator.cs این کلاس برای محاسبه پویای فرمولای ریاضی توی پروژه اد شده ولی استفاده نشد بکنیم خطا میده که روی این نسخه از ویندوز 10 آی اوتی ساپورت نمیشه
        /// چون از داینامیک سی شارپ استفاده کرده برای محاسبه فرمول
        /// مام گفتیم به کیرم رفتیم تحریما و محدودیت ها رو دور زدیم رسیدیم به دیتاتیبل که تابع کامپیوت داره یعنی فرمول ریاضی بهش بدی جوابتو میده مثلا
        /// 3+10*500+900-800 
        /// بهش بدیم قشنگ محاسبه میکنه مام همینو میخایم
        /// </summary>
        private static DataTable firedt;
        private static DataTable dashitemdt;
        private static DataTable localfiredt;
        /// <summary>
        /// تابع دریافت اطلاعات از پورت سریال
        /// در همین تابع است که 8 بایت دریافتی از پورت سریال رو پردازش میکنیم جوابمونو از توش در میاریم و به عنوان لاگ ذخیره میکنیم که لاگ ها هم توی ترد دیگه ارسال خواهند شد به سرور
        /// راستی تعداد بایت هایی که ما میفرستیم روی پورت و میگیرم ازش 8 بایت هست تو استاندارد این پروژه
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@MUST REFACTOR AND CLEAN
        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            try
            {
                Task<UInt32> loadAsyncTask;
                uint ReadBufferLength = 256;
                cancellationToken.ThrowIfCancellationRequested();
                dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
                using (var childCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(childCancellationTokenSource.Token);
                    UInt32 bytesRead = await loadAsyncTask;
                    // اگر بایتی خونده شد از پورت با جداکننده دش میزاریمشون توی استرینگ 
                    // InString
                    // که جلوتر کلی باهاش کار داریم
                    if (bytesRead > 0)
                    {
                        InString = string.Empty;
                        for (var i = 0; i < bytesRead; i++)
                        {
                            InString += dataReaderObject.ReadByte() + "-";
                        }
                        InString = InString.Substring(0, InString.LastIndexOf("-"));
                        SlockTimer.Stop();

                        Debug.WriteLine("Receive:" + InString);

                        ShotQueue.TryPeek(out var shot);
                        if (shot == null)
                        {
                            SLOCK = false;
                            return;
                        }
                        //چک کردن اسلیو کد
                        // اسلیو کد چیست؟ در کامندهایی که میفرستیم به دستگاه ها و همچنین کامندهایی که دریافت میکنیم
                        // 12-32-225-121-265-9-15-5
                        // مثلا در کامند بالا
                        // چه دریافت باشد چه ارسال
                        // همیشه اولین بایت اسلیوکد است که شماره ادرس دستگاه سخت افزاری مورد نظر در شبکه مدباس مجتمع می باشد
                        // پس با این احتساب در یک مجتمع وقتی چندین دستگاه داریم که قراره همشونو منیج کنیم با اسلیوکدهای مختلف اول کامندها سروکار داریم
                        //حالا چند خط زیر برای اینه که چک کنیم وقتی کامندی رو فرستادیم روی شبکه مدباس سریال با اسلیو کد مثلا 4 یعنی برای دستگاه شماره چهار
                        // اگر جوابی که برامون میاد اسلیو کد کامندش 4 نباشه یعنی تداخلی رخ داده یعنی ی دستگاه خارکسده دیگه جواب داده که نباید میداده که ممکنه بابت نویز شبکه این حالت رخ بده
                        // یا هر باگ سخت افزاری دیگه
                        var testslave = int.Parse(InString.Split('-')[0]);
                        //if shot.device.rowno != testslave > پاسخ نامعتبر است تداخل
                        if (shot.DashboardItem != null)
                        {
                            var device = LocalCache.Devices.FirstOrDefault(l => l.ID == shot.DashboardItem.DeviceID);
                            if (device != null)
                            {
                                if (device.RowNow != testslave)
                                {
                                    shot.DashboardItem.Value = "پاسخ دستگاه نامعتبر است - تداخل";
                                    shot.DashboardItem.SaveTime = DateTime.UtcNow;
                                    LocalCache.DashboardLogs.Add(new DashboardLogEntity()
                                    {
                                        DashboardItemID = shot.DashboardItem.ID,
                                        Value = shot.DashboardItem.Value,
                                        SaveTime = DateTime.UtcNow,
                                        IsSended = false,
                                        ResultID = 0,
                                        CreateDate = DateTime.UtcNow,
                                        Description = "درخواست زمان دار"
                                    });
                                    if (ShotQueue.Count > 0)
                                    {
                                        ShotQueue.Dequeue();
                                    }
                                    SLOCK = false;
                                    return;
                                }
                            }
                        }
                        else if (shot.InstructionFire != null)
                        {
                            var device = LocalCache.Devices.FirstOrDefault(l => l.ID == shot.InstructionFire.DeviceID);
                            if (device != null)
                            {
                                if (device.RowNow != testslave)
                                {
                                    if (Extension.IsSqlServerAvailable(LocalCache.Setting.SQLServerIP))
                                    {
                                        sqlDb.UpdateInstructionFireOnServer(shot.InstructionFire.ID, "پاسخ دستگاه نامعتبر است - تداخل", 0);
                                    }
                                    if (ShotQueue.Count > 0)
                                    {
                                        ShotQueue.Dequeue();
                                    }
                                    SLOCK = false;  
                                    return;
                                }
                            }
                        }
                        else if (shot.LocalFire != null)
                        {
                            var device = LocalCache.Devices.FirstOrDefault(l => l.ID == shot.LocalFire.DeviceID);
                            if (device != null)
                            {
                                if (device.RowNow != testslave)
                                {
                                    shot.LocalFire.Value = "پاسخ دستگاه نامعتبر است - تداخل";
                                    LocalCache.DashboardLogs.Add(new DashboardLogEntity()
                                    {
                                        InstructionID = shot.LocalFire.InstructionID,
                                        Value = shot.LocalFire.Value,
                                        SaveTime = DateTime.UtcNow,
                                        IsSended = false,
                                        ResultID = 0,
                                        CreateDate = DateTime.UtcNow,
                                        Description = (shot.LocalFire.Type == "LocalFire") ? "درخواست کاربر کنترلر تتیس" : "عملیات حاصل سناریو"
                                    });
                                    if (shot.LocalFire.Type == "LocalFire")
                                    {
                                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { UpdateLocalFireResultUI(shot.LocalFire.Value); });
                                    }
                                    if (ShotQueue.Count > 0)
                                    {
                                        ShotQueue.Dequeue();
                                    }
                                    SLOCK = false;
                                    return;
                                }
                            }
                        }
                        // حالا که مطمینیم تداخلی نیست و جواب از همون دستگاه مادرجنده ای که براش درخواست فرستادیم اومده میریم سراغ پردازش پاسخی که اون خارکسده بهمون داده
                        // اول بایستی چک کنیم که درخواستی که سر صف هست و واسش پاسخ اومده از چه جنسی هست
                        // فعلا سه حالت داریم
                        // یا درخواست زمانداره که اتومات اضافه میشه به صف درخواست ها و پردازش میشه طبق تایمر اولویتش
                        // یا فایر سایته همون درخواستی که کاربر سایت میزنه و جوابشو همون روی سایت میخاد
                        // یا فایر کاربر خوده کنترلر هست که روی فرم همین برنامه درخواستو زده
                        var onlineshot = ShotQueue.Peek();
                        int _deviceid = 0, _instructionid = 0;
                        string _value = string.Empty;

                        // اگر درخواست کاربر سایت بود یا همون فایر سایت
                        if (onlineshot.InstructionFire != null)
                        {
                            // درآوردن اطلاعات اولیه از آبجکت کش
                            var fire = onlineshot.InstructionFire;
                            _deviceid = onlineshot.InstructionFire.DeviceID;
                            _instructionid = onlineshot.InstructionFire.InstructionID;
                            var device = LocalCache.Devices.FirstOrDefault(l => l.ID == fire.DeviceID);
                            var deviceresults = LocalCache.Results.Where(l => l.DeviceType == device.DeviceType).ToList();
                            var items = InString.Split('-');
                            if (items != null && items.Count() > 0)
                            {
                                var data = "";
                                var slaveid = items[0];
                                var funcode = items[1];
                                var crc1 = items[items.Count() - 2];
                                var crc2 = items[items.Count() - 1];
                                var bytelist = new List<byte>();
                                for (int i = 0; i < items.Count() - 2; i++)
                                {
                                    bytelist.Add(byte.Parse(items[i]));
                                    if (i > 1)
                                    {
                                        data += items[i] + "-";
                                    }
                                }
                                //1-134-18-1-2
                                // محاسبه سی آر سی پاسخ دریافت شده این سی آر سی هم موقع ارسال محاسبه میشه هم موقع دریافت و فرمولش مشخصه که توی تابع های کلاس اکستنشن زده شده
                                // اگر سی ار سی بایت های دریافت شده با سی ار سی محاسبه شده ما از بایت ها یکی بود یعنی درسته دیتا و حله
                                // و گرنه توی پاسخ سی آر سی ارور لاگ میکنیم
                                var crc = Extension.ComputeCRC(bytelist);
                                if (crc.CRC1 == int.Parse(crc1) && crc.CRC2 == int.Parse(crc2))
                                {
                                    // اگر کامند دریافتی در لیست پاسخ ها برای این دستگاه تعریف شده باشد یعنی پاسخ از پیش تعریف شده است
                                    // اخه پاسخ های دستگاه دو حالت داره یا از پیش تعریف شده است یا محاسباتی هست
                                    // از پیش تعریف شده یعنی اگر دستگاه این جواب رو داد مثلا 1-2-3-4-45 یعنی دستگاه با موفقیت خاموش شد یا هر کس شر دیگه ای
                                    // اما پاسخ از پیش تعیین نشده یعنی فرمول داره و مقداری رو بهمون میده دستگاه مثلا دماشو میده 400 یعنی حال اون کامند بایت ها اگر فرمول تعریف شده توی دیتابیس رو روش اجرا کنیم
                                    // جواب مشخص عددی در میاد ازش
                                    // حالا این زیر اول چک میکنیم این کامندی که اومده جزو پاسخ های از پیش تعریف شده دستگاه هست یا نه اگر بود که جواب مشخصشو میزاریم توی لاگ
                                    // وگرنه میریم سراغ محاسبه فرمول و ادامه کس شراش
                                    var result = deviceresults.Find(l => l.Data.Replace("-", "") == data.Replace("-", ""));
                                    if (result != null && string.IsNullOrWhiteSpace(LocalCache.Instructions.FirstOrDefault(k => k.ID == fire.InstructionID).Formula))
                                    {
                                        // جواب از مدل عادی است
                                        fire.ResultID = result.ID;
                                        fire.Value = result.Memo;
                                        _value = fire.Value;
                                        // اگر سرور وصل باشه جواب درخواست کاربر سایت مستقیم اینزرت میشه روی دیتابیس مرکز
                                        if (Extension.IsSqlServerAvailable(LocalCache.Setting.SQLServerIP))
                                        {
                                            sqlDb.UpdateInstructionFireOnServer(onlineshot.InstructionFire.ID, fire.Value, result.ID);
                                        }
                                    }
                                    else
                                    {
                                        // جواب از مدل ثابت نیست بلکه مقدار است مقداری عددی که با کمک فرمول باید بیرون کشیده شود مثلا
                                        // دمایی فشاری سطحی چیزی است که خروجی مقدارش بعد فرمول باید بشود مثلا 145
                                        // جواب از مدل خواندنی است و ولیو بایستی دیتکت شود
                                        fire.ResultID = 0;
                                        var dataarray = data.Split('-');
                                        dataarray = dataarray.Where(val => val != string.Empty).ToArray();
                                        if (dataarray != null && dataarray.Length > 0)
                                        {
                                            int len;
                                            try
                                            {
                                                var strtt = dataarray[0];
                                                len = int.Parse(strtt);
                                            }
                                            catch
                                            {
                                                len = 0;
                                            }
                                            if (dataarray.Length >= len)
                                            {
                                                string value = string.Empty;
                                                for (int j = 0; j < len; j++)
                                                {
                                                    value += dataarray[j + 1] += "-";
                                                }
                                                //formula
                                                //
                                                var formulon = LocalCache.Instructions.FirstOrDefault(k => k.ID == fire.InstructionID).Formula;

                                                if (!string.IsNullOrWhiteSpace(formulon))
                                                {
                                                    var bytess = value.Split('-');
                                                    try
                                                    {
                                                        // در حلقه زیر میایم عدد بایت ها رو توی فرمول ریپلیس میکنیم و بعد میریم سراغ محاسبه با دیتاتیبل
                                                        // فرمول ها رو توی جدول دستورالعمل ها توی سایت کاربر متخصص سخت افزار از قبل تعریف کرده مثلا
                                                        // a+b+c یعنی بایت اول و دوم و سوم از کامند دریافتی باهم جمع بشه
                                                        // (((a3+b4+z+h3)*10)+16)-1 یعنی بایت هفت تاد و هشتم با بایت صد و پنجم با بایت بیست و پنجم با بایت هشتاد و پنجم جمع میشه با ده ضرب میشه با 16 جمع میشه منهای یک میشه
                                                        // حالا وقتی مقادیر بایت ها با کاراکترهای انگلیسی ریپلیس بشه توی فرمول های بالا فرمول نهایی میشه مثه ذیل
                                                        // (((250+150+32+112)*10)+16)-1
                                                        // حالا این فرمولو میدیم به تابع کامپیوت دیتاتیبل عدد نهایی رو بهمون میده
                                                        var alphabets = formulon.GetFormulaAlphabetPartOnly();
                                                        if (alphabets != null)
                                                        {
                                                            foreach (var alphabet in alphabets)
                                                            {
                                                                formulon = formulon.Replace(alphabet,
                                                                    bytess[alphabet.GetCharIndexInFormula()]);
                                                            }
                                                            //formulon = alphabets.Where(alphabet => !string.IsNullOrWhiteSpace(alphabet)).Aggregate(formulon, (current, alphabet) => current.Replace(alphabet, bytess[alphabet.GetCharIndexInFormula()]));
                                                        }
                                                        firedt = new DataTable();
                                                        var objvalue = firedt.Compute(formulon, "");
                                                        decimal decval = objvalue.ToDecimal();//dashitemdt.Compute(formulon, "").ToDecimal();
                                                        //if IEEE then Compute In That Format
                                                        // اگر دستورالعمل از نوع آی تری ای بود نحوه محاسبه عدد نهایش فرق میکنه که در ذیل امده است و گرنه همون عدد حاصل از فرمول زن جنده قرار میگیره توش
                                                        if (LocalCache.Instructions
                                                            .FirstOrDefault(k => k.ID == fire.InstructionID).IEEE)
                                                        {
                                                            fire.Value = decval.ConvertIeeeToDouble().ToString("N2");
                                                        }
                                                        else
                                                        {
                                                            fire.Value = decval.ToString("N2");
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        fire.Value = string.Empty;
                                                        _errorLog.SaveLog(ex,"خطا در پردازش فرمول");
                                                    }

                                                }
                                                if (string.IsNullOrWhiteSpace(fire.Value))
                                                {
                                                    fire.Value = value;
                                                    _value = fire.Value;
                                                }
                                                if (Extension.IsSqlServerAvailable(LocalCache.Setting.SQLServerIP))
                                                {
                                                    sqlDb.UpdateInstructionFireOnServer(onlineshot.InstructionFire.ID, fire.Value, 0);
                                                }
                                            }
                                            else
                                            {
                                                // پاسخ نامشخص از دستگاه
                                                fire.Value = "پاسخ دستگاه نامعتبر است";
                                                if (Extension.IsSqlServerAvailable(LocalCache.Setting.SQLServerIP))
                                                {
                                                    sqlDb.UpdateInstructionFireOnServer(onlineshot.InstructionFire.ID, fire.Value, 0);
                                                }
                                            }
                                        }

                                    }
                                }
                                else
                                {
                                    fire.ResultID = 0;
                                    fire.Value = "CRCERROR";
                                    if (Extension.IsSqlServerAvailable(LocalCache.Setting.SQLServerIP))
                                    {
                                        sqlDb.UpdateInstructionFireOnServer(onlineshot.InstructionFire.ID, fire.Value, 0);
                                    }
                                }
                                if (ShotQueue.Count > 0) { ShotQueue.Dequeue(); }
                                SLOCK = false;
                            }
                        }
                        //اگر درخواست زماندار بود و اتومات با توجه به اولویت زمانیش اضافه شده بود و به صف و درخواستش به ارسال شده بود اینجا جوابش گاییده میشه
                        else if (onlineshot.DashboardItem != null)
                        {
                            var dashitem = LocalCache.DashboardItems.FirstOrDefault(l => l.ID == onlineshot.DashboardItem.ID);
                            var device = LocalCache.Devices.FirstOrDefault(l => l.ID == dashitem.DeviceID);
                            var deviceresults = LocalCache.Results.Where(l => l.DeviceType == device.DeviceType).ToList();
                            var dashiteminstruction = LocalCache.Instructions.FirstOrDefault(l => l.ID == dashitem.InstructionID);
                            _deviceid = dashitem.DeviceID;
                            _instructionid = dashitem.InstructionID;
                            var items = InString.Split('-');
                            if (items != null && items.Count() > 0)
                            {
                                var data = "";
                                var slaveid = items[0];
                                var funcode = items[1];
                                var crc1 = items[items.Count() - 2];
                                var crc2 = items[items.Count() - 1];
                                var bytelist = new List<byte>();
                                for (int i = 0; i < items.Count() - 2; i++)
                                {
                                    bytelist.Add(byte.Parse(items[i]));
                                    if (i > 1)
                                    {
                                        data += items[i] + "-";
                                    }
                                }
                                var crc = Extension.ComputeCRC(bytelist);
                                if (crc.CRC1 == int.Parse(crc1) && crc.CRC2 == int.Parse(crc2))
                                {
                                    var result = deviceresults.Find(l => l.Data.Replace("-", "") == data.Replace("-", ""));
                                    if (result != null && string.IsNullOrWhiteSpace(LocalCache.Instructions.FirstOrDefault(k => k.ID == LocalCache.DashboardItems.FirstOrDefault(l => l.ID == dashitem.ID).InstructionID).Formula))
                                    {
                                        // جواب از مدل عادی است
                                        dashitem.ResultID = result.ID;
                                        dashitem.Value = result.Memo;
                                        _value = dashitem.Value;
                                        dashitem.SaveTime = DateTime.UtcNow;
                                        LocalCache.DashboardLogs.Add(new DashboardLogEntity()
                                        {
                                            DashboardItemID = dashitem.ID,
                                            Value = result.Memo,
                                            SaveTime = DateTime.UtcNow,
                                            IsSended = false,
                                            ResultID = result.ID,
                                            CreateDate = DateTime.UtcNow,
                                            Description = "درخواست زمان دار"
                                        });
                                    }
                                    else
                                    {
                                        // جواب از مدل خواندنی است و ولیو بایستی دیتکت شود
                                        dashitem.ResultID = 0;
                                        var dataarray = data.Split('-');
                                        dataarray = dataarray.Where(val => val != string.Empty).ToArray();
                                        if (dataarray != null && dataarray.Length > 0)
                                        {
                                            int len;
                                            try
                                            {
                                                var strtt = dataarray[0];
                                                len = int.Parse(strtt);
                                            }
                                            catch
                                            {
                                                len = 0;
                                            }

                                            if (dataarray.Length >= len)
                                            {
                                                string value = string.Empty;
                                                for (int j = 0; j < len; j++)
                                                {
                                                    value += dataarray[j + 1] += "-";
                                                }

                                                //
                                                //formula
                                                var formulon = LocalCache.Instructions.FirstOrDefault(k =>
                                                    k.ID == LocalCache.DashboardItems
                                                        .FirstOrDefault(l => l.ID == dashitem.ID)
                                                        .InstructionID).Formula;

                                                if (!string.IsNullOrWhiteSpace(formulon))
                                                {
                                                    var bytess = value.Split('-');
                                                    try
                                                    {
                                                        var alphabets = formulon.GetFormulaAlphabetPartOnly();
                                                        if (alphabets != null)
                                                        {
                                                            foreach (var alphabet in alphabets)
                                                            {
                                                                formulon = formulon.Replace(alphabet,
                                                                    bytess[alphabet.GetCharIndexInFormula()]);
                                                            }

                                                            //formulon = alphabets.Where(alphabet => !string.IsNullOrWhiteSpace(alphabet)).Aggregate(formulon, (current, alphabet) => current.Replace(alphabet, bytess[alphabet.GetCharIndexInFormula()]));
                                                        }

                                                        dashitemdt = new DataTable();
                                                        var objvalue = dashitemdt.Compute(formulon, "");
                                                        decimal decval =
                                                            objvalue
                                                                .ToDecimal(); //dashitemdt.Compute(formulon, "").ToDecimal();
                                                        //if IEEE then Compute In That Format
                                                        if (LocalCache.Instructions
                                                            .FirstOrDefault(k =>
                                                                k.ID == LocalCache.DashboardItems
                                                                    .FirstOrDefault(l => l.ID == dashitem.ID)
                                                                    .InstructionID).IEEE)
                                                        {
                                                            dashitem.Value = decval.ConvertIeeeToDouble()
                                                                .ToString("N2");
                                                        }
                                                        else
                                                        {
                                                            dashitem.Value = decval.ToString("N2");
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        dashitem.Value = string.Empty;
                                                        _errorLog.SaveLog(ex, "خطا در پردازش فرمول");
                                                    }

                                                }

                                                if (string.IsNullOrWhiteSpace(dashitem.Value))
                                                {
                                                    dashitem.Value = value;
                                                    _value = dashitem.Value;
                                                }

                                                dashitem.SaveTime = DateTime.UtcNow;
                                                LocalCache.DashboardLogs.Add(new DashboardLogEntity()
                                                {
                                                    DashboardItemID = dashitem.ID,
                                                    Value = dashitem.Value,
                                                    SaveTime = DateTime.UtcNow,
                                                    IsSended = false,
                                                    ResultID = 0,
                                                    CreateDate = DateTime.UtcNow,
                                                    Description = "درخواست زمان دار"
                                                });
                                                //scenario
                                                // پردازش سناریو
                                                // سناریوها کاربردشون اینه که روی پاسخ درخواست های زماندار تنظیم میشن مثلا روی سطح منبع ها
                                                // وقتی مثلا سه ثانیه یه بار مقدار منبع دریافت شد بر اساس مقدارش
                                                // براساس مقدارش که مینیمم یا ماکزیمم چیزی که توی سناریو تعریف شده و همچنین زمان میاد روی دستگاه متصل به اون دستگاه منبع که توی لیست دستگاه های سایت تنظیم شده
                                                // دستور العمل مشخص شده توسط کاربر توی تعریف سناریو رو اضافه میکنه به صف تا اجرا بشهه و خاهر دستگاه متصلو بگاد
                                                var now = DateTime.UtcNow.TimeOfDay;
                                                decimal dashval;
                                                try
                                                {
                                                    dashval = decimal.Parse(dashitem.Value);
                                                }
                                                catch (Exception)
                                                {
                                                    dashval = 0;
                                                }


                                                var shouldRunScenario = !((dashiteminstruction.MinValue >= 0 &&
                                                                           dashval < dashiteminstruction
                                                                               .MinValue) ||
                                                                          (dashiteminstruction.MaxValue >= 0 &&
                                                                           dashval > dashiteminstruction.MaxValue));

                                                if (shouldRunScenario)
                                                {
                                                    var scenariolst = LocalCache.DashboardScenarios.Where(l =>
                                                        l.DashboardItemID == dashitem.ID && l.FromTime <= now &&
                                                        l.ToTime >= now).ToList();
                                                    if (scenariolst != null && scenariolst.Count > 0 && dashval > 0)
                                                    {
                                                        foreach (var item in scenariolst)
                                                        {
                                                            decimal maxval, minval;
                                                            try
                                                            {
                                                                maxval = decimal.Parse(item.MaxValue);
                                                            }
                                                            catch (Exception)
                                                            {
                                                                maxval = 0;
                                                            }

                                                            try
                                                            {
                                                                minval = decimal.Parse(item.MinValue);
                                                            }
                                                            catch (Exception)
                                                            {
                                                                minval = 0;
                                                            }
                                                            // اگر شرایط سناریو سازگار بود درخواست مورد نظره سناریو برای گاییدن دستگاه متصل اضافه میشه به صف درخواست ها

                                                            if (maxval > 0 && dashval >= maxval)
                                                            {
                                                                ShotQueue.Enqueue(new Shot(new LocalFireEntity()
                                                                {
                                                                    DeviceID = item.ConnectedDeviceID,
                                                                    InstructionID = item.InstructionID,
                                                                    Type = "ScenarioFire"
                                                                }));
                                                            }
                                                            else if (minval > 0 && dashval <= minval)
                                                            {
                                                                ShotQueue.Enqueue(new Shot(new LocalFireEntity()
                                                                {
                                                                    DeviceID = item.ConnectedDeviceID,
                                                                    InstructionID = item.InstructionID,
                                                                    Type = "ScenarioFire"
                                                                }));
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // پاسخ نامشخص از دستگاه
                                                dashitem.Value = "پاسخ دستگاه نامعتبر است";
                                                dashitem.SaveTime = DateTime.UtcNow;
                                                LocalCache.DashboardLogs.Add(new DashboardLogEntity()
                                                {
                                                    DashboardItemID = dashitem.ID,
                                                    Value = dashitem.Value,
                                                    SaveTime = DateTime.UtcNow,
                                                    IsSended = false,
                                                    ResultID = 0,
                                                    CreateDate = DateTime.UtcNow,
                                                    Description = "درخواست زمان دار"
                                                });
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    dashitem.ResultID = 0;
                                    dashitem.Value = "CRCERROR";
                                    LocalCache.DashboardLogs.Add(new DashboardLogEntity()
                                    {
                                        DashboardItemID = dashitem.ID,
                                        Value = dashitem.Value,
                                        SaveTime = DateTime.UtcNow,
                                        IsSended = false,
                                        ResultID = 0,
                                        CreateDate = DateTime.UtcNow,
                                        Description = "درخواست زمان دار"
                                    });
                                }
                                if (ShotQueue.Count > 0) { ShotQueue.Dequeue(); }
                                SLOCK = false;
                            }
                        }
                        //اگر درخواست از کاربر لوکال دستگاه بود یعنی روی فرم همین برنامه درخواست رو زده بود
                        else if (onlineshot.LocalFire != null)
                        {
                            var localfire = onlineshot.LocalFire;
                            var device = LocalCache.Devices.FirstOrDefault(l => l.ID == localfire.DeviceID);
                            var deviceresults = LocalCache.Results.Where(l => l.DeviceType == device.DeviceType).ToList();
                            _deviceid = localfire.DeviceID;
                            _instructionid = localfire.InstructionID;
                            var items = InString.Split('-');
                            if (items != null && items.Count() > 0)
                            {
                                var data = "";
                                var slaveid = items[0];
                                var funcode = items[1];
                                var crc1 = items[items.Count() - 2];
                                var crc2 = items[items.Count() - 1];
                                var bytelist = new List<byte>();
                                for (int i = 0; i < items.Count() - 2; i++)
                                {
                                    bytelist.Add(byte.Parse(items[i]));
                                    if (i > 1)
                                    {
                                        data += items[i] + "-";
                                    }
                                }
                                var crc = Extension.ComputeCRC(bytelist);
                                if (crc.CRC1 == int.Parse(crc1) && crc.CRC2 == int.Parse(crc2))
                                {
                                    var result = deviceresults.Find(l => l.Data.Replace("-", "") == data.Replace("-", ""));
                                    if (result != null && string.IsNullOrWhiteSpace(LocalCache.Instructions.FirstOrDefault(k => k.ID == localfire.InstructionID).Formula))
                                    {
                                        // جواب از مدل عادی است
                                        localfire.ResultID = result.ID;
                                        localfire.Value = result.Memo;
                                        _value = localfire.Value;
                                        LocalCache.DashboardLogs.Add(new DashboardLogEntity()
                                        {
                                            InstructionID = localfire.InstructionID,
                                            Value = result.Memo,
                                            SaveTime = DateTime.UtcNow,
                                            IsSended = false,
                                            ResultID = result.ID,
                                            CreateDate = DateTime.UtcNow,
                                            Description = (onlineshot.LocalFire.Type == "LocalFire") ? "درخواست کاربر کنترلر تتیس" : ("عملیات حاصل سناریو" + "#" + localfire.DeviceID.ToString())
                                        });
                                        if (onlineshot.LocalFire.Type == "LocalFire")
                                        {
                                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { UpdateLocalFireResultUI(localfire.Value); });
                                        }
                                    }
                                    else
                                    {
                                        // جواب از مدل خواندنی است و ولیو بایستی دیتکت شود
                                        localfire.ResultID = 0;
                                        //
                                        var dataarray = data.Split('-');
                                        dataarray = dataarray.Where(val => val != string.Empty).ToArray();
                                        if (dataarray != null && dataarray.Length > 0)
                                        {
                                            int len;
                                            try
                                            {
                                                var strtt = dataarray[0];
                                                len = int.Parse(strtt);
                                            }
                                            catch
                                            {
                                                len = 0;
                                            }
                                            if (dataarray.Length >= len)
                                            {
                                                string value = string.Empty;
                                                for (int j = 0; j < len; j++)
                                                {
                                                    value += dataarray[j + 1] += "-";
                                                }
                                                //                                                
                                                //formula
                                                var formulon = LocalCache.Instructions.FirstOrDefault(k => k.ID == localfire.InstructionID).Formula;

                                                if (!string.IsNullOrWhiteSpace(formulon))
                                                {
                                                    var bytess = value.Split('-');
                                                    try
                                                    {
                                                        var alphabets = formulon.GetFormulaAlphabetPartOnly();
                                                        if (alphabets != null)
                                                        {
                                                            foreach (var alphabet in alphabets)
                                                            {
                                                                formulon = formulon.Replace(alphabet,
                                                                    bytess[alphabet.GetCharIndexInFormula()]);
                                                            }
                                                            //formulon = alphabets.Where(alphabet => !string.IsNullOrWhiteSpace(alphabet)).Aggregate(formulon, (current, alphabet) => current.Replace(alphabet, bytess[alphabet.GetCharIndexInFormula()]));
                                                        }
                                                        localfiredt = new DataTable();
                                                        var objvalue = localfiredt.Compute(formulon, "");
                                                        decimal decval = objvalue.ToDecimal();//dashitemdt.Compute(formulon, "").ToDecimal();
                                                        //if IEEE then Compute In That Format
                                                        if (LocalCache.Instructions.FirstOrDefault(k => k.ID == localfire.InstructionID).IEEE)
                                                        {
                                                            localfire.Value = decval.ConvertIeeeToDouble().ToString("N2");
                                                        }
                                                        else
                                                        {
                                                            localfire.Value = decval.ToString("N2");
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        localfire.Value = string.Empty;
                                                        _errorLog.SaveLog(ex, "خطا در پردازش فرمول");
                                                    }

                                                }
                                                if (string.IsNullOrWhiteSpace(localfire.Value))
                                                {
                                                    localfire.Value = value;
                                                    _value = localfire.Value;
                                                }
                                                LocalCache.DashboardLogs.Add(new DashboardLogEntity()
                                                {
                                                    InstructionID = localfire.InstructionID,
                                                    Value = localfire.Value,
                                                    SaveTime = DateTime.UtcNow,
                                                    IsSended = false,
                                                    ResultID = 0,
                                                    CreateDate = DateTime.UtcNow,
                                                    Description = (onlineshot.LocalFire.Type == "LocalFire") ? "درخواست کاربر کنترلر تتیس" : ("عملیات حاصل سناریو" + "#" + localfire.DeviceID.ToString())
                                                });
                                                if (onlineshot.LocalFire.Type == "LocalFire")
                                                {
                                                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { UpdateLocalFireResultUI(localfire.Value); });
                                                }
                                            }
                                            else
                                            {
                                                // پاسخ نامشخص از دستگاه
                                                localfire.Value = "پاسخ دستگاه نامعتبر است";
                                                LocalCache.DashboardLogs.Add(new DashboardLogEntity()
                                                {
                                                    InstructionID = localfire.InstructionID,
                                                    Value = localfire.Value,
                                                    SaveTime = DateTime.UtcNow,
                                                    IsSended = false,
                                                    ResultID = 0,
                                                    CreateDate = DateTime.UtcNow,
                                                    Description = (onlineshot.LocalFire.Type == "LocalFire") ? "درخواست کاربر کنترلر تتیس" : ("عملیات حاصل سناریو" + "#" + localfire.DeviceID.ToString())
                                                });
                                                if (onlineshot.LocalFire.Type == "LocalFire")
                                                {
                                                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { UpdateLocalFireResultUI(localfire.Value); });
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    localfire.ResultID = 0;
                                    localfire.Value = "CRCERROR";
                                    LocalCache.DashboardLogs.Add(new DashboardLogEntity()
                                    {
                                        InstructionID = localfire.InstructionID,
                                        Value = localfire.Value,
                                        SaveTime = DateTime.UtcNow,
                                        IsSended = false,
                                        ResultID = 0,
                                        CreateDate = DateTime.UtcNow,
                                        Description = (onlineshot.LocalFire.Type == "LocalFire") ? "درخواست کاربر کنترلر تتیس" : ("عملیات حاصل سناریو" + "#" + localfire.DeviceID.ToString())
                                    });
                                    if (onlineshot.LocalFire.Type == "LocalFire")
                                    {
                                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { UpdateLocalFireResultUI(localfire.Value); });
                                    }
                                }
                                if (ShotQueue.Count > 0) { ShotQueue.Dequeue(); }
                                SLOCK = false;
                            }
                        }




                        //todo check if instruction has effect on balance then update balance and make an instruction base on balance if less than zero or above
                        //همچنین ی جایی برای دانلود اتومات لاگ های مثبت شارژ کنتور باید بیندیشیم
                        //
                        if (_deviceid > 0 && _instructionid > 0 && LocalCache.Instructions.FirstOrDefault(a => a.ID == _instructionid).IsEffectOnBalance && decimal.TryParse(_value, out decimal __value))
                        {
                            var ins = LocalCache.Instructions.FirstOrDefault(a => a.ID == _instructionid);
                            var dev = LocalCache.Devices.FirstOrDefault(a => a.ID == _deviceid);
                            var oldbalance = dev.Balance.ToDecimal();
                            dev.Balance += (__value * -1);//مقدار دریافتی از کنتور از بالانس کم میشود
                            localDb.UpdateDevice(dev);
                            if(oldbalance > 0 && dev.Balance <= 0 && ins.ConnectedDeviceInstructionIDNegative.ToInt() > 0)
                            {
                                //turn it off
                                ShotQueue.Enqueue(new Shot(new LocalFireEntity()
                                {
                                    DeviceID = dev.ID,
                                    InstructionID = ins.ConnectedDeviceInstructionIDNegative.ToInt(),
                                    Type = "LocalFire"
                                }));
                            }
                            if (oldbalance <= 0 && dev.Balance > 0 && ins.ConnectedDeviceInstructionIDPositive.ToInt() > 0)
                            {
                                //turn it on
                                ShotQueue.Enqueue(new Shot(new LocalFireEntity()
                                {
                                    DeviceID = dev.ID,
                                    InstructionID = ins.ConnectedDeviceInstructionIDPositive.ToInt(),
                                    Type = "LocalFire"
                                }));
                            }
                        }
                        //
                    }
                }
            }
            catch (Exception ex)
            {
                _errorLog.SaveLog(ex, ShotQueue.Count > 0 ? "Read*"+ InString + "*" + JsonConvert.SerializeObject(ShotQueue.Peek()) : "");
                throw ex;
            }
        }
        #endregion

        #region [DetachCOMPort]
        private void CancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }
        private void CloseDevice()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;
            listOfDevices.Clear();
        }
        #endregion

        #region [ResetWindows]
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
                ShowError(ex); _errorLog.SaveLog(ex);
                throw ex;
            }
        }
        #endregion

        #region [SetSystemDateTime]
        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetSystemTime(ref SYSTEMTIME st);
        private void SetSystemDate()
        {
            try
            {
                while (true)
                {
                    if (Extension.IsSqlServerAvailable(LocalCache.Setting.SQLServerIP))
                    {
                        var date = sqlDb.GetServerTime();
                        SYSTEMTIME st = new SYSTEMTIME
                        {
                            wYear = short.Parse(date.Year.ToString()),
                            wMonth = short.Parse(date.Month.ToString()),
                            wDay = short.Parse(date.Day.ToString()),
                            wHour = short.Parse(date.Hour.ToString()),
                            wMinute = (short.Parse(date.Minute.ToString())),
                            wSecond = short.Parse(date.Second.ToString())
                        };
                        SetSystemTime(ref st);
                        Thread.Sleep(10800000);//every 3 hour
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(ex); _errorLog.SaveLog(ex);
                throw ex;
            }
        }
        #endregion

        #region [UpdateClock]
        private async void UpdateClock()
        {
            try
            {
                while (true)
                {
                    if (serialPort != null)
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () => { lblDatetime.Text = Methods.GregorianToShamshiDateWithTime(DateTime.UtcNow); });//DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"); });
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
                _errorLog.SaveLog(ex, "UpdateClock");
                throw ex;
            }
        }
        #endregion

        #region [PutInstructionFireInQueue]
        private void CheckInstructionFire()
        {
            try
            {
                while (true)
                {
                    if (serialPort != null && Extension.IsSqlServerAvailable(LocalCache.Setting.SQLServerIP))//  یعنی تنها وقتی پردازش جلو میره که فرم داشبورد روی صفحه باشه و سرور هم وصل باشه
                    {
                        var deviceids = LocalCache.GetAllDeviceIDs();
                        if (deviceids != null && deviceids.Count() > 0)
                        {
                            var fires = sqlDb.DownloadAllInstructionFires(deviceids);
                            //ShotQueue.Clear();
                            foreach (var fire in fires)
                            {
                                if (!ShotQueue.Any(l => l.InstructionFire != null && l.InstructionFire.ID == fire.ID))
                                {
                                    ShotQueue.Enqueue(new Shot(fire));
                                }
                            }
                        }
                    }
                    Thread.Sleep(3000);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
                _errorLog.SaveLog(ex, "CheckInstructionFire");
                throw ex;
            }
        }
        #endregion

        #region [PutDashboardItemsInQueue]
        private void CheckDashboardItems(decimal timeout)
        {
            try
            {
                while (true)
                {
                    if (serialPort != null)
                    {
                        var priority = LocalCache.DashboardPriorities.FirstOrDefault(l => l.Value == timeout);
                        if (priority != null)
                        {
                            var dash = LocalCache.DashboardItems.Where(l => LocalCache.Devices.Where(k => k.LocationID == LocalCache.Setting.SiteID).Select(j => j.ID).ToList().Contains(l.DeviceID) && l.Priority == priority.ID);
                            foreach (var item in dash)
                            {
                                if (!ShotQueue.Any(l => l.DashboardItem != null && l.DashboardItem.ID == item.ID))
                                {
                                    ShotQueue.Enqueue(new Shot(item));
                                }
                                //if (ShotQueue.Count == 0 || ShotQueue.Peek().DashboardItem.ID != item.ID)
                                //{
                                //ShotQueue.Enqueue(new Shot(item));
                                //}
                            }
                        }
                    }
                    Thread.Sleep((int)(timeout * (decimal)1000));
                }
            }
            catch (Exception ex)
            {
                ShowError(ex); _errorLog.SaveLog(ex);
                throw ex;
            }
        }
        #endregion

        #region [SenderThread]
        private void SendProcessing()
        {
            try
            {
                while (true)
                {
                    while (SLOCK == true)
                    {
                        continue;
                    }
                    if (serialPort != null)
                    {
                        //ShotQueue.TrimExcess();
                        if (ShotQueue != null && ShotQueue.Count > 0)
                        {
                            if(ShotQueue.Any(l=>l.InstructionFire !=null || l.LocalFire != null))
                            {
                                while(ShotQueue.Peek() != null && ShotQueue.Peek().DashboardItem != null)
                                {
                                    ShotQueue.Dequeue();
                                }
                            }
                            var item = ShotQueue.Peek();
                            InstructionEntity instruction;
                            DeviceEntity device;
                            if (item.InstructionFire != null)
                            {
                                instruction = LocalCache.Instructions.FirstOrDefault(l => l.ID == item.InstructionFire.InstructionID);
                                device = LocalCache.Devices.FirstOrDefault(l => l.ID == item.InstructionFire.DeviceID);
                            }
                            else if (item.DashboardItem != null)
                            {
                                instruction = LocalCache.Instructions.FirstOrDefault(l => l.ID == item.DashboardItem.InstructionID);
                                device = LocalCache.Devices.FirstOrDefault(l => l.ID == item.DashboardItem.DeviceID);
                            }
                            else
                            {// local fire
                                instruction = LocalCache.Instructions.FirstOrDefault(l => l.ID == item.LocalFire.InstructionID);
                                device = LocalCache.Devices.FirstOrDefault(l => l.ID == item.LocalFire.DeviceID);
                            }

                            if (instruction != null && instruction.GPIO > 0)
                            {
                                SlockTimer = new System.Timers.Timer(5000);
                                SlockTimer.Elapsed += (sender, e) => HandleTimer();
                                SlockTimer.Start();
                                switch (instruction.GPIO)
                                {
                                    case 12:
                                        GPIOPIN_12.Write(instruction.GPIOValue == 1
                                            ? GpioPinValue.High
                                            : GpioPinValue.Low);
                                        localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                                        {
                                            GPIO = 12,
                                            GPIOValue = instruction.GPIOValue.Value,
                                            SaveTime = DateTime.Now
                                        });
                                        break;
                                    case 13:
                                        GPIOPIN_13.Write(instruction.GPIOValue == 1
                                            ? GpioPinValue.High
                                            : GpioPinValue.Low);
                                        localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                                        {
                                            GPIO = 13,
                                            GPIOValue = instruction.GPIOValue.Value,
                                            SaveTime = DateTime.Now
                                        });
                                        break;
                                    case 16:
                                        GPIOPIN_16.Write(instruction.GPIOValue == 1
                                            ? GpioPinValue.High
                                            : GpioPinValue.Low);
                                        localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                                        {
                                            GPIO = 16,
                                            GPIOValue = instruction.GPIOValue.Value,
                                            SaveTime = DateTime.Now
                                        });
                                        break;
                                    case 19:
                                        GPIOPIN_19.Write(instruction.GPIOValue == 1
                                            ? GpioPinValue.High
                                            : GpioPinValue.Low);
                                        localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                                        {
                                            GPIO = 19,
                                            GPIOValue = instruction.GPIOValue.Value,
                                            SaveTime = DateTime.Now
                                        });
                                        break;
                                    case 20:
                                        GPIOPIN_20.Write(instruction.GPIOValue == 1
                                            ? GpioPinValue.High
                                            : GpioPinValue.Low);
                                        localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                                        {
                                            GPIO = 20,
                                            GPIOValue = instruction.GPIOValue.Value,
                                            SaveTime = DateTime.Now
                                        });
                                        break;
                                    case 21:
                                        GPIOPIN_21.Write(instruction.GPIOValue == 1
                                            ? GpioPinValue.High
                                            : GpioPinValue.Low);
                                        localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                                        {
                                            GPIO = 21,
                                            GPIOValue = instruction.GPIOValue.Value,
                                            SaveTime = DateTime.Now
                                        });
                                        break;
                                    case 25:
                                        GPIOPIN_25.Write(instruction.GPIOValue == 1
                                            ? GpioPinValue.High
                                            : GpioPinValue.Low);
                                        localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                                        {
                                            GPIO = 25,
                                            GPIOValue = instruction.GPIOValue.Value,
                                            SaveTime = DateTime.Now
                                        });
                                        break;
                                    case 26:
                                        GPIOPIN_26.Write(instruction.GPIOValue == 1
                                            ? GpioPinValue.High
                                            : GpioPinValue.Low);
                                        localDb.InsertOrUpdateGPIOHistory(new GPIOHistoryEntity()
                                        {
                                            GPIO = 26,
                                            GPIOValue = instruction.GPIOValue.Value,
                                            SaveTime = DateTime.Now
                                        });
                                        break;
                                }
                            }
                            else
                            {
                                var strtofire = device.RowNow + "-" + instruction.FunctionCode + "-" + instruction.Data;
                                var crc = Extension.ComputeCRC(strtofire.ToByteList());
                                strtofire += "-" + crc.CRC1.ToString() + "-" + crc.CRC2.ToString();
                                SLOCK = true;
                                OutString = strtofire;
                                SlockTimer = new System.Timers.Timer(5000);
                                SlockTimer.Elapsed += (sender, e) => HandleTimer();
                                SlockTimer.Start();
                                sendTextButton_Click(null, null);
                            }
                        }
                    }
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
                _errorLog.SaveLog(ex, ShotQueue.Count > 0 ? "Send*" + JsonConvert.SerializeObject(ShotQueue.Peek()) : "");
                throw ex;
            }
        }
        #endregion

        #region [HandleSendTimeout]
        private async void HandleTimer()
        {
            try
            {
                SlockTimer.Stop();
                if (ShotQueue != null && ShotQueue.Count > 0)
                {
                    var onlineshot = ShotQueue.Peek();                    

                    if (onlineshot.InstructionFire != null)
                    {
                        var fire = onlineshot.InstructionFire;
                        fire.Value = "Timeout";                        
                        if (Extension.IsSqlServerAvailable(LocalCache.Setting.SQLServerIP))
                        {
                            sqlDb.UpdateInstructionFireOnServer(onlineshot.InstructionFire.ID, fire.Value, 0);
                        }
                    }
                    else if (onlineshot.DashboardItem != null)
                    {
                        var dash = LocalCache.DashboardItems.FirstOrDefault(l => l.ID == onlineshot.DashboardItem.ID);                        
                        dash.Value = "Timeout";
                        dash.SaveTime = DateTime.UtcNow;
                        LocalCache.DashboardItems.Where(usr => usr.ID == dash.ID).Select(usr => { usr.ResultID = 0; usr.Value = dash.Value; usr.SaveTime = dash.SaveTime; return usr; }).ToList();
                        LocalCache.DashboardLogs.Add(new DashboardLogEntity()
                        {
                            DashboardItemID = dash.ID,
                            Value = "Timeout",
                            SaveTime = DateTime.UtcNow,
                            IsSended = false,
                            ResultID = 0,
                            CreateDate = DateTime.UtcNow,
                            Description = "درخواست زمان دار"
                        });
                    }
                    else if (onlineshot.LocalFire != null)
                    {                        
                        LocalCache.DashboardLogs.Add(new DashboardLogEntity()
                        {
                            InstructionID = onlineshot.LocalFire.InstructionID,
                            Value = "Timeout",
                            SaveTime = DateTime.UtcNow,
                            IsSended = false,
                            ResultID = 0,
                            CreateDate = DateTime.UtcNow,
                            Description = (onlineshot.LocalFire.Type == "LocalFire") ? "درخواست کاربر کنترلر تتیس" : ("عملیات حاصل سناریو" + "#" + onlineshot.LocalFire.DeviceID.ToString())
                        });
                        if (onlineshot.LocalFire.Type == "LocalFire")
                        {
                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { UpdateLocalFireResultUI("Timeout"); });
                        }
                    }
                   
                    if (ShotQueue.Count > 0) { ShotQueue.Dequeue(); }
                    SLOCK = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region [UploadDashboardItemsLogToSqlServer]
        private static bool SendLock;
        private async void UploadDashboardLogsToServer()
        {
            try
            {
                while (true)
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { lblIpAddresss.Text = (string.IsNullOrWhiteSpace(lblIpAddresss.Text) ? Extension.LocalIPAddress : lblIpAddresss.Text); });
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { ImgConnection.Source = (Extension.IsSqlServerAvailable(LocalCache.Setting.SQLServerIP) ? new BitmapImage(new Uri("ms-appx:/Assets/connect.png", UriKind.Absolute)) : new BitmapImage(new Uri("ms-appx:/Assets/disconnect.png", UriKind.Absolute))); });
                    if (serialPort != null)
                    {
                        var localcachelog = new List<DashboardLogEntity>(LocalCache.DashboardLogs);
                        LocalCache.DashboardLogs.Clear();
                        foreach (var log in localcachelog)
                        {
                            localDb.InsertDashboardLogAsync(log);
                        }
                        if (Extension.IsSqlServerAvailable(LocalCache.Setting.SQLServerIP) && SendLock == false)
                        {
                            try
                            {
                                SendLock = true;
                                var lst = await localDb.GetAllDashboardLog();//select top 1000
                                if (lst != null && lst.Count > 0)
                                {
                                    sqlDb.UploadDashboardLogsToServer(lst);
                                    foreach(var lll in lst)
                                    {
                                        localDb.DeleteDashboardLog(lll.ID);
                                    }
                                    //localDb.DeleteAllDashboardLog();
                                }
                                if (sqlDb.CheckForHardUpdate(LocalCache.Setting.SiteID))
                                {
                                    DTS.Init(LocalCache.Setting.SQLServerIP);
                                    if (DTS.FullDownload()) { sqlDb.UpdateRasHardUpdateStatus(LocalCache.Setting.SiteID); CoreApplication.Exit(); };
                                }
                            }
                            catch (Exception ex)
                            {
                                _errorLog.SaveLog(ex, "UploadDashboardLogsToServer");
                            }
                            finally
                            {
                                SendLock = false;
                            }

                        }
                    }
                    Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex); _errorLog.SaveLog(ex);
                throw ex;
            }
        }
        #endregion

        #region [DashboardUI]
        private void MainCommandBar_Closed(object sender, object e)
        {
            MainCommandBar.IsOpen = true;
        }
        private void BindPivotItems()
        {
            try
            {
                PivotItem pvt;
                foreach (var itm in LocalCache.Devices.Where(l => l.LocationID == LocalCache.Setting.SiteID))
                {
                    var dashcontent = new DashContent(itm.ID, LocalCache);
                    dashcontent._LocalFireClicked += Dashcontent__LocalFireClicked;
                    pvt = new PivotItem
                    {
                        Header = itm.Name,
                        Tag = itm.ID,
                        Content = dashcontent,
                        Style = (Style)this.Resources["Styles"]
                    };
                    pivotMain.Items.Add(pvt);
                    pvt = null;
                }
            }
            catch (Exception ex)
            {
                _errorLog.SaveLog(ex, "BindPivotItems");
                throw ex;
            }
        }
        private async void Dashcontent__LocalFireClicked(object sender, EventArgs e)
        {
            try
            {
                var dialog = new MessageDialog("دستورالعمل به دستگاه ارسال می شود ، آیا اطمینان دارید؟؟")
                {
                    Title = "توجه"
                };
                dialog.Commands.Add(new UICommand { Label = "بله", Id = 0 });
                dialog.Commands.Add(new UICommand { Label = "خیر", Id = 1 });
                var res = await dialog.ShowAsync();
                if ((int)res.Id == 0)
                {
                    if ((LocalFireEntity)sender != null)
                    {
                        //ShotQueue.Clear();
                        ShotQueue.Enqueue(new Shot((LocalFireEntity)sender));
                    }
                }

            }
            catch (Exception ex)
            {
                _errorLog.SaveLog(ex);
                throw ex;
            }
        }
        private void UpdateLocalFireResultUI(string value)
        {
            try
            {
                if (pivotMain.SelectedItem != null)
                {
                    var pivot = (PivotItem)pivotMain.SelectedItem;
                    var content = pivot.Content as DashContent;
                    if (content != null)
                    {
                        content.SetLocalFireResult(value);
                    }
                }
            }
            catch (Exception ex)
            {
                _errorLog.SaveLog(ex);
                throw ex;
            }
        }
        private async void UpdateDashboardUIGrid()
        {
            try
            {
                while (true)
                {
                    if (serialPort != null)
                    {
                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                            var pivot = (PivotItem)pivotMain.SelectedItem;
                            if (pivot != null && pivot.Content is DashContent content)
                            {
                                content.Refresh(LocalCache);
                            }
                        });
                        
                    }
                    Thread.Sleep(7000);
                }
            }
            catch (Exception ex)
            {
                _errorLog.SaveLog(ex);
                throw ex;
            }
        }
        #endregion


        private void DownloadDeviceChargeLog()
        {   
            try
            {
                while (true)
                {
                    if (Extension.IsSqlServerAvailable(LocalCache.Setting.SQLServerIP))
                    {
                        var lst = sqlDb.DownloadDeviceChargeValue();
                        if(lst != null && lst.Count > 0)
                        {
                            string deviceids = string.Join(",", lst.Select(item => item.DeviceID).Distinct().ToArray());
                            foreach (var devid in deviceids.Split(','))
                            {
                                var charge = lst.Where(a => a.DeviceID == devid.ToInt()).Sum(a => a.LogValue);
                                var dev = LocalCache.Devices.FirstOrDefault(a => a.ID == devid.ToInt());
                                dev.Balance += charge;
                                localDb.UpdateDevice(dev);
                            }
                            foreach (var item in lst)
                            {
                                sqlDb.UpdateDeviceChargeValueLog(item.ID);
                            }
                        }
                        
                        Thread.Sleep(300000);//every 5 minutes
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(ex); _errorLog.SaveLog(ex);
                throw ex;
            }
        }

        private async void ShowError(Exception ex)
        {
            var Error = ex.Message + "#" + (ex.InnerException?.Message ?? "") + "#" + (ex.InnerException?.InnerException?.Message ?? "");
            var stacktrace = ex.StackTrace + "#" + (ex.InnerException?.StackTrace ?? "") + "#" + (ex.InnerException?.InnerException?.StackTrace ?? "");
            var dialog = new MessageDialog(Error + Environment.NewLine + stacktrace);
            dialog.Title = "خطای ثبت تنظیمات";
            await dialog.ShowAsync();
        }
    }
}
//sample send data
//private async void b1_Click(object sender, RoutedEventArgs e)
//{
//    OutString = "1-2-3-4";
//    sendTextButton_Click(null, null);
//}