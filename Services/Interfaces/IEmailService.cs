namespace AttendanceManagementSystem.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendInvitationEmailAsync(string toEmail, string toName, string token, string roleName, string inviterName);
        Task<bool> SendWelcomeEmailAsync(string toEmail, string userName, string roleName);
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
    }
}