using System.ComponentModel.DataAnnotations;
using System;

namespace ConquerWeb.Models.ViewModels
{
    public class EditNewsViewModel
    {
        [Required]
        public int Id { get; set; } // Düzenlenecek haberin ID'si

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters.")]
        [Display(Name = "News Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        [MinLength(10, ErrorMessage = "Content must be at least 10 characters.")]
        [Display(Name = "News Content")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Author is required.")]
        [StringLength(50, ErrorMessage = "Author name cannot exceed 50 characters.")]
        [Display(Name = "Author")]
        public string Author { get; set; }

        [Required(ErrorMessage = "Publish Date is required.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Publish Date")]
        public DateTime Publish_Date { get; set; } = DateTime.Now; // Varsayılan olarak şimdiki zaman
    }
}