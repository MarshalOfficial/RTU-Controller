using System;
namespace THTController.Date
{
    public static class Methods
    {

        public static string CurrentShamsiDate()
        {

            return PersianDate.Parse(DateTime.Today).ToString().Substring(0, 10);

        }

        public static string CurrentShamsiDateDateWithoutSlash()
        {

            return GetPersianDateWithoutSlash(CurrentShamsiDate());

        }

        public static bool TryParseShamshiToGregorianDate(string strdateShamshi, out DateTime resultStrDate, out Exception exception)
        {

            exception = null;

            resultStrDate = DateTime.Now;

            try
            {

                strdateShamshi = GetPersianDateWithSlash(strdateShamshi);

                resultStrDate = PersianDate.ConvertToGregorian(strdateShamshi);

                return true;

            }

            catch (Exception ex)
            {

                exception = ex;

            }

            return false;

        }

        public static DateTime? ParseShamshiToGregorianDate(string strdateShamshi)
        {

            Exception ex;

            DateTime dateTime;

            var result = TryParseShamshiToGregorianDate(strdateShamshi, out dateTime, out ex);

            if (result)

                return dateTime;

            return null;

        }

        public static DateTime? ParseShamshiToGregorianDate(PersianDate date)
        {

            Exception ex;

            DateTime dateTime;

            var result = TryParseShamshiToGregorianDate(date.ToShortDateString(), out dateTime, out ex);

            if (result)

                return dateTime;

            return null;

        }

        public static PersianDate? ParseGregorianToShamshiDate(int yy, int mm, int dd)
        {

            try
            {

                var dt = new DateTime(yy, mm, dd);

                Exception ex;

                PersianDate dateTime;

                var result = TryParseGregorianToShamshiDate(dt, out dateTime, out ex);

                if (result)

                    return dateTime;

                return null;

            }

            catch (Exception)
            {

                return null;

            }



        }

        public static PersianDate? ParseGregorianToShamshiDate(DateTime dateTime)
        {

            Exception ex;

            PersianDate pd;

            var result = TryParseGregorianToShamshiDate(dateTime, out pd, out ex);

            if (result)

                return pd;

            return null;

        }

        public static PersianDate? ParseGregorianToShamshiDate(string dateTime)
        {

            DateTime dt;

            if (!DateTime.TryParse(dateTime, out dt))

                return null;

            Exception ex;

            PersianDate pd;

            var result = TryParseGregorianToShamshiDate(dt, out pd, out ex);

            if (result)

                return pd;

            return null;

        }

        public static bool TryParseGregorianToShamshiDate(DateTime date, out PersianDate resultStrDate, out Exception exception)
        {

            exception = null;

            resultStrDate = DateTime.Now;

            try
            {

                resultStrDate = PersianDate.Parse(date);

                return true;

            }

            catch (Exception ex)
            {

                exception = ex;

            }

            return false;

        }

        public static string GetPersianDateWithoutSlash(string persianDate)
        {

            persianDate = persianDate.Trim();

            return persianDate.Length == 10 ? persianDate.Replace("/", "") : persianDate;

        }

        public static string GetPersianDateWithSlash(string persianDate)
        {

            if (persianDate == null)

                return "";

            persianDate = persianDate.Trim();

            return string.Format(new DateFormat(), "{0:FD}", persianDate);

        }

        public static string GregorianToShamshiDateWithoutSlash(DateTime dateTime)
        {

            return GetPersianDateWithoutSlash(GregorianToShamshiDateWithSlash(dateTime));

        }

        public static string GregorianToShamshiDateWithSlash(DateTime dateTime)
        {

            Exception ex;

            PersianDate pd;

            var result = TryParseGregorianToShamshiDate(dateTime, out pd, out ex);

            if (result)

                return pd.ToShortDateString();

            return "";

        }

        public static string GregorianToShamshiDateWithTime(DateTime dateTime)
        {

            Exception ex;

            PersianDate pd;

            var result = TryParseGregorianToShamshiDate(dateTime, out pd, out ex);

            if (result)

                return pd.ToString();

            return "";
        }
    }

}
