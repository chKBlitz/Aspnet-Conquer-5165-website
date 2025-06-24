namespace ConquerWeb.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductCurrency { get; set; }
        public string ProductDesc { get; set; }
        public int DBScrolls { get; set; }
        public string ProductImage { get; set; }
    }
}