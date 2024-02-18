using System;

namespace LibraryManagementSystem.Models
{
    public class BookStatusViewModel
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsOverdue { get; set; }
        public string CurrentBorrowerName { get; set; }
        public DateTime? DueDate { get; set; }
        public int TotalBorrowings { get; set; }
    }
} 