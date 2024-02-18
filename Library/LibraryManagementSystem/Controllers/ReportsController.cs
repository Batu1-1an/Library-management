using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using OfficeOpenXml;
using System.IO;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var model = new DashboardViewModel
            {
                TotalMembers = _context.Users.Count(),
                TotalBooks = _context.Books.Count(),
                ActiveBorrowings = _context.Borrowings.Count(b => !b.ReturnDate.HasValue),
                TotalPenalties = CalculateTotalPenalties(),
                RecentActivities = GetRecentActivities()
            };

            return View(model);
        }

        public IActionResult UsersList()
        {
            var users = _context.Users.Select(u => new UserListViewModel
            {
                Id = u.Id,
                Name = $"{u.FirstName} {u.LastName}",
                Email = u.Email,
                IsActive = u.IsActive,
                IsAdmin = u.IsAdmin,
                RegisterDate = u.RegisterDate
            }).ToList();

            return View(users);
        }

        public IActionResult UserBorrowing(int userId)
        {
            var borrowings = _context.Borrowings
                .Include(b => b.Book)
                .Where(b => b.UserId == userId)
                .Select(b => new BorrowHistoryViewModel
                {
                    BookTitle = b.Book.Title,
                    BorrowDate = b.BorrowDate,
                    DueDate = b.DueDate,
                    ReturnDate = b.ReturnDate,
                    IsOverdue = !b.ReturnDate.HasValue && b.DueDate < DateTime.Now
                })
                .OrderByDescending(b => b.BorrowDate)
                .ToList();

            return View(borrowings);
        }

        public IActionResult BookStatus()
        {
            var books = _context.Books
                .Include(b => b.Borrowings)
                .ThenInclude(b => b.User)
                .Select(b => new BookStatusViewModel
                {
                    Title = b.Title,
                    Author = b.Author,
                    ISBN = b.ISBN,
                    IsAvailable = !b.Borrowings.Any(br => !br.ReturnDate.HasValue),
                    IsOverdue = b.Borrowings.Any(br => !br.ReturnDate.HasValue && br.DueDate < DateTime.Now),
                    CurrentBorrowerName = b.Borrowings
                        .Where(br => !br.ReturnDate.HasValue)
                        .Select(br => $"{br.User.FirstName} {br.User.LastName}")
                        .FirstOrDefault()
                })
                .ToList();

            return View(books);
        }

        public IActionResult ActiveBorrowings()
        {
            var activeBorrowings = _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.User)
                .Where(b => !b.ReturnDate.HasValue)
                .Select(b => new BorrowingDetailsViewModel
                {
                    BookTitle = b.Book.Title,
                    BookAuthor = b.Book.Author,
                    BorrowDate = b.BorrowDate,
                    DueDate = b.DueDate,
                    ReturnDate = b.ReturnDate,
                    IsOverdue = b.DueDate < DateTime.Now,
                    Status = b.DueDate < DateTime.Now ? "Overdue" : "Active"
                })
                .OrderBy(b => b.DueDate)
                .ToList();

            return View(activeBorrowings);
        }

        public IActionResult Penalties()
        {
            var overdueBorrowings = _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.User)
                .Where(b => !b.ReturnDate.HasValue && b.DueDate < DateTime.Now)
                .AsEnumerable()
                .Select(b => new
                {
                    BookTitle = b.Book.Title,
                    UserName = $"{b.User.FirstName} {b.User.LastName}",
                    DueDate = b.DueDate,
                    DaysOverdue = (int)(DateTime.Now - b.DueDate).TotalDays,
                    PenaltyAmount = (decimal)(DateTime.Now - b.DueDate).TotalDays * 0.5m
                })
                .OrderByDescending(b => b.DaysOverdue)
                .ToList();

            ViewBag.TotalPenalties = overdueBorrowings.Sum(b => b.PenaltyAmount);
            return View(overdueBorrowings);
        }

        public IActionResult ExportUsersList()
        {
            var users = _context.Users.Select(u => new UserListViewModel
            {
                Id = u.Id,
                Name = $"{u.FirstName} {u.LastName}",
                Email = u.Email,
                IsActive = u.IsActive,
                IsAdmin = u.IsAdmin,
                RegisterDate = u.RegisterDate
            }).ToList();

            var csvContent = "ID,Name,Email,Status,Role,Register Date\n";
            foreach (var user in users)
            {
                csvContent += $"{user.Id},{user.Name},{user.Email},{(user.IsActive ? "Active" : "Inactive")},{(user.IsAdmin ? "Admin" : "User")},{user.RegisterDate:dd.MM.yyyy}\n";
            }

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
            return File(bytes, "text/csv", $"users_list_{DateTime.Now:yyyyMMdd}.csv");
        }

        private decimal CalculateTotalPenalties()
        {
            var overdueBorrowings = _context.Borrowings
                .Where(b => !b.ReturnDate.HasValue && b.DueDate < DateTime.Now)
                .AsEnumerable()
                .Select(b => new
                {
                    DaysOverdue = (decimal)(DateTime.Now - b.DueDate).TotalDays
                })
                .ToList();

            return overdueBorrowings.Sum(b => b.DaysOverdue * 0.5m);
        }

        private List<RecentActivityViewModel> GetRecentActivities()
        {
            var activities = new List<RecentActivityViewModel>();

            var recentBorrowings = _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.User)
                .Where(b => !b.ReturnDate.HasValue)
                .OrderByDescending(b => b.BorrowDate)
                .Take(5)
                .Select(b => new RecentActivityViewModel
                {
                    Description = $"Borrowed {b.Book.Title}",
                    Name = $"{b.User.FirstName} {b.User.LastName}",
                    Date = b.BorrowDate
                })
                .ToList();

            activities.AddRange(recentBorrowings);

            var recentReturns = _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.User)
                .Where(b => b.ReturnDate.HasValue)
                .OrderByDescending(b => b.ReturnDate)
                .Take(5)
                .Select(b => new RecentActivityViewModel
                {
                    Description = $"Returned {b.Book.Title}",
                    Name = $"{b.User.FirstName} {b.User.LastName}",
                    Date = b.ReturnDate.Value
                })
                .ToList();

            activities.AddRange(recentReturns);

            return activities
                .OrderByDescending(a => a.Date)
                .Take(10)
                .ToList();
        }

        [HttpGet]
        public IActionResult EditUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsActive = user.IsActive,
                IsAdmin = user.IsAdmin
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.Find(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.IsActive = model.IsActive;
                user.IsAdmin = model.IsAdmin;

                _context.Update(user);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction(nameof(UsersList));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            // Kullanıcının ödünç aldığı kitapları kontrol et
            var hasBorrowings = _context.Borrowings
                .Any(b => b.UserId == id && !b.ReturnDate.HasValue);

            if (hasBorrowings)
            {
                TempData["ErrorMessage"] = "Cannot delete user with active borrowings.";
                return RedirectToAction(nameof(UsersList));
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "User deleted successfully.";
            return RedirectToAction(nameof(UsersList));
        }

        public IActionResult ViewBorrowings(int id)
        {
            var borrowings = _context.Borrowings
                .Include(b => b.Book)
                .Where(b => b.UserId == id)
                .Select(b => new BorrowingDetailsViewModel
                {
                    BookTitle = b.Book.Title,
                    BookAuthor = b.Book.Author,
                    BorrowDate = b.BorrowDate,
                    DueDate = b.DueDate,
                    ReturnDate = b.ReturnDate,
                    IsOverdue = !b.ReturnDate.HasValue && b.DueDate < DateTime.Now,
                    Status = b.ReturnDate.HasValue ? "Returned" : (b.DueDate < DateTime.Now ? "Overdue" : "Active")
                })
                .OrderByDescending(b => b.BorrowDate)
                .ToList();

            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.UserName = $"{user.FirstName} {user.LastName}";
            ViewBag.UserId = id;
            return View(borrowings);
        }

        [HttpGet]
        public IActionResult Analytics()
        {
            return View();
        }

        public IActionResult ExportBorrowings(int? userId = null)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            var borrowings = _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.User)
                .AsQueryable();

            if (userId.HasValue)
            {
                borrowings = borrowings.Where(b => b.UserId == userId);
            }

            var borrowingsList = borrowings.Select(b => new BorrowingDetailsViewModel
            {
                BookTitle = b.Book.Title,
                BookAuthor = b.Book.Author,
                UserName = $"{b.User.FirstName} {b.User.LastName}",
                BorrowDate = b.BorrowDate,
                DueDate = b.DueDate,
                ReturnDate = b.ReturnDate,
                IsOverdue = !b.ReturnDate.HasValue && b.DueDate < DateTime.Now
            }).ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Borrowings");
                
                // Add headers
                worksheet.Cells[1, 1].Value = "Book Title";
                worksheet.Cells[1, 2].Value = "Author";
                worksheet.Cells[1, 3].Value = "User";
                worksheet.Cells[1, 4].Value = "Borrow Date";
                worksheet.Cells[1, 5].Value = "Due Date";
                worksheet.Cells[1, 6].Value = "Return Date";
                worksheet.Cells[1, 7].Value = "Status";

                // Add data
                int row = 2;
                foreach (var item in borrowingsList)
                {
                    worksheet.Cells[row, 1].Value = item.BookTitle;
                    worksheet.Cells[row, 2].Value = item.BookAuthor;
                    worksheet.Cells[row, 3].Value = item.UserName;
                    worksheet.Cells[row, 4].Value = item.BorrowDate.ToString("dd.MM.yyyy");
                    worksheet.Cells[row, 5].Value = item.DueDate.ToString("dd.MM.yyyy");
                    worksheet.Cells[row, 6].Value = item.ReturnDate?.ToString("dd.MM.yyyy") ?? "-";
                    worksheet.Cells[row, 7].Value = item.IsOverdue ? "Overdue" : (item.ReturnDate.HasValue ? "Returned" : "Active");
                    row++;
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Generate file name
                string fileName = userId.HasValue 
                    ? $"borrowings_user_{userId}_{DateTime.Now:yyyyMMdd}.xlsx"
                    : $"all_borrowings_{DateTime.Now:yyyyMMdd}.xlsx";

                return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }
} 