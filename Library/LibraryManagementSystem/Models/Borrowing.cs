using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Borrowing
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int BookId { get; set; }
        public virtual Book Book { get; set; }

        [Required]
        public DateTime BorrowDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }
} 