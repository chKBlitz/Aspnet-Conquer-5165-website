using ConquerWeb.Models;
using System.Collections.Generic;

namespace ConquerWeb.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public Account Account { get; set; }
        public Character Character { get; set; }
        public List<PaymentRecord> PaymentHistory { get; set; } // Yeni eklendi
    }
}