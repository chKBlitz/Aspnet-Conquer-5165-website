using System;

namespace ConquerWeb.Models
{
    public class Account
    {
        public int UID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int Status { get; set; }
        public string IP { get; set; }
        public string SecretID { get; set; }
        public DateTime Creation_Date { get; set; }
        public DateTime? Last_Login { get; set; }
        public string Reset_Token { get; set; }
        public DateTime? Reset_Token_Expiry { get; set; }
    }
}