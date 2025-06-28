using System;

namespace ConquerWeb.Models
{
    public class PaymentRecord
    {
        public int ID { get; set; }
        public string CharacterName { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public string Email { get; set; }
        public int? VIPDays { get; set; }
        public int? DBScrolls { get; set; }
        public DateTime PayDate { get; set; }
        public string Paymentscol { get; set; } // Ek bilgi veya TxnID
    }
}