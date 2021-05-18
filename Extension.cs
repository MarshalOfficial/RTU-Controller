using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
namespace THTController
{
    /// <summary>
    /// کلاس اکستنشن
    /// تمامی توابع به درد بخور و کتابخونه ای اعم از تبدیل نوع ها ، محاسبه فرمول ها ، محاسبه سی ار سی و ... در این کلاس توسعه داده شده است
    /// </summary>
    public static class Extension
    {
        /// <summary>
        /// تبدیل هر آبجکتی به اینت
        /// </summary>
        /// <param name="obj">آبجکت پاس داده شده جهت تبدیل</param>
        public static int ToInt(this object obj)
        {
            try
            {
                return int.Parse(obj.ToString());
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// تبدیل هر آبجکتی به دسیمال
        /// </summary>
        /// <param name="obj">آبجکت پاس داده شده جهت تبدیل</param>
        public static decimal ToDecimal(this object obj)
        {
            try
            {
                return decimal.Parse(obj.ToString());
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// تبدیل هر آبجکتی به هگزیمال
        /// </summary>
        /// <param name="obj">آبجکت پاس داده شده جهت تبدیل</param>
        public static string ToHex(this object obj)
        {
            try
            {
                return obj.ToInt().ToString("X");
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// تبدیل استرینگ به اینت
        /// </summary>
        /// <param name="obj">استرینگ پاس داده شده جهت تبدیل</param>
        public static int ToInt(this string obj)
        {
            try
            {
                return int.Parse(obj, System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// این تابع جهت چک کردن اتصال به اسکیوال سرور می باشد
        /// </summary>
        /// <param name="_ip">ای پی سرور مورد نظر را با این متغیر پاس می دهیم</param>
        /// <returns></returns>
        public static bool IsSqlServerAvailable(string _ip)
        {
            string ip = _ip;//(string.IsNullOrWhiteSpace(_ip)) ? new DatabaseHelperClass().GetSetting(0).SQLServerIP : _ip;
            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    tcpClient.SendTimeout = tcpClient.ReceiveTimeout = 1000;
                    tcpClient.Connect(ip, 1433);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// این پراپرتی ای پی ادرس لوکال کنترلر را پس می دهد
        /// </summary>
        public static string LocalIPAddress
        {
            get
            {
                try
                {
                    List<string> IpAddress = new List<string>();
                    var Hosts = Windows.Networking.Connectivity.NetworkInformation.GetHostNames().ToList();
                    foreach (var Host in Hosts.Where(a => a.DisplayName.StartsWith("1") && a.DisplayName.Contains(".")))
                    {
                        string IP = Host.DisplayName;
                        IpAddress.Add(IP);
                    }
                    IPAddress address = IPAddress.Parse(IpAddress.Last());
                    return address.ToString();
                }
                catch (Exception ex)
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// تابع محاسبه سی ار سی
        /// </summary>
        /// <param name="bytes">لیست بایت ها پاس داده می شود</param>
        /// <returns></returns>
        public static CRCEntity ComputeCRC(List<byte> bytes)
        {
            byte[] buf = bytes.ToArray();
            UInt16 crc = 0xFFFF;
            for (int pos = 0; pos < bytes.Count; pos++)
            {
                crc ^= (UInt16)buf[pos];
                for (int i = 8; i != 0; i--)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                        crc >>= 1;
                }
            }
            var upper = (byte)(crc >> 8);
            var lower = (byte)(crc & 0xff);
            var upint = int.Parse(upper.ToString());
            var lowint = int.Parse(lower.ToString());
            return new CRCEntity() { CRC1 = lowint, CRC2 = upint };
        }


        /// <summary>
        /// لیستی از بایت ها را در رشته ای که دش دارد می گیرد و به صورت لیست بر میگرداند
        /// </summary>
        /// <param name="bytestringwithdash">لیست بایت ها به این صورت پاس داده می شود 1-2-3-4-5-6-7-8-9</param>
        /// <returns>بایت ها را به صورت لیست برمیگرداند</returns>
        public static List<byte> ToByteList(this string bytestringwithdash)
        {
            var strs = bytestringwithdash.Split('-').ToList();
            var bytes = new List<byte>();
            foreach (var item in strs)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                bytes.Add(byte.Parse(item));
            }
            return bytes;
        }


        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// تبدیل از ای تری ای به عدد اعشاری
        /// </summary>
        /// <param name="ieeeObj">آبجکت در فرمت ای تری ای بهش پاس داده می شود</param>
        /// <returns>به صورت عدد اعشاری برمیگرداند</returns>
        public static double ConvertIeeeToDouble(this object ieeeObj)
        {   
            try
            {
                var fb = Convert.ToUInt32(ieeeObj);
                return BitConverter.ToSingle(BitConverter.GetBytes((int)fb), 0);
            }
            catch (Exception e)
            {
                return 0;
            }
            
        }

        /// <summary>
        /// لیست حروف انگلیسی را از فرمول در میاورد و پاس می دهد
        /// </summary>
        /// <param name="formula">رشته فرمول</param>
        /// <returns></returns>
        public static List<string> GetFormulaAlphabetPartOnly(this string formula)
        {
            var result = new List<string>();
            var test = formula.Split('(', ')', '*', '+', '-', '/');//.Where(x => !string.IsNullOrWhiteSpace(x)).Where(l => !decimal.TryParse(l, out var n)).ToArray();
            foreach (var s in test)
            {
                if (!string.IsNullOrWhiteSpace(s) && !decimal.TryParse(s, out var n) && result.All(l => l != s))
                {
                    result.Add(s);
                }    
            }
            return result;
        }


        /// <summary>
        /// گرفتن شماره ایندکس بایت هر حرف انگلیسی
        /// به عنوان مثال a می شود بایت صفرم
        /// a1 میشود بایت 26 ام
        /// </summary>
        /// <param name="charr">کاراکتر انگلیسی پاس داده می شود مثلا a1 یا g9</param>
        /// <returns>شماره بایتش در لیست بایت ها محاسبه و برمی گرداند</returns>
        public static long GetCharIndexInFormula(this string charr)
        {
            try
            {
                //a,b,c,d,....
                //a1,b1,c1,d1,...
                //a2,b2,c2,d2,....
                var str = charr.ToLower();
                var alphabet = str[0];
                var number = str.Replace(alphabet.ToString(), string.Empty);
                if (string.IsNullOrWhiteSpace(number))
                {
                    return GetEnglishAlphabetIndex(alphabet);
                }
                else
                {
                    return (GetEnglishAlphabetIndex(alphabet) + (long.Parse(number) * 26));
                }
            }
            catch (Exception e)
            {
                return 0;
            }
        }


        /// <summary>
        /// ایندکس حروف الفبای انگلیسی
        /// </summary>
        /// <param name="c">کاراکتر انگلیسی پاس داده شده</param>
        /// <returns>ایندکسش توی ترتیب حروف الفبا</returns>
        public static int GetEnglishAlphabetIndex(char c)
        {
            switch (c)
            {
                case 'a':
                    return 0;
                case 'b':
                    return 1;
                case 'c':
                    return 2;
                case 'd':
                    return 3;
                case 'e':
                    return 4;
                case 'f':
                    return 5;
                case 'g':
                    return 6;
                case 'h':
                    return 7;
                case 'i':
                    return 8;
                case 'j':
                    return 9;
                case 'k':
                    return 10;
                case 'l':
                    return 11;
                case 'm':
                    return 12;
                case 'n':
                    return 13;
                case 'o':
                    return 14;
                case 'p':
                    return 15;
                case 'q':
                    return 16;
                case 'r':
                    return 17;
                case 's':
                    return 18;
                case 't':
                    return 19;
                case 'u':
                    return 20;
                case 'v':
                    return 21;
                case 'w':
                    return 22;
                case 'x':
                    return 23;
                case 'y':
                    return 24;
                case 'z':
                    return 25;
                default:
                    return 0;
            }
        }
    }
    public class CRCEntity
    {
        public int CRC1 { get; set; } // down byte   
        public int CRC2 { get; set; } // upper byte   
    }
}