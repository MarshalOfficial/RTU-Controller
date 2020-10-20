namespace SerialSample
{
    /// <summary>
    /// این کلاس ویژگی های لازم برای پورت سریال را تعریف می کند
    /// </summary>
    public class SeriClass
    {
        public string Baudrate { get; set; }
        public string Parity { get; set; }
        public string Stopbits { get; set; }
        public string Databits { get; set; }
        public int Readtimeout { get; set; }
        public int Writeout { get; set; }
        public int FirstDelay { get; set; }
        public int SecondDelay { get; set; }
        public int ThirdDelay { get; set; }
    }
}
 