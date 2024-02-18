using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Admin")]
        public bool IsAdmin { get; set; }
    }
} 