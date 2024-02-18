using System;

namespace LibraryManagementSystem.Models
{
    public class BorrowingDetailsViewModel
    {
        public string BookTitle { get; set; }
        public string BookAuthor { get; set; }
        public string UserName { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsOverdue { get; set; }
        public string Status { get; set; }
    }
} 