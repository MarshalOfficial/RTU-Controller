using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
namespace THTController.Date
{
    [Serializable]
    public struct PersianDate : IComparable,

        IComparable<PersianDate>,

        IComparer,

        IComparer<PersianDate>,

        ICloneable
    {
        #region Fields

        private static readonly PersianCalendar calendar = new PersianCalendar();

        public static readonly PersianDate MinValue = new PersianDate(1, 1, 1);

        public static readonly PersianDate MaxValue = new PersianDate(1500, 12, 29, 11, 59, 59);

        private readonly string[] weekArray;

        private readonly string[] monthsArray; 

        private int year;

        private int month;

        private int day;

        private int hour;

        private int minute;

        private int second;

        #endregion

        #region Ctor

        public PersianDate(int year, int month, int day, int hour, int minute, int second)
        {

            if (!PersianDate.ValidDate(year, month, day, hour, minute, second))

                throw new ArgumentException("Date time is not valid");

            this.year = year;

            this.month = month;

            this.day = day;

            this.hour = hour;

            this.minute = minute;

            this.second = second;

            weekArray = new string[] {"شنبه", "یشنبه", "دوشنبه", "سه شنبه", "هارشنبه", "نجشنبه", "جمعه"};

            monthsArray = new string[]

            {

                "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی",

                "بهمن", "اسفند"

            };

            var dt = calendar.ToDateTime(year, month, day, hour, minute, second, 0);

            this.DayOfWeek = calendar.GetDayOfWeek(dt);

        }

        /// <summary>

        ///

        /// </summary>

        /// <param name="year">شماره سال به شمسی</param>

        /// <param name="month">شماره ماه به شمسی</param>

        /// <param name="day">شماره روز یه شمسی</param>

        public PersianDate(int year, int month, int day)

            : this(year, month <= 0 ? 1 : month, day <= 0 ? 1 : day, 0, 0, 0)
        {

        }

        #endregion

        #region Props

        public int Year
        {

            get { return year; }

            set
            {

                if (!PersianDate.ValidDate(value, month, day, hour, minute, second))

                    throw new ArgumentException("Year is out of range");

                year = value;

            }

        }

        public int Month
        {

            get { return month; }

            set
            {

                if (!PersianDate.ValidDate(year, value, day, hour, minute, second))

                    throw new ArgumentException("Month is out of range");

                month = value;

            }

        }

        public int Day
        {

            get { return day; }

            set
            {

                if (!PersianDate.ValidDate(year, month, value, hour, minute, second))

                    throw new ArgumentException("Day is out of range");

                day = value;

            }

        }

        public int Hour
        {

            get { return hour; }

            set
            {

                if (!PersianDate.ValidDate(year, month, day, value, minute, second))

                    throw new ArgumentException("Day is out of range");

                hour = value;

            }

        }

        public int Minute
        {

            get { return minute; }

            set
            {

                if (!PersianDate.ValidDate(year, month, day, hour, value, second))

                    throw new ArgumentException("Day is out of range");

                minute = value;

            }

        }

        public int Second
        {

            get { return second; }

            set
            {

                if (!PersianDate.ValidDate(year, month, day, hour, minute, value))

                    throw new ArgumentException("Day is out of range");

                second = value;

            }

        }

        public DayOfWeek DayOfWeek { get; private set; }

        public string DayOfWeekShamsi
        {

            get
            {
                switch (DayOfWeek)
                {
                    case DayOfWeek.Friday:
                        return "جمعه";
                    case DayOfWeek.Monday:
                        return "دوشنبه";
                    case DayOfWeek.Saturday:
                        return "شنبه";
                    case DayOfWeek.Sunday:
                        return "یکشنبه";
                    case DayOfWeek.Thursday:
                        return "پنجشنبه";
                    case DayOfWeek.Tuesday:
                        return "سه شنبه";
                    case DayOfWeek.Wednesday:
                        return "چهارشنبه";
                }
                return string.Empty;
            }
        }

        public static PersianDate Now => Parse(DateTime.Now);

        #endregion

        #region Methods

        #region Static Methods

        public static bool ValidDate(int year, int month, int day, int hour, int minute, int second)
        {

            var result = true;

            try
            {

                calendar.ToDateTime(year, month, day, hour, minute, second, 0);

            }

            catch (ArgumentException)
            {

                result = false;

            }

            return result;

        }

        [Obsolete("please use valid date method ValidDate(1388,05,06,04,10,20) ")]

        public static bool ValidDate(int year, int month, int day)
        {

            var result = true;

            try
            {

                calendar.ToDateTime(year, month, day, 0, 0, 0, 0);

            }

            catch (ArgumentException)
            {

                result = false;

            }

            return result;

        }

        public static bool ValidDate(PersianDate persianDate)
        {

            return ValidDate(persianDate.Year, persianDate.Month, persianDate.Day, persianDate.Hour, persianDate.Minute,

                persianDate.Second);

        }

        public static int Compare2Date(PersianDate persianDate1, PersianDate persianDate2)
        {

            if (persianDate1.year > persianDate2.Year)

                return 1;

            else if (persianDate1.year < persianDate2.Year)

                return -1;



            if (persianDate1.month > persianDate2.Month)

                return 1;

            else if (persianDate1.month < persianDate2.Month)

                return -1;



            if (persianDate1.day > persianDate2.Day)

                return 1;

            else if (persianDate1.day < persianDate2.Day)

                return -1;

            if (persianDate1.hour > persianDate2.hour)

                return 1;

            else if (persianDate1.hour < persianDate2.hour)

                return -1;



            if (persianDate1.minute > persianDate2.Minute)

                return 1;

            else if (persianDate1.minute < persianDate2.Minute)

                return -1;



            if (persianDate1.second > persianDate2.Second)

                return 1;

            else if (persianDate1.second < persianDate2.Second)

                return -1;



            return 0;

        }

        /// <summary>

        /// تبدیل تاریخ میلادی به تاریخ شمسی

        /// </summary>

        /// <param name="date"></param>

        /// <returns></returns>

        public static PersianDate Parse(DateTime date)
        {

            var pd = new PersianDate(

                calendar.GetYear(date),

                calendar.GetMonth(date),

                calendar.GetDayOfMonth(date),

                calendar.GetHour(date),

                calendar.GetMinute(date),

                calendar.GetSecond(date))

            {

                DayOfWeek = calendar.GetDayOfWeek(date)

            };

            return pd;
        }

        /// <summary>

        /// تبدیل تاریخ میلادی به شمسی

        /// </summary>

        /// <param name="year">سال</param>

        /// <param name="month">ماه</param>

        /// <param name="day">روز</param>

        /// <param name="hour">ساعت</param>

        /// <param name="minute">دقیقه</param>

        /// <param name="second">ثانیه</param>

        /// <returns>تاریخ شمسی</returns>

        public static PersianDate Parse(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
        {

            var dt = new DateTime(year, month, day, hour, minute, second);

            return Parse(dt);

        }

        /// <summary>

        /// تبدیل تاریخ شمسی به تاریخ میلادی

        /// </summary>

        /// <param name="persianDate">تاریخ فارسی</param>

        /// <returns>تاریخ میلادی</returns>

        public static DateTime ConvertToGregorian(PersianDate persianDate)
        {

            var shamsiDate = persianDate.Year + "" + persianDate.month + "" + persianDate.day;

            var returnDate = ConvertToGregorian(shamsiDate);

            return returnDate;

        }

        /// <summary>

        /// تبدیل رشته تاریخ شمسی به تاریخ میلادی

        /// </summary>

        /// <param name="persianDate">رشته تاریخ فارسی</param>

        /// <returns>تاریخ میلادی</returns>

        public static DateTime ConvertToGregorian(string shamsiDate)
        {

            shamsiDate = shamsiDate.Replace("/", "").Trim();

            var year = int.Parse(shamsiDate.Substring(0, 4));

            var month = int.Parse(shamsiDate.Substring(4, 2));

            var day = int.Parse(shamsiDate.Substring(6, 2));

            var p = new PersianCalendar();

            var date = p.ToDateTime(year, month, day, 0, 0, 0, 0);

            return date;

        }

        #endregion

        #region Public Methods

        public PersianDate AddHours(int value)
        {

            try
            {

                return (PersianDate) (((DateTime) this).AddHours(value));

            }

            catch (Exception ex)
            {

                throw ex;

            }

        }

        public PersianDate AddMinutes(int value)
        {

            try
            {

                return (PersianDate) (((DateTime) this).AddMinutes(value));

            }

            catch (Exception ex)
            {

                throw ex;

            }

        }

        public PersianDate AddSeconds(int value)
        {

            try
            {

                return (PersianDate) (((DateTime) this).AddSeconds(value));

            }

            catch (Exception ex)
            {

                throw ex;

            }

        }

        public PersianDate AddDays(int value)
        {

            try
            {

                return (PersianDate) (((DateTime) this).AddDays(value));

            }

            catch (Exception ex)
            {

                throw ex;

            }

        }

        public PersianDate AddMonths(int value)
        {

            try
            {

                return (PersianDate) (((DateTime) this).AddMonths(value));

            }

            catch (Exception ex)
            {

                throw ex;

            }

        }

        public PersianDate AddYears(int value)
        {

            try
            {

                return (PersianDate) (((DateTime) this).AddYears(value));

            }

            catch (Exception ex)
            {

                throw ex;

            }

        }

        public int GetDaysInMonth()
        {

            return calendar.GetDaysInMonth(year, month);

        }



        public int GetDaysInYear()
        {

            return calendar.GetDaysInYear(year);

        }

        #endregion

        #region internal Method(s)

        public PersianDate GetLastSaturday()
        {

            var date = AddDays(-(day + 1));

            return date.AddDays(-(GetWeekNo(date.DayOfWeek) - 1));

        }

        #endregion

        #region Private Method(s)

        private static int GetWeekNo(DayOfWeek week)
        {

            switch (week)
            {

                case DayOfWeek.Saturday:

                    return 1;

                case DayOfWeek.Sunday:

                    return 2;

                case DayOfWeek.Monday:

                    return 3;

                case DayOfWeek.Tuesday:

                    return 4;

                case DayOfWeek.Wednesday:

                    return 5;

                case DayOfWeek.Thursday:

                    return 6;

                case DayOfWeek.Friday:

                    return 7;

                default:
                    throw new ArgumentOutOfRangeException(nameof(week), week, null);
            }
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// for example ToString(yyyy-MM-dd hh:month:ss);
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public string ToString(string format)
        {
            try
            {
                format = Regex.Replace(format, "dd", day.ToString("00"));

                format = Regex.Replace(format, "MM", month.ToString("00"));

                format = Regex.Replace(format, "yyyy", year.ToString("0000"), System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                format = Regex.Replace(format, "DD", weekArray[GetWeekNo(calendar.GetDayOfWeek((DateTime) this)) - 1]);

                format = Regex.Replace(format, "ND", weekArray[GetWeekNo(DayOfWeek) - 1]);

                format = Regex.Replace(format, "NM", monthsArray[month - 1]);

                format = Regex.Replace(format, "hh", hour.ToString("00"));

                format = Regex.Replace(format, "mm", minute.ToString("00"));

                format = Regex.Replace(format, "ss", second.ToString("00"));
            }
            catch
            {
            }
            return format;
        }

        public override string ToString()
        {
            return $"{year:D4}/{month:D2}/{day:D2} {hour:D2}:{minute:D2}:{second:D2}";
        }

        public string ToShortDateString()
        {
            return $"{year:D4}/{month:D2}/{day:D2}";
        }

        public override bool Equals(object obj)
        {
            if (obj is PersianDate)

                return ((PersianDate) obj) == this;

            return false;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        #endregion

        #endregion

        #region Operators

        public static bool operator ==(PersianDate persianDate1, PersianDate persianDate2)
        {
            return (persianDate1.Second == persianDate2.Second && (persianDate1.Minute == persianDate2.Minute) && (persianDate1.Hour == persianDate2.Hour) && (persianDate1.Day == persianDate2.Day) && (persianDate1.Month == persianDate2.Month) && (persianDate1.Year == persianDate2.Year));
        }

        public static bool operator !=(PersianDate persianDate1, PersianDate persianDate2)
        {
            return !(persianDate1.Second == persianDate2.Second && (persianDate1.Minute == persianDate2.Minute) && (persianDate1.Hour == persianDate2.Hour) && (persianDate1.Day == persianDate2.Day) && (persianDate1.Month == persianDate2.Month) && (persianDate1.Year == persianDate2.Year));
        }


        public static bool operator >(PersianDate persianDate1, PersianDate persianDate2)
        {
            return PersianDate.Compare2Date(persianDate1, persianDate2) == 1;
        }

        public static bool operator <(PersianDate persianDate1, PersianDate persianDate2)
        {
            return PersianDate.Compare2Date(persianDate1, persianDate2) == -1;
        }

        public static bool operator >=(PersianDate persianDate1, PersianDate persianDate2)
        {
            var res = PersianDate.Compare2Date(persianDate1, persianDate2);
            return res == 1 || res == 0;
        }

        public static bool operator <=(PersianDate persianDate1, PersianDate persianDate2)
        {
            var res = PersianDate.Compare2Date(persianDate1, persianDate2);
            return res == -1 || res == 0;
        }

        #endregion

        #region Implicit Casting

        public static implicit operator DateTime(PersianDate persianDate)
        {
            return PersianDate.ValidDate(persianDate) ? calendar.ToDateTime(persianDate.Year, persianDate.Month, persianDate.Day, persianDate.Hour, persianDate.Minute, persianDate.Second, 0) : DateTime.MinValue;
        }

        public static implicit operator PersianDate(DateTime date)
        {
            return date.Equals(DateTime.MinValue) ? MinValue : Parse(date);
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (!(obj is PersianDate))

                new ArgumentException("obj is not PersianDate");

            return CompareTo((PersianDate) obj);
        }

        #endregion

        #region IComparable<PersianDate> Members

        public int CompareTo(PersianDate other)
        {
            return PersianDate.Compare2Date(this, other);
        }

        #endregion

        #region IComparer Members

        public int Compare(object x, object y)
        {
            if (!(x is PersianDate))

                throw new ArgumentException("x is not PersianDate");

            if (!(y is PersianDate))

                throw new ArgumentException("y is not PersianDate");

            return PersianDate.Compare2Date((PersianDate) x, (PersianDate) y);
        }

        #endregion

        #region IComparer<PersianDate> Members

        public int Compare(PersianDate x, PersianDate y)
        {
            return PersianDate.Compare2Date(x, y);
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return (PersianDate) this.MemberwiseClone();
        }

        #endregion
    }
}