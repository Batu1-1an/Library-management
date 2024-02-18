using System;

namespace LibraryManagementSystem.Models
{
    public class BorrowHistoryViewModel
    {
        public string BookTitle { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsOverdue { get; set; }
    }
} 