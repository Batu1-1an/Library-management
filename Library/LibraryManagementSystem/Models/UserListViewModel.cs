using System;

namespace LibraryManagementSystem.Models
{
    public class UserListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime RegisterDate { get; set; }
    }
} 