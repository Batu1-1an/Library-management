using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ReservationController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reservation/MyReservations
        public async Task<IActionResult> MyReservations()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var reservations = await _context.BookReservations
                .Include(r => r.Book)
                .Where(r => r.UserId == userId && r.IsActive)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            return View(reservations);
        }

        // POST: Reservation/Reserve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reserve(int id)
        {
            var book = await _context.Books
                .Include(b => b.Borrowings)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Check if user already has an active reservation for this book
            var existingReservation = await _context.BookReservations
                .AnyAsync(r => r.BookId == id && r.UserId == userId && r.IsActive);

            if (existingReservation)
            {
                TempData["Error"] = "You already have an active reservation for this book.";
                return RedirectToAction("Details", "Book", new { id });
            }

            // Create new reservation
            var reservation = new BookReservation
            {
                BookId = id,
                UserId = userId,
                ReservationDate = DateTime.Now,
                IsActive = true
            };

            _context.Add(reservation);
            await _context.SaveChangesAsync();

            // TODO: Send email notification
            // This would be implemented using an email service

            TempData["Success"] = "Book reserved successfully. You will be notified when it becomes available.";
            return RedirectToAction("Details", "Book", new { id });
        }

        // POST: Reservation/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var reservation = await _context.BookReservations
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (reservation == null)
            {
                return NotFound();
            }

            reservation.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Reservation cancelled successfully.";
            return RedirectToAction(nameof(MyReservations));
        }
    }
} 