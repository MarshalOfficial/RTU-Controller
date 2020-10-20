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
namespace SerialSample
{
    public sealed partial class Dashboard : Page
    {
        #region [Variables]
        private int red_state = 1;
        private bool green_state = false;
        private const int RED = 23;
        private const int GREEN = 24;
        private GpioPin redPin;
        private GpioPin greenPin;
        private SerialDevice serialPort = null;
        private DataWriter dataWriteObject = null;
        private DataReader dataReaderObject = null;
        private ObservableCollection<DeviceInformation> listOfDevices;
        private CancellationTokenSource ReadCancellationTokenSource;
        private string baudrate, parity, stopbits, databits;
        private int readtimeout, writeout;
        public static string OutString;
        public static string InString;
        private static bool SLOCK; // lock when a data send and wait for result .
        private static SqlServerRepository sqlDb;
        private static DatabaseHelperClass localDb;
        private Thread DateTimeThread; //  to download sql server datetime on local machine
        private Thread ClockThread; // to show clock on dashboard form
        private Thread FireCheckThread; // to process fires
        private Thread SenderThread; // to send shots
        private List<Thread> DashboardThreads; // to process dashboarditems with diffrent priority
        private static System.Timers.Timer SlockTimer; // stopwatch to check devices timeout and put Timeout value for requests
        private static Queue<Shot> ShotQueue; // to handle all shots into serial port
        private Thread UploadLogThread; // to send DashboardItemLog To Sql server   
        private static CacheEntity LocalCache;
        private static ErrorLog _errorLog;
        #endregion

        #region [Init]
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

        public Dashboard()
        {
            this.InitializeComponent();
            listOfDevices = new ObservableCollection<DeviceInformation>();
            InitCache();
            lblIpAddresss.Text = Extension.LocalIPAddress;
            _errorLog = new ErrorLog(LocalCache.Setting.SQLServerIP);
            sqlDb = new SqlServerRepository(LocalCache.Setting.SQLServerIP);
            localDb = new DatabaseHelperClass(LocalCache.Setting.SQLServerIP);
            lblCurrentSiteName.Text = LocalCache.Sites.FirstOrDefault(l => l.ID == LocalCache.Setting.SiteID).Name;
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
                ShowError(ex);_errorLog.SaveLog(ex);
                throw ex;
            }
        }
        #endregion

        #region [GPIO]
        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();
            if (redPin == null)
            {
                redPin = gpio.OpenPin(RED);
            }
            if (greenPin == null)
            {
                greenPin = gpio.OpenPin(GREEN);
            }
            redPin.Write(GpioPinValue.Low);
            greenPin.Write(GpioPinValue.Low);
            redPin.SetDriveMode(GpioPinDriveMode.Output);
            greenPin.SetDriveMode(GpioPinDriveMode.Output);
        }
        public void BeforeSend(TimeSpan delay)
        {
            redPin.Write(GpioPinValue.High);
            greenPin.Write(GpioPinValue.Low);
            Task.Delay(delay).Wait();
        }
        public void AfterSend(TimeSpan delay)
        {
            Task.Delay(delay).Wait();
            redPin.Write(GpioPinValue.Low);
            greenPin.Write(GpioPinValue.High);
        }
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
                ShowError(ex);_errorLog.SaveLog(ex);
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
                ShowError(ex);_errorLog.SaveLog(ex);
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
                if (ex.Message.ToLower().Contains("index was outside the bounds") || ex.Message.ToLower().Contains("indexoutofrangeexception"))
                {
                    CoreApplication.Exit();
                }
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
                ShowError(ex);_errorLog.SaveLog(ex);
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
        private async Task WriteAsync()
        {
            try
            {
                Task<UInt32> storeAsyncTask;
                var items = OutString.Split('-');
                foreach (var i in items)
                {
                    if (ValidateData(i))
                    {
                        continue;
                    }
                    else
                    {
                        throw new Exception("رشته ارسالی حاوی اطلاعات نامعتبر است مقدار بایستی به بایت باشد");
                    }
                }
                foreach (var i in items)
                {
                    dataWriteObject.WriteByte(Convert.ToByte(i));
                }

                BeforeSend(TimeSpan.FromMilliseconds(0));
                storeAsyncTask = dataWriteObject.StoreAsync().AsTask();
                AfterSend(TimeSpan.FromMilliseconds(0));
                Task.Delay(TimeSpan.FromMilliseconds(0)).Wait();
                UInt32 bytesWritten = await storeAsyncTask;
                if (bytesWritten > 0)
                {
                    //status.Text = sendText1.Text + ", ";
                    //status.Text += "اطلاعات با موفقیت ارسال شد";
                    OutString = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);_errorLog.SaveLog(ex);
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
            greenPin.Dispose();
            redPin.Dispose();
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
        private static DataTable firedt;
        private static DataTable dashitemdt;
        private static DataTable localfiredt;
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
                    if (bytesRead > 0)
                    {
                        InString = string.Empty;
                        for (int i = 0; i < bytesRead; i++)
                        {
                            InString += dataReaderObject.ReadByte().ToString() + "-";
                        }
                        InString = InString.Substring(0, InString.LastIndexOf("-"));
                        //status.Text = "اطلاعات با موفقیت دریافت شد";
                        //string test = InString;

                        if (ShotQueue == null || ShotQueue.Count == 0) { SLOCK = false; return; };
                        if (!string.IsNullOrWhiteSpace(InString) && !LocalCache.Devices.Select(l => l.ID).ToList().Contains(int.Parse(InString.Split('-')[0])))
                        {
                            return;
                        }
                        var onlineshot = ShotQueue.Peek();
                        //RefreshSlockTimer();
                        SlockTimer.Stop();
                        if (onlineshot.InstructionFire != null)
                        {
                            var fire = onlineshot.InstructionFire;
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
                                var crc = Extension.ComputeCRC(bytelist);
                                if (crc.CRC1 == int.Parse(crc1) && crc.CRC2 == int.Parse(crc2))
                                {
                                    var result = deviceresults.Find(l => l.Data.Replace("-", "") == data.Replace("-", ""));
                                    if (result != null && string.IsNullOrWhiteSpace(LocalCache.Instructions.FirstOrDefault(k => k.ID == fire.InstructionID).Formula))
                                    {
                                        // جواب از مدل عادی است
                                        fire.ResultID = result.ID;
                                        fire.Value = result.Memo;
                                        if (Extension.IsSqlServerAvailable(LocalCache.Setting.SQLServerIP))
                                        {
                                            sqlDb.UpdateInstructionFireOnServer(onlineshot.InstructionFire.ID, fire.Value, result.ID);
                                        }
                                    }
                                    else
                                    {
                                        // جواب از مدل خواندنی است و ولیو بایستی دیتکت شود
                                        fire.ResultID = 0;
                                        var dataarray = data.Split('-');
                                        var len = dataarray[0].ToInt();
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
                                            for (int k = 0; k < bytess.Length; k++)
                                            {
                                                if (!string.IsNullOrWhiteSpace(bytess[k]))
                                                {
                                                    bytess[k] = decimal.Parse(bytess[k].ToString()).ToString("N2");
                                                    switch (k)
                                                    {
                                                        case 0:
                                                            formulon = formulon.Replace("a", bytess[k]);
                                                            break;
                                                        case 1:
                                                            formulon = formulon.Replace("b", bytess[k]);
                                                            break;
                                                        case 2:
                                                            formulon = formulon.Replace("c", bytess[k]);
                                                            break;
                                                        case 3:
                                                            formulon = formulon.Replace("d", bytess[k]);
                                                            break;
                                                        case 4:
                                                            formulon = formulon.Replace("e", bytess[k]);
                                                            break;
                                                        case 5:
                                                            formulon = formulon.Replace("f", bytess[k]);
                                                            break;
                                                        case 6:
                                                            formulon = formulon.Replace("g", bytess[k]);
                                                            break;
                                                        case 7:
                                                            formulon = formulon.Replace("h", bytess[k]);
                                                            break;
                                                        case 8:
                                                            formulon = formulon.Replace("i", bytess[k]);
                                                            break;
                                                        case 9:
                                                            formulon = formulon.Replace("j", bytess[k]);
                                                            break;
                                                        case 10:
                                                            formulon = formulon.Replace("k", bytess[k]);
                                                            break;
                                                    }
                                                }
                                            }
                                            try
                                            {
                                                firedt = new DataTable();
                                                decimal decval = (decimal)firedt.Compute(formulon, "");
                                                fire.Value = decval.ToString("N2");
                                            }
                                            catch (Exception)
                                            {
                                                fire.Value = string.Empty;
                                            }

                                        }
                                        if (string.IsNullOrWhiteSpace(fire.Value))
                                        {
                                            fire.Value = value;
                                        }
                                        if (Extension.IsSqlServerAvailable(LocalCache.Setting.SQLServerIP))
                                        {
                                            sqlDb.UpdateInstructionFireOnServer(onlineshot.InstructionFire.ID, fire.Value, 0);
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
                        else if (onlineshot.DashboardItem != null)
                        {
                            var dashitem = LocalCache.DashboardItems.FirstOrDefault(l => l.ID == onlineshot.DashboardItem.ID);
                            var device = LocalCache.Devices.FirstOrDefault(l => l.ID == dashitem.DeviceID);
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
                                var crc = Extension.ComputeCRC(bytelist);
                                if (crc.CRC1 == int.Parse(crc1) && crc.CRC2 == int.Parse(crc2))
                                {
                                    var result = deviceresults.Find(l => l.Data.Replace("-", "") == data.Replace("-", ""));
                                    if (result != null && string.IsNullOrWhiteSpace(LocalCache.Instructions.FirstOrDefault(k => k.ID == LocalCache.DashboardItems.FirstOrDefault(l => l.ID == dashitem.ID).InstructionID).Formula))
                                    {
                                        // جواب از مدل عادی است
                                        dashitem.ResultID = result.ID;
                                        dashitem.Value = result.Memo;
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
                                        var len = dataarray[0].ToInt();
                                        string value = string.Empty;
                                        for (int j = 0; j < len; j++)
                                        {
                                            value += dataarray[j + 1] += "-";
                                        }
                                        //formula
                                        var formulon = LocalCache.Instructions.FirstOrDefault(k => k.ID == LocalCache.DashboardItems.FirstOrDefault(l => l.ID == dashitem.ID).InstructionID).Formula;

                                        if (!string.IsNullOrWhiteSpace(formulon))
                                        {
                                            var bytess = value.Split('-');
                                            for (int k = 0; k < bytess.Length; k++)
                                            {
                                                if (!string.IsNullOrWhiteSpace(bytess[k]))
                                                {
                                                    bytess[k] = decimal.Parse(bytess[k].ToString()).ToString("N2");
                                                    switch (k)
                                                    {
                                                        case 0:
                                                            formulon = formulon.Replace("a", bytess[k]);
                                                            break;
                                                        case 1:
                                                            formulon = formulon.Replace("b", bytess[k]);
                                                            break;
                                                        case 2:
                                                            formulon = formulon.Replace("c", bytess[k]);
                                                            break;
                                                        case 3:
                                                            formulon = formulon.Replace("d", bytess[k]);
                                                            break;
                                                        case 4:
                                                            formulon = formulon.Replace("e", bytess[k]);
                                                            break;
                                                        case 5:
                                                            formulon = formulon.Replace("f", bytess[k]);
                                                            break;
                                                        case 6:
                                                            formulon = formulon.Replace("g", bytess[k]);
                                                            break;
                                                        case 7:
                                                            formulon = formulon.Replace("h", bytess[k]);
                                                            break;
                                                        case 8:
                                                            formulon = formulon.Replace("i", bytess[k]);
                                                            break;
                                                        case 9:
                                                            formulon = formulon.Replace("j", bytess[k]);
                                                            break;
                                                        case 10:
                                                            formulon = formulon.Replace("k", bytess[k]);
                                                            break;
                                                    }
                                                }
                                            }
                                            try
                                            {
                                                dashitemdt = new DataTable();
                                                decimal decval = (decimal)dashitemdt.Compute(formulon, "");
                                                dashitem.Value = decval.ToString("N2");
                                            }
                                            catch (Exception)
                                            {
                                                dashitem.Value = string.Empty;
                                            }

                                        }
                                        if (string.IsNullOrWhiteSpace(dashitem.Value))
                                        {
                                            dashitem.Value = value;
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
                                        var scenariolst = LocalCache.DashboardScenarios.Where(l => l.DashboardItemID == dashitem.ID && l.FromTime <= now && l.ToTime >= now).ToList();
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
                                                if (maxval > 0 && dashval >= maxval)
                                                {
                                                    ShotQueue.Enqueue(new Shot(new LocalFireEntity() { DeviceID = item.ConnectedDeviceID, InstructionID = item.InstructionID, Type = "ScenarioFire" }));
                                                }
                                                else if (minval > 0 && dashval <= minval)
                                                {
                                                    ShotQueue.Enqueue(new Shot(new LocalFireEntity() { DeviceID = item.ConnectedDeviceID, InstructionID = item.InstructionID, Type = "ScenarioFire" }));
                                                }
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
                        else if (onlineshot.LocalFire != null)
                        {
                            var localfire = onlineshot.LocalFire;
                            var device = LocalCache.Devices.FirstOrDefault(l => l.ID == localfire.DeviceID);
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
                                var crc = Extension.ComputeCRC(bytelist);
                                if (crc.CRC1 == int.Parse(crc1) && crc.CRC2 == int.Parse(crc2))
                                {
                                    var result = deviceresults.Find(l => l.Data.Replace("-", "") == data.Replace("-", ""));
                                    if (result != null && string.IsNullOrWhiteSpace(LocalCache.Instructions.FirstOrDefault(k => k.ID == localfire.InstructionID).Formula))
                                    {
                                        // جواب از مدل عادی است
                                        localfire.ResultID = result.ID;
                                        localfire.Value = result.Memo;
                                        LocalCache.DashboardLogs.Add(new DashboardLogEntity()
                                        {
                                            InstructionID = localfire.InstructionID,
                                            Value = result.Memo,
                                            SaveTime = DateTime.UtcNow,
                                            IsSended = false,
                                            ResultID = result.ID,
                                            CreateDate = DateTime.UtcNow,
                                            Description = (onlineshot.LocalFire.Type == "LocalFire") ? "درخواست کاربر کنترلر تتیس" : "عملیات حاصل سناریو"
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
                                        var dataarray = data.Split('-');
                                        var len = dataarray[0].ToInt();
                                        string value = string.Empty;
                                        for (int j = 0; j < len; j++)
                                        {
                                            value += dataarray[j + 1] += "-";
                                        }
                                        //formula
                                        var formulon = LocalCache.Instructions.FirstOrDefault(k => k.ID == localfire.InstructionID).Formula;

                                        if (!string.IsNullOrWhiteSpace(formulon))
                                        {
                                            var bytess = value.Split('-');
                                            for (int k = 0; k < bytess.Length; k++)
                                            {
                                                if (!string.IsNullOrWhiteSpace(bytess[k]))
                                                {
                                                    bytess[k] = decimal.Parse(bytess[k].ToString()).ToString("N2");
                                                    switch (k)
                                                    {
                                                        case 0:
                                                            formulon = formulon.Replace("a", bytess[k]);
                                                            break;
                                                        case 1:
                                                            formulon = formulon.Replace("b", bytess[k]);
                                                            break;
                                                        case 2:
                                                            formulon = formulon.Replace("c", bytess[k]);
                                                            break;
                                                        case 3:
                                                            formulon = formulon.Replace("d", bytess[k]);
                                                            break;
                                                        case 4:
                                                            formulon = formulon.Replace("e", bytess[k]);
                                                            break;
                                                        case 5:
                                                            formulon = formulon.Replace("f", bytess[k]);
                                                            break;
                                                        case 6:
                                                            formulon = formulon.Replace("g", bytess[k]);
                                                            break;
                                                        case 7:
                                                            formulon = formulon.Replace("h", bytess[k]);
                                                            break;
                                                        case 8:
                                                            formulon = formulon.Replace("i", bytess[k]);
                                                            break;
                                                        case 9:
                                                            formulon = formulon.Replace("j", bytess[k]);
                                                            break;
                                                        case 10:
                                                            formulon = formulon.Replace("k", bytess[k]);
                                                            break;
                                                    }
                                                }
                                            }
                                            try
                                            {
                                                localfiredt = new DataTable();
                                                decimal decval = (decimal)localfiredt.Compute(formulon, "");
                                                localfire.Value = decval.ToString("N2");
                                            }
                                            catch (Exception)
                                            {
                                                localfire.Value = string.Empty;
                                            }

                                        }
                                        if (string.IsNullOrWhiteSpace(localfire.Value))
                                        {
                                            localfire.Value = value;
                                        }
                                        //
                                        LocalCache.DashboardLogs.Add(new DashboardLogEntity()
                                        {
                                            InstructionID = localfire.InstructionID,
                                            Value = localfire.Value,
                                            SaveTime = DateTime.UtcNow,
                                            IsSended = false,
                                            ResultID = 0,
                                            CreateDate = DateTime.UtcNow,
                                            Description = (onlineshot.LocalFire.Type == "LocalFire") ? "درخواست کاربر کنترلر تتیس" : "عملیات حاصل سناریو"
                                        });
                                        if (onlineshot.LocalFire.Type == "LocalFire")
                                        {
                                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { UpdateLocalFireResultUI(localfire.Value); });
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
                                        Description = (onlineshot.LocalFire.Type == "LocalFire") ? "درخواست کاربر کنترلر تتیس" : "عملیات حاصل سناریو"
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
                    }
                }
            }
            catch (Exception ex)
            {
                ///ShowError(ex);
                _errorLog.SaveLog(ex);
                if(ex.Message.ToLower().Contains("index was outside the bounds") || ex.Message.ToLower().Contains("indexoutofrangeexception"))
                {
                    CoreApplication.Exit();
                }
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
                ShowError(ex);_errorLog.SaveLog(ex);
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
                ShowError(ex);_errorLog.SaveLog(ex);
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
                ShowError(ex); _errorLog.SaveLog(ex);
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
                                //if (localDb.GetDeviceInstructionFire(fire.ID) == null)
                                //{
                                //    localDb.InsertInstructionFire(fire);
                                //}
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
                ShowError(ex);_errorLog.SaveLog(ex);
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
                            var dash = LocalCache.DashboardItems.Where(l => LocalCache.Devices.Where(k=>k.LocationID == LocalCache.Setting.SiteID).Select(j => j.ID).ToList().Contains(l.DeviceID) && l.Priority == priority.ID);
                            foreach (var item in dash)
                            {
                                if(!ShotQueue.Any(l => l.DashboardItem != null && l.DashboardItem.ID == item.ID))
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
                ShowError(ex);_errorLog.SaveLog(ex);
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
                            var item = ShotQueue.Peek();
                            InstructionEntity instruction;
                            DeviceEntity device;
                            if (item.InstructionFire != null)
                            {
                                instruction = LocalCache.Instructions.FirstOrDefault(l => l.ID == item.InstructionFire.InstructionID);
                                device = LocalCache.Devices.FirstOrDefault(l => l.ID == item.InstructionFire.DeviceID);
                            }
                            else if(item.DashboardItem != null)
                            {
                                instruction = LocalCache.Instructions.FirstOrDefault(l => l.ID == item.DashboardItem.InstructionID);
                                device = LocalCache.Devices.FirstOrDefault(l => l.ID == item.DashboardItem.DeviceID);
                            }
                            else
                            {// local fire
                                instruction = LocalCache.Instructions.FirstOrDefault(l => l.ID == item.LocalFire.InstructionID);
                                device = LocalCache.Devices.FirstOrDefault(l => l.ID == item.LocalFire.DeviceID);
                            }
                            var strtofire = device.RowNow + "-" + instruction.FunctionCode + "-" + instruction.Data;
                            var crc = Extension.ComputeCRC(strtofire.ToByteList());
                            strtofire += "-" + crc.CRC1.ToString() + "-" + crc.CRC2.ToString();
                            SLOCK = true;
                            OutString = strtofire;
                            SlockTimer.Stop();
                            SlockTimer.Start();
                            sendTextButton_Click(null, null);
                        }
                    }
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);_errorLog.SaveLog(ex);
                throw ex;
            }
        }
        #endregion

        #region [HandleSendTimeout]
        private void RefreshSlockTimer()
        {
            SlockTimer.Stop();
            SlockTimer.Start();
        }
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
                            Description = (onlineshot.LocalFire.Type == "LocalFire") ? "درخواست کاربر کنترلر تتیس" : "عملیات حاصل سناریو"
                        });
                        if (onlineshot.LocalFire.Type == "LocalFire")
                        {
                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { UpdateLocalFireResultUI("Timeout"); });
                        }
                    }
                    if (ShotQueue.Count > 0) { ShotQueue.Dequeue(); }
                    SLOCK = false;
                }
                //RefreshSlockTimer();
            }
            catch(Exception ex)
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
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { ImgConnection.Source = (Extension.IsSqlServerAvailable(LocalCache.Setting.SQLServerIP) ? new BitmapImage(new Uri("ms-appx:/Assets/connect.png", UriKind.Absolute)): new BitmapImage(new Uri("ms-appx:/Assets/disconnect.png", UriKind.Absolute)) );});
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
                                var lst = await localDb.GetAllDashboardLog();
                                if (lst != null && lst.Count > 0)
                                {
                                    SendLock = true;
                                    sqlDb.UploadDashboardLogsToServer(lst);
                                    localDb.DeleteAllDashboardLog();
                                }
                            }
                            catch (Exception ex)
                            {
                                _errorLog.SaveLog(ex);
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
                ShowError(ex);_errorLog.SaveLog(ex);
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
                foreach (var itm in LocalCache.Devices.Where(l=>l.LocationID == LocalCache.Setting.SiteID))
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
            catch(Exception ex)
            {
                _errorLog.SaveLog(ex);
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
                if(pivotMain.SelectedItem != null)
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
        private void pivotMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var pivot = (PivotItem)(sender as Pivot).SelectedItem;
                if (pivot.Content is DashContent content)
                {
                    content.Refresh(LocalCache);
                }
            }
            catch (Exception ex)
            {
                _errorLog.SaveLog(ex);
                throw ex;
            }
        }
        #endregion


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