using System;
namespace THTController.DBLayer
{
    /// <summary>
    /// جدول آخرین وضعیت جی پی آی اوهای کنترلر
    /// یعنی در این جدول ذخیره می شود آخرین وضعیت جی پی ای اوها که وقتی خاست رفرش کنه وضعیتشونو بدونه چی بوده از قبل
    /// </summary>
    public class GPIOHistoryEntity
    {
        [SQLite.Net.Attributes.PrimaryKey]
        public int GPIO { get; set; }
        public int GPIOValue { get; set; }
        public DateTime SaveTime { get; set; }
    }
}
