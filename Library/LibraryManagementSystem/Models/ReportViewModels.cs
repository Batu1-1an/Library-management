using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models
{
    public class GenreStatistics
    {
        public string Genre { get; set; }
        public int Count { get; set; }
    }

    public class PopularBookViewModel
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public int BorrowCount { get; set; }
    }

    public class OverdueBookViewModel
    {
        public string BookTitle { get; set; }
        public string MemberName { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
    }
} 