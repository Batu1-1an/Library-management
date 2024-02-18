using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var books = _context.Books.ToList();
            return View(books);
        }

        public IActionResult Details(int id)
        {
            var book = _context.Books
                .Include(b => b.Borrowings)
                .ThenInclude(b => b.User)
                .FirstOrDefault(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new Book());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(Book book)
        {
            if (ModelState.IsValid)
            {
                // Check if ISBN already exists
                if (_context.Books.Any(b => b.ISBN == book.ISBN))
                {
                    ModelState.AddModelError("ISBN", "Bu ISBN numarası zaten kullanılıyor.");
                    return View(book);
                }

                try
                {
                    _context.Add(book);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Kitap başarıyla eklendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Kitap eklenirken bir hata oluştu. Lütfen tüm alanları kontrol edip tekrar deneyin.");
                    return View(book);
                }
            }
            return View(book);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var book = _context.Books.Find(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id, Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    book.UpdatedAt = DateTime.Now;
                    _context.Update(book);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var book = _context.Books.Find(id);
            if (book == null)
            {
                return NotFound();
            }

            if (book.IsAvailable)
            {
                _context.Books.Remove(book);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Cannot delete book while it is borrowed.");
            return RedirectToAction(nameof(Details), new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Borrow(int id)
        {
            var book = _context.Books.Find(id);
            if (book == null)
            {
                return NotFound();
            }

            if (book.AvailableCopies <= 0)
            {
                ModelState.AddModelError("", "No copies available for borrowing.");
                return RedirectToAction(nameof(Details), new { id = id });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var borrowing = new Borrowing
            {
                BookId = id,
                UserId = userId,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14)
            };

            book.AvailableCopies--;
            _context.Borrowings.Add(borrowing);
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Return(int id)
        {
            var book = _context.Books.Find(id);
            if (book == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var borrowing = _context.Borrowings
                .FirstOrDefault(b => b.BookId == id && b.UserId == userId && !b.ReturnDate.HasValue);

            if (borrowing == null)
            {
                ModelState.AddModelError("", "No active borrowing found for this book.");
                return RedirectToAction(nameof(Details), new { id = id });
            }

            borrowing.ReturnDate = DateTime.Now;
            book.AvailableCopies++;
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = id });
        }

        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Search(BookSearchViewModel model)
        {
            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(model.SearchTerm))
            {
                query = query.Where(b =>
                    b.Title.Contains(model.SearchTerm) ||
                    b.Author.Contains(model.SearchTerm) ||
                    b.ISBN.Contains(model.SearchTerm));
            }

            if (model.Genre.HasValue)
            {
                query = query.Where(b => b.Genre == model.Genre.Value);
            }

            if (!string.IsNullOrWhiteSpace(model.Publisher))
            {
                query = query.Where(b => b.Publisher.Contains(model.Publisher));
            }

            if (model.IsAvailable.HasValue)
            {
                query = query.Where(b => b.IsAvailable == model.IsAvailable.Value);
            }

            model.Books = query.ToList();
            return View(model);
        }

        [Authorize]
        public IActionResult MyBorrowings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var borrowings = _context.Borrowings
                .Include(b => b.Book)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BorrowDate)
                .ToList();

            return View(borrowings);
        }

        [HttpGet]
        public IActionResult GetAvailableBooks()
        {
            var books = _context.Books
                .Select(b => new
                {
                    id = b.Id,
                    title = b.Title,
                    author = b.Author,
                    genre = b.Genre.ToString(),
                    availableCopies = b.AvailableCopies
                })
                .ToList();

            return Json(books);
        }

        [HttpGet]
        public IActionResult GetCurrentBorrowings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var borrowings = _context.Borrowings
                .Include(b => b.Book)
                .Where(b => b.UserId == userId && !b.ReturnDate.HasValue)
                .Select(b => new
                {
                    bookId = b.BookId,
                    bookTitle = b.Book.Title,
                    borrowDate = b.BorrowDate,
                    dueDate = b.DueDate,
                    isOverdue = b.DueDate < DateTime.Now
                })
                .ToList();

            return Json(borrowings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult ImportFromExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length <= 0)
            {
                TempData["Error"] = "Please select an Excel file to import.";
                return RedirectToAction(nameof(Create));
            }

            if (!Path.GetExtension(excelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Please upload a valid Excel file (.xlsx).";
                return RedirectToAction(nameof(Create));
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    excelFile.CopyTo(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;
                        var importedBooks = new List<Book>();

                        // Skip header row
                        for (int row = 2; row <= rowCount; row++)
                        {
                            var book = new Book
                            {
                                Title = worksheet.Cells[row, 1].Value?.ToString(),
                                Author = worksheet.Cells[row, 2].Value?.ToString(),
                                ISBN = worksheet.Cells[row, 3].Value?.ToString(),
                                Genre = Enum.TryParse<Genre>(worksheet.Cells[row, 4].Value?.ToString(), out var genre) ? genre : Genre.Other,
                                Publisher = worksheet.Cells[row, 5].Value?.ToString(),
                                PublicationDate = DateTime.TryParse(worksheet.Cells[row, 6].Value?.ToString(), out var pubDate) ? pubDate : DateTime.Now,
                                PageCount = int.TryParse(worksheet.Cells[row, 7].Value?.ToString(), out var pageCount) ? pageCount : 0,
                                CopyCount = int.TryParse(worksheet.Cells[row, 8].Value?.ToString(), out var copyCount) ? copyCount : 1,
                                Summary = worksheet.Cells[row, 9].Value?.ToString(),
                                ImageUrl = worksheet.Cells[row, 10].Value?.ToString(),
                                CreatedAt = DateTime.Now,
                                AvailableCopies = int.TryParse(worksheet.Cells[row, 8].Value?.ToString(), out var availableCopies) ? availableCopies : 1
                            };

                            if (!string.IsNullOrWhiteSpace(book.Title) && !string.IsNullOrWhiteSpace(book.Author))
                            {
                                importedBooks.Add(book);
                            }
                        }

                        if (importedBooks.Any())
                        {
                            _context.Books.AddRange(importedBooks);
                            _context.SaveChanges();
                            TempData["Success"] = $"Successfully imported {importedBooks.Count} books.";
                        }
                        else
                        {
                            TempData["Warning"] = "No valid books found in the Excel file.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error importing books: {ex.Message}";
            }

            return RedirectToAction(nameof(Create));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
} 