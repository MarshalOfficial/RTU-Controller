using System;
namespace THTController.Date
{
    class DateFormatException : Exception
    {
        public DateFormatException(string message)
            : base(message)
        {
        }
    }
}