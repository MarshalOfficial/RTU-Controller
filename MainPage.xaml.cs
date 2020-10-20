// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using Windows.Devices.Gpio;
using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;
using THTController;

namespace SerialSample
{    
    /// <summary>
    /// فرم تست ارسال
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //
        private int red_state = 1;
        private bool green_state = false;
        private const int RED = 23;
        private const int GREEN = 24;
        private GpioPin redPin;
        private GpioPin greenPin;
        //
        /// <summary>
        /// Private variables
        /// </summary>
        private SerialDevice serialPort = null;
        DataWriter dataWriteObject = null;
        DataReader dataReaderObject = null;

        private ObservableCollection<DeviceInformation> listOfDevices;
        private CancellationTokenSource ReadCancellationTokenSource;

        public string baudrate, parity, stopbits, databits;
        public int readtimeout, writeout;


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


        public MainPage()
        {
            this.InitializeComponent();            
            sendTextButton.IsEnabled = false;
            listOfDevices = new ObservableCollection<DeviceInformation>();
            ListAvailablePorts();
            InitGPIO();
        }

        /// <summary>
        /// ListAvailablePorts
        /// - Use SerialDevice.GetDeviceSelector to enumerate all serial devices
        /// - Attaches the DeviceInformation to the ListBox source so that DeviceIds are displayed
        /// </summary>
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
                status.Text = ex.Message;
            }
        }

        /// <summary>
        /// comPortInput_Click: Action to take when 'Connect' button is clicked
        /// - Get the selected device index and use Id to create the SerialDevice object
        /// - Configure default settings for the serial port
        /// - Create the ReadCancellationTokenSource token
        /// - Start listening on the serial port input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void comPortInput_Click(object sender, RoutedEventArgs e)
        {
            var selection = listOfDevices;

            if (selection.Count <= 0)
            {
                status.Text = "Select a device and connect";
                return;
            }

            DeviceInformation entry = (DeviceInformation)selection[0];         

            try
            {                
                serialPort = await SerialDevice.FromIdAsync(entry.Id);
                if (serialPort == null) return;

                // Configure serial settings
                serialPort.WriteTimeout = TimeSpan.FromMilliseconds(writeout);
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(readtimeout);
                serialPort.BaudRate = uint.Parse(baudrate);
                switch(parity)
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

                // Display configured settings
                status.Text = "Serial port configured successfully: ";
                status.Text += serialPort.BaudRate + "-";
                status.Text += serialPort.DataBits + "-";
                status.Text += serialPort.Parity.ToString() + "-";

                status.Text += serialPort.StopBits;

                // Set the RcvdText field to invoke the TextChanged callback
                // The callback launches an async Read task to wait for data
                //rcvdText.Text = "منتظر دریافت اطلاعات...";

                // Create cancellation token object to close I/O operations when closing the device
                ReadCancellationTokenSource = new CancellationTokenSource();

                // Enable 'WRITE' button to allow sending data
                sendTextButton.IsEnabled = true;

                Listen();
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
                sendTextButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// sendTextButton_Click: Action to take when 'WRITE' button is clicked
        /// - Create a DataWriter object with the OutputStream of the SerialDevice
        /// - Create an async task that performs the write operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void sendTextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {                
                if (serialPort != null)
                {
                    // Create the DataWriter object and attach to OutputStream
                    dataWriteObject = new DataWriter(serialPort.OutputStream);

                    //Launch the WriteAsync task to perform the write
                    await WriteAsync();
                }
                else
                {
                    status.Text = "Select a device and connect";                
                }
            }
            catch (Exception ex)
            {
                status.Text = "sendTextButton_Click: " + ex.Message;
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

        

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            greenPin.Dispose();
            redPin.Dispose();
            serialPort.Dispose();
        }
       
        
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
        private async Task WriteAsync()
        {
            Task<UInt32> storeAsyncTask;

            //
            //InitGPIO();
            //green_state = !green_state;
            //while (green_state == true)
            //{
            //    redPin.Write(GpioPinValue.High);
            //    await Task.Delay(500);
            //    redPin.Write(GpioPinValue.Low); ;
            //    await Task.Delay(500);
            //}
            //

            var crc = Extension.ComputeCRC(sendText1.Text.ToByteList());
            var itms = sendText1.Text.Split('-');
            foreach (var item in itms)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    dataWriteObject.WriteByte(Convert.ToByte(item));
                }
            }
            dataWriteObject.WriteByte(Convert.ToByte(crc.CRC1.ToString()));
            dataWriteObject.WriteByte(Convert.ToByte(crc.CRC2.ToString()));
            BeforeSend(TimeSpan.FromMilliseconds(ToInt(tbxFirstDelay.Text)));
            storeAsyncTask = dataWriteObject.StoreAsync().AsTask();
            AfterSend(TimeSpan.FromMilliseconds(ToInt(tbxSecondDelay.Text)));
            Task.Delay(TimeSpan.FromMilliseconds(ToInt(tbxThirdDelay.Text))).Wait();
            UInt32 bytesWritten = await storeAsyncTask;
            if (bytesWritten > 0)
            {
                status.Text = sendText1.Text + ", ";
                status.Text += "اطلاعات با موفقیت ارسال شد";
            }
            ClearInputData();

        }
        private int ToInt(string txt)
        {
            try
            {
                return int.Parse(txt);
            }
            catch (Exception)
            {
                return 0;
            }
        }
        /// <summary>
        /// - Create a DataReader object
        /// - Create an async task to read from the SerialDevice InputStream
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
                    dataReaderObject = new DataReader(serialPort.InputStream);

                    // keep reading the serial input
                    while (true)
                    {
                        await ReadAsync(ReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (TaskCanceledException tce) 
            {
                status.Text = "Reading task was cancelled, closing device and cleaning up";
                CloseDevice();            
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
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
        
        private void btnGoToSetting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                greenPin.Dispose();
                redPin.Dispose();
                this.Frame.Navigate(typeof(frmConfig), new SeriClass());
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
            }
        }

        private void ClearInputData()
        {
            rcvdText1.Text = rcvdText2.Text = rcvdText3.Text = rcvdText4.Text = rcvdText5.Text = rcvdText6.Text =
            rcvdText7.Text = rcvdText8.Text = rcvdText9.Text = rcvdText10.Text = rcvdText11.Text = rcvdText12.Text = string.Empty;
        }

        /// <summary>
        /// ReadAsync: Task that waits on data and reads asynchronously from the serial device InputStream
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 256;

            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            using (var childCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                // Create a task object to wait for data on the serialPort.InputStream
                loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(childCancellationTokenSource.Token);

                // Launch the task and wait
                UInt32 bytesRead = await loadAsyncTask;
                if (bytesRead > 0)
                {
                    //rcvdText.Text += dataReaderObject.ReadByte().ToString() + "-";
                    ClearInputData();
                    for (int i = 0; i < bytesRead; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                rcvdText1.Text = dataReaderObject.ReadByte().ToString();
                                break;
                            case 1:
                                rcvdText2.Text = dataReaderObject.ReadByte().ToString();
                                break;
                            case 2:
                                rcvdText3.Text = dataReaderObject.ReadByte().ToString();
                                break;
                            case 3:
                                rcvdText4.Text = dataReaderObject.ReadByte().ToString();
                                break;
                            case 4:
                                rcvdText5.Text = dataReaderObject.ReadByte().ToString();
                                break;
                            case 5:
                                rcvdText6.Text = dataReaderObject.ReadByte().ToString();
                                break;
                            case 6:
                                rcvdText7.Text = dataReaderObject.ReadByte().ToString();
                                break;
                            case 7:
                                rcvdText8.Text = dataReaderObject.ReadByte().ToString();
                                break;
                            case 8:
                                rcvdText9.Text = dataReaderObject.ReadByte().ToString();
                                break;
                            case 9:
                                rcvdText10.Text = dataReaderObject.ReadByte().ToString();
                                break;
                            case 10:
                                rcvdText11.Text = dataReaderObject.ReadByte().ToString();
                                break;
                            case 11:
                                rcvdText12.Text = dataReaderObject.ReadByte().ToString();
                                break;
                        }
                    }
                    //rcvdText.Text += dataReaderObject.ReadByte().ToString() + "-" +
                    //                dataReaderObject.ReadByte().ToString() + "-" + 
                    //                dataReaderObject.ReadByte().ToString() + "-" +
                    //                dataReaderObject.ReadByte().ToString() + "-" +
                    //                dataReaderObject.ReadByte().ToString() + "-" +
                    //                dataReaderObject.ReadByte().ToString() + "-" +
                    //                dataReaderObject.ReadByte().ToString() + "-" +
                    //                dataReaderObject.ReadByte().ToString();
                    status.Text = "اطلاعات با موفقیت دریافت شد";
                }
            }
        }

        /// <summary>
        /// CancelReadTask:
        /// - Uses the ReadCancellationTokenSource to cancel read operations
        /// </summary>
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

        /// <summary>
        /// CloseDevice:
        /// - Disposes SerialDevice object
        /// - Clears the enumerated device Id list
        /// </summary>
        private void CloseDevice()
        {            
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;
            sendTextButton.IsEnabled = false;
            ClearInputData();
            listOfDevices.Clear();               
        }

        /// <summary>
        /// closeDevice_Click: Action to take when 'Disconnect and Refresh List' is clicked on
        /// - Cancel all read operations
        /// - Close and dispose the SerialDevice object
        /// - Enumerate connected devices
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeDevice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                status.Text = "";
                CancelReadTask();
                CloseDevice();
                ListAvailablePorts();
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
            }          
        }

        

    }
}
