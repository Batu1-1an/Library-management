using System.Collections.Generic;

namespace LibraryManagementSystem.Models
{
    public class BookSearchViewModel
    {
        public string SearchTerm { get; set; }
        public Genre? Genre { get; set; }
        public string Publisher { get; set; }
        public bool? IsAvailable { get; set; }
        public IEnumerable<Book> Books { get; set; } = new List<Book>();
    }
} 