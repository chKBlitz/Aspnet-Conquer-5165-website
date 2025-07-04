using System.ComponentModel.DataAnnotations;

namespace ConquerWeb.Models.ViewModels
{
    public class EditUserViewModel
    {
        public int UID { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 50 characters.")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Username can only contain letters and numbers.")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(100, ErrorMessage = "Email address cannot exceed 100 characters.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [Range(0, 3, ErrorMessage = "Status must be between 0 and 3 (0=Normal, 3=Admin).")] // Örn: 0 normal, 3 admin
        [Display(Name = "Account Status")]
        public int Status { get; set; }

        // Şifre değiştirme Admin panelinden yapılacaksa buraya eklenebilir, veya ayrı bir action olabilir.
        // [DataType(DataType.Password)]
        // [Display(Name = "New Password (optional)")]
        // public string NewPassword { get; set; }
    }
}