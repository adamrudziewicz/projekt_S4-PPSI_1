namespace OgloszeniaSytem.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendWelcomeEmailAsync(string to, string userName);
        Task SendOgloszenieNotificationAsync(string to, string ogloszenieTitle);
    }
}