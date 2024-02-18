using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class BookReservation
    {
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime ReservationDate { get; set; }

        public DateTime? NotificationSentDate { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public virtual Book Book { get; set; }
        public virtual User User { get; set; }
    }
} 