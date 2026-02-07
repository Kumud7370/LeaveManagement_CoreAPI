using AttendanceManagementSystem.Models.Settings;
using AttendanceManagementSystem.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace AttendanceManagementSystem.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendInvitationEmailAsync(string toEmail, string toName, string token, string roleName, string inviterName)
        {
            var subject = "You're Invited to Join Attendance Management System";

            // Update this URL when frontend is ready
            var acceptUrl = $"http://localhost:3000/accept-invitation?token={token}";

            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Invitation to Join</h1>
        </div>
        <div class='content'>
            <p>Hello {toName},</p>
            <p><strong>{inviterName}</strong> has invited you to join the Attendance Management System as a <strong>{roleName}</strong>.</p>
            <p>Click the button below to accept the invitation and create your account:</p>
            <p style='text-align: center;'>
                <a href='{acceptUrl}' class='button'>Accept Invitation</a>
            </p>
            <p>Or copy and paste this link into your browser:</p>
            <p style='word-break: break-all; color: #4CAF50;'>{acceptUrl}</p>
            <p><strong>Note:</strong> This invitation will expire in 7 days.</p>
        </div>
        <div class='footer'>
            <p>© 2024 Attendance Management System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, body);
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string userName, string roleName)
        {
            var subject = "Welcome to Attendance Management System";

            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome!</h1>
        </div>
        <div class='content'>
            <p>Hello <strong>{userName}</strong>,</p>
            <p>Your account has been successfully created in the Attendance Management System.</p>
            <p><strong>Role:</strong> {roleName}</p>
            <p>You can now log in using your username and password.</p>
            <p>If you have any questions, please contact your administrator.</p>
        </div>
        <div class='footer'>
            <p>© 2024 Attendance Management System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, body);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                    EnableSsl = _emailSettings.EnableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}");
                return false;
            }
        }
    }
}