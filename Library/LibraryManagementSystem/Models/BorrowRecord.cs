using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class BorrowRecord
    {
        public int Id { get; set; }

        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public int MemberId { get; set; }
        public Member Member { get; set; } = null!;

        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public bool IsReturned { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
} 