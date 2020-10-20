using System;
namespace THTController.Date
{
    public class DateFormat : IFormatProvider, ICustomFormatter
    {

        private const int DateLength = 8;

        private const int MinDateLength = 6;

        #region IFormatProvider Members

        public object GetFormat(Type formatType)
        {

            // Check if the class implements ICustomFormatter

            return formatType == typeof(ICustomFormatter) ? this : null;

        }
        #endregion
        #region ICustomFormatter Members
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg == null)

                return "";

            var currentDate = arg.ToString();

            currentDate = currentDate.Trim();

            if (string.IsNullOrEmpty(currentDate) || string.IsNullOrWhiteSpace(currentDate))

                throw new DateFormatException("تاریخ غیر معتبر");

            currentDate = currentDate.Replace("/", "");

            if (format.ToUpper() != "FD")

                return currentDate;

            if (currentDate.Length == MinDateLength)

                currentDate = string.Format("13{0}", currentDate);

            if (!(currentDate.Length == DateLength || currentDate.Length == MinDateLength))

                throw new DateFormatException("تاریخ غیر معتبر");
            int n;
            if (!int.TryParse(currentDate, out n))
            {
                throw new DateFormatException("تاریخ غیر معتبر");
            }
            return currentDate.Insert(4, "/").Insert(7, "/");
        }
        #endregion
    }
}