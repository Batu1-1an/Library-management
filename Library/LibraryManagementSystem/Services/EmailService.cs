using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;

namespace LibraryManagementSystem.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendBorrowingDeadlineReminderAsync(string to, string bookTitle, DateTime dueDate, string reminderType);
        Task SendPenaltyNotificationAsync(string to, string bookTitle, int daysLate, decimal penaltyAmount);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpServer = _configuration["EmailSettings:SmtpServer"];
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            _smtpUsername = _configuration["EmailSettings:Username"];
            _smtpPassword = _configuration["EmailSettings:Password"];
            _fromEmail = _configuration["EmailSettings:FromEmail"];
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(to));

            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }

        public async Task SendBorrowingDeadlineReminderAsync(string to, string bookTitle, DateTime dueDate, string reminderType)
        {
            var subject = $"Library Book Return Reminder - {reminderType} Notice";
            var urgencyMessage = reminderType switch
            {
                "One Week" => "You have one week remaining to return your book.",
                "Three Days" => "Please note that your book is due in three days.",
                "One Day" => "URGENT: Your book is due tomorrow!",
                _ => "Your book's due date is approaching."
            };

            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2c3e50;'>Book Return Reminder</h2>
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px;'>
                        <p>Dear Library Member,</p>
                        <p><strong style='color: #e74c3c;'>{urgencyMessage}</strong></p>
                        <div style='background-color: #ffffff; padding: 15px; border-left: 4px solid #3498db; margin: 15px 0;'>
                            <p><strong>Book Title:</strong> {bookTitle}</p>
                            <p><strong>Due Date:</strong> {dueDate:dddd, MMMM d, yyyy}</p>
                        </div>
                        <p>Please ensure you return the book by the due date to avoid any late fees.</p>
                        <p>Our progressive late fee structure:</p>
                        <ul>
                            <li>First week: $0.50 per day</li>
                            <li>Second week: $1.00 per day</li>
                            <li>After two weeks: $2.00 per day</li>
                        </ul>
                        <p>Thank you for using our library services!</p>
                        <br>
                        <p>Best regards,</p>
                        <p>Library Management System</p>
                    </div>
                    <p style='font-size: 12px; color: #7f8c8d; margin-top: 20px;'>
                        This is an automated message. Please do not reply to this email.
                    </p>
                </div>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPenaltyNotificationAsync(string to, string bookTitle, int daysLate, decimal penaltyAmount)
        {
            var subject = "Library Book Late Return Penalty Notice";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2c3e50;'>Late Return Penalty Notice</h2>
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px;'>
                        <p>Dear Library Member,</p>
                        <div style='background-color: #ffffff; padding: 15px; border-left: 4px solid #e74c3c; margin: 15px 0;'>
                            <p><strong>Book Title:</strong> {bookTitle}</p>
                            <p><strong>Days Late:</strong> {daysLate}</p>
                            <p><strong>Penalty Amount:</strong> ${penaltyAmount:F2}</p>
                        </div>
                        <p>This penalty has been calculated based on our progressive fee structure:</p>
                        <ul>
                            <li>First week: $0.50 per day</li>
                            <li>Second week: $1.00 per day</li>
                            <li>After two weeks: $2.00 per day</li>
                        </ul>
                        <p>Please settle this penalty at your earliest convenience to maintain your borrowing privileges.</p>
                        <p>To avoid future penalties, please return your books by their due dates.</p>
                        <br>
                        <p>Best regards,</p>
                        <p>Library Management System</p>
                    </div>
                    <p style='font-size: 12px; color: #7f8c8d; margin-top: 20px;'>
                        This is an automated message. Please do not reply to this email.
                    </p>
                </div>";

            await SendEmailAsync(to, subject, body);
        }
    }
} 