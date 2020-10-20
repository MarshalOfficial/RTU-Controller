using System;
using System.Collections.ObjectModel;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Windows.Devices.Gpio;

namespace SerialSample
{
    public static class SerialManager 
    {
        static private int red_state = 1;
        private static bool green_state = false;
        private const int RED = 23;
        private const int GREEN = 24;
        private static GpioPin redPin;
        private static GpioPin greenPin;
        private static SerialDevice serialPort = null;
        private static DataWriter dataWriteObject = null;
        private static DataReader dataReaderObject = null;
        private static ObservableCollection<DeviceInformation> listOfDevices;
        private static CancellationTokenSource ReadCancellationTokenSource;
        private static string baudrate, parity, stopbits, databits;
        private static int readtimeout, writeout, firstdelay, seconddelay, thirddelay;

        public static string OutString;
        public static string InString;   

        public static void Init(SeriClass obj)
        {
            baudrate = obj.Baudrate;
            parity = obj.Parity;
            stopbits = obj.Stopbits;
            databits = obj.Databits;
            readtimeout = obj.Readtimeout;
            writeout = obj.Writeout;
            firstdelay = obj.FirstDelay;
            seconddelay = obj.SecondDelay;
            thirddelay = obj.ThirdDelay;
            listOfDevices = new ObservableCollection<DeviceInformation>();
            ListAvailablePorts();
            InitGPIO();
        }

        private static async void ListAvailablePorts()
        {
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);
                for (int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Add(dis[i]);
                }
                SelectComPort();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static void InitGPIO()
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

        public static async void SelectComPort()
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
                ReadCancellationTokenSource = new CancellationTokenSource();
                Listen();
                SendData("1-2-3-4-5-6-7-8-9-10");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static async void Listen()
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
                CloseDevice();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }
        public delegate void OnDataReadHandler(object sender, DataReadEventArgs e);
        public static event OnDataReadHandler OnDataRead;  
        private static async Task ReadAsync(CancellationToken cancellationToken)
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
                        OnDataRead(null, new DataReadEventArgs(InString));
                    }
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        private static void CloseDevice()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;
            listOfDevices.Clear();
        }

        private static void CancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
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
            catch (Exception)
            {
                return false;
            }
        }

        public static async void SendData(string data)
        {
            try
            {
                if (serialPort != null)
                {
                    OutString = data; 
                    dataWriteObject = new DataWriter(serialPort.OutputStream);
                    await WriteAsync();
                }
                else
                {    
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (dataWriteObject != null)
                {
                    dataWriteObject.DetachStream();
                    dataWriteObject = null;
                }
            }
        }
        public delegate void OnDataSendHandler(object sender, DataSendEventArgs e);
        public static event OnDataSendHandler OnDataSend;      
        private static async Task WriteAsync()
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
                BeforeSend(TimeSpan.FromMilliseconds(firstdelay));
                storeAsyncTask = dataWriteObject.StoreAsync().AsTask();
                AfterSend(TimeSpan.FromMilliseconds(seconddelay));
                Task.Delay(TimeSpan.FromMilliseconds(thirddelay)).Wait();
                UInt32 bytesWritten = await storeAsyncTask;
                if (bytesWritten > 0)
                {
                    //status.Text += "اطلاعات با موفقیت ارسال شد";
                    OnDataSend(null, new DataSendEventArgs(OutString));
                    OutString = string.Empty;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static void BeforeSend(TimeSpan delay)
        {
            redPin.Write(GpioPinValue.High);
            greenPin.Write(GpioPinValue.Low);
            Task.Delay(delay).Wait();
        }
        private static void AfterSend(TimeSpan delay)
        {
            Task.Delay(delay).Wait();
            redPin.Write(GpioPinValue.Low);
            greenPin.Write(GpioPinValue.High);
        }


        //#region [Dispose]
        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}
        //protected virtual void Dispose(bool disposing)
        //{
        //    if (disposed)
        //    { return; }
        //    if (disposing)
        //    {
        //        CancelReadTask();
        //        CloseDevice();
        //        greenPin.Dispose();
        //        redPin.Dispose();
        //    }
        //    disposed = true;
        //}

        //~SerialManager()
        //{
        //    Dispose(false);
        //}
        //#endregion
    }
    public class DataSendEventArgs : EventArgs
    {   
        public string Data { get; private set; }

        public DataSendEventArgs(string data)
        {
            Data = data;
        }
    }
    public class DataReadEventArgs : EventArgs
    {
        public string Data { get; private set; }

        public DataReadEventArgs(string data)
        {
            Data = data;    
        }
    }

}