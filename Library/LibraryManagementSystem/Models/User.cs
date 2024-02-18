using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Models
{
    public class User : IdentityUser<int>
    {
        public User()
        {
            RegisterDate = DateTime.UtcNow;
            IsActive = true;
            IsAdmin = false;
            Borrowings = new List<Borrowing>();
        }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        public bool IsActive { get; set; }

        public DateTime RegisterDate { get; set; }

        public bool IsAdmin { get; set; }

        public virtual ICollection<Borrowing> Borrowings { get; set; }
    }
} 