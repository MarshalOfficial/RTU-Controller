using SerialSample.DBLayer;
using System;
using System.Diagnostics;

namespace THTController
{
    /// <summary>
    /// این کلاس جهت لاگ کردن خطاهای کش شده در برنامه کنترلر ، روی دیتابیس اسکیوال سرور مرکز می باشد 
    /// </summary>
    public class ErrorLog
    {
        /// <summary>
        /// ای پی اسکیوال سرور مرکز
        /// </summary>
        private string _sqlip;

        /// <summary>
        /// در سازنده این کلاس ای پی اسکیوال سرور رو پاس میدیم بهش که بدونه کجا میخاد وصل شه
        /// </summary>
        /// <param name="sqlip"></param>
        public ErrorLog(string sqlip)
        {
            _sqlip = sqlip;
        }

        /// <summary>
        /// تابع اصلی لاگ کردن خطا
        /// </summary>
        /// <param name="ex">آبجکت اکسپشن کش شده در برنامه</param>
        /// <param name="memo">توضیحات اضافی که کنار خطا میخایم لاگ بشه</param>
        public void SaveLog(Exception ex,string memo="")
        {
            //مسیج های خطا رو سرجمع میکنیم توی متغیر به نام ارور
            // هر ابجکت اکسپشن ممکنه اکسپشن داخلی هم داشته باشه برای همین تا دو مرحله اکسپشن های داخلی رو هم چک میکنیم
            var Error = ex.Message + "#" + (ex.InnerException?.Message ?? "") + "#" + (ex.InnerException?.InnerException?.Message ?? "");
            //استک تریس های اکسپشن ها رو هم در میاریم ، استک تریس یعنی محل وقوع خطا در سورس کد کجا بوده
            var stacktrace = ex.StackTrace + "#" + (ex.InnerException?.StackTrace ?? "") + "#" + (ex.InnerException?.InnerException?.StackTrace ?? "");
            //خطاها رو توی اوت پوت ویژوال استودیو هم لاگ میکنیم
            // این لاگ موقعی که روی رسپری برنامه دیفالت بشه هیچ تاثیری نداره فقط موقعی که از توی ویژوال استودیو برنامه رو اجرا میکنیم روی رسپری توی اوت پوت ویژوال استودیو چاپ میشه
            Debug.WriteLine(DateTime.UtcNow + "#" +  Error);
            Debug.WriteLine(DateTime.UtcNow + "#" + stacktrace);

            // اگر ای پی اسکیوال پاس داده شده بود و اتصال به اسکیوال سرور مرکز هم برقرار بود یعنی مشکل شبکه وجود نداشت
            if (!string.IsNullOrWhiteSpace(_sqlip) && Extension.IsSqlServerAvailable(_sqlip))
            {
                var localip = Extension.LocalIPAddress;
                var stack = ((!string.IsNullOrWhiteSpace(memo)) ? stacktrace + "*" + memo : stacktrace);
                //اینزرت شدن خطا در اسکیوال سرور مرکز
                new SqlServerRepository(_sqlip).InsertErrorOnSqlServer(localip, Error, stack);//ehsan//Reza
            }
        }
    }
}