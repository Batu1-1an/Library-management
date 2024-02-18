using System.Collections.Generic;

namespace LibraryManagementSystem.Models
{
    public class DashboardViewModel
    {
        public int TotalMembers { get; set; }
        public int TotalBooks { get; set; }
        public int ActiveBorrowings { get; set; }
        public decimal TotalPenalties { get; set; }
        public List<RecentActivityViewModel> RecentActivities { get; set; }
    }
} 