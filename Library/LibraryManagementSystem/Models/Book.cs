using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public enum Genre
    {
        Fiction,
        NonFiction,
        Mystery,
        Romance,
        ScienceFiction,
        Fantasy,
        Biography,
        History,
        Science,
        Technology,
        Education,
        Literature,
        Poetry,
        Drama,
        Horror,
        Thriller,
        Adventure,
        Children,
        YoungAdult,
        Comics,
        Manga,
        Cooking,
        Art,
        Music,
        Sports,
        Travel,
        Health,
        SelfHelp,
        Business,
        Economics,
        Politics,
        Philosophy,
        Religion,
        Language,
        Reference,
        Other
    }

    public class Book
    {
        public Book()
        {
            Borrowings = new List<Borrowing>();
            Tags = new HashSet<Tag>();
            CreatedAt = DateTime.Now;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Kitap adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Kitap adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Kitap Adı")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Yazar adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Yazar adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Yazar")]
        public string Author { get; set; }

        [Required(ErrorMessage = "ISBN numarası zorunludur.")]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN numarası 10 veya 13 karakter olmalıdır.")]
        [RegularExpression(@"^[0-9-]*$", ErrorMessage = "ISBN sadece rakam ve tire içerebilir.")]
        [Display(Name = "ISBN")]
        public string ISBN { get; set; }

        [Required(ErrorMessage = "Yayınevi zorunludur.")]
        [StringLength(100, ErrorMessage = "Yayınevi adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Yayınevi")]
        public string Publisher { get; set; }

        [Required(ErrorMessage = "Kitap türü seçilmelidir.")]
        [Display(Name = "Tür")]
        public Genre Genre { get; set; }

        [Required]
        [StringLength(100)]
        public string Summary { get; set; }

        [Required]
        [Display(Name = "Publication Date")]
        public DateTime PublicationDate { get; set; }

        [Required]
        [Range(1, 10000)]
        [Display(Name = "Page Count")]
        public int PageCount { get; set; }

        [Required(ErrorMessage = "Kopya sayısı girilmelidir.")]
        [Range(1, 1000, ErrorMessage = "Kopya sayısı 1 ile 1000 arasında olmalıdır.")]
        [Display(Name = "Kopya Sayısı")]
        public int CopyCount { get; set; }

        [Display(Name = "Mevcut Kopya")]
        public int AvailableCopies { get; set; }

        [Display(Name = "Is Available")]
        public bool IsAvailable => AvailableCopies > 0;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [StringLength(500)]
        [Display(Name = "Cover Image URL")]
        public string ImageUrl { get; set; }

        public virtual ICollection<Borrowing> Borrowings { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
    }
} 