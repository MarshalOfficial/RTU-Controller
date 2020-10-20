namespace SerialSample.DBLayer
{
    /// <summary>
    /// جدول مجتمع ها
    /// </summary>
    public class SiteEntity
    {
        [SQLite.Net.Attributes.PrimaryKey]
        public int ID { get; set; } 
        public string Name { get; set; }
        public string Address { get; set; } 
        public string X { get; set; }
        public string Y { get; set; }
        public string GoogleAddress { get; set; }
        public int IconID { get; set; } 
    }
}
