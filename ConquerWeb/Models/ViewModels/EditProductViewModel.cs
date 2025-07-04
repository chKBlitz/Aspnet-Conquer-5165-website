using System.ComponentModel.DataAnnotations;

namespace ConquerWeb.Models.ViewModels
{
    public class EditProductViewModel
    {
        public int ProductId { get; set; } // Ürün düzenleniyorsa ID'si, ekleniyorsa 0 olacak

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Product name must be between 3 and 100 characters.")]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 100000.00, ErrorMessage = "Price must be between 0.01 and 100000.00.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Product Price")]
        public decimal ProductPrice { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        [StringLength(10, ErrorMessage = "Currency cannot exceed 10 characters.")]
        [Display(Name = "Currency (e.g., USD, TL)")]
        public string ProductCurrency { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        [Display(Name = "Product Description")]
        public string ProductDesc { get; set; }

        [Required(ErrorMessage = "DBScrolls value is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "DBScrolls must be a non-negative number.")]
        [Display(Name = "DBScrolls Amount")]
        public int DBScrolls { get; set; }

        // [Url(ErrorMessage = "Invalid URL format.")]  <--- BU SATIRI SİLİN
        [Required(ErrorMessage = "Product image filename is required.")] // Mesajı güncelledik
        [StringLength(255, ErrorMessage = "Image filename cannot exceed 255 characters.")] // Mesajı güncelledik
        [Display(Name = "Product Image Filename")] // Etiketi güncelledik
        public string ProductImage { get; set; }
    }
}