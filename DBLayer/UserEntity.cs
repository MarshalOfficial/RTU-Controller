namespace SerialSample.DBLayer
{
    /// <summary>
    /// جدول کاربران
    /// </summary>
    public class UserEntity
    {
        [SQLite.Net.Attributes.PrimaryKey]
        public int ID { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public int RoleID { get; set; }
        public bool IsActive { get; set; }  
    }
}
