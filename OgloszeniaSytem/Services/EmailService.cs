using System.Net;
using System.Net.Mail;

namespace OgloszeniaSytem.Services
{
    // Niewykorzystane
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly bool _isSimulationMode;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _isSimulationMode = _configuration.GetValue<bool>("Email:SimulationMode", false);
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (_isSimulationMode)
            {
                await SimulateEmailSending(to, subject, body);
                return;
            }

            try
            {
                var smtpClient = new SmtpClient(_configuration["Email:SmtpServer"])
                {
                    Port = int.Parse(_configuration["Email:SmtpPort"]!),
                    Credentials = new NetworkCredential(
                        _configuration["Email:Username"],
                        _configuration["Email:Password"]),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["Email:FromAddress"]!),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(to);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email wysłany do: {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas wysyłania emaila do: {To}", to);
                throw;
            }
        }

        private async Task SimulateEmailSending(string to, string subject, string body)
        {
            // Symulacja opóźnienia wysyłania
            await Task.Delay(500);
            
            _logger.LogInformation("SYMULACJA: Email został 'wysłany' do: {To}", to);
            _logger.LogInformation("SYMULACJA: Temat: {Subject}", subject);
            _logger.LogDebug("SYMULACJA: Treść: {Body}", body);
            
            // Opcjonalnie: zapisz do pliku
            await SaveEmailToFile(to, subject, body);
        }

        private async Task SaveEmailToFile(string to, string subject, string body)
        {
            var fileName = $"email_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N[..8]}.html";
            var filePath = Path.Combine("SimulatedEmails", fileName);
            
            Directory.CreateDirectory("SimulatedEmails");
            
            var emailContent = $@"
                <html>
                <head><title>{subject}</title></head>
                <body>
                    <h3>Do: {to}</h3>
                    <h3>Temat: {subject}</h3>
                    <hr>
                    {body}
                </body>
                </html>";
            
            await File.WriteAllTextAsync(filePath, emailContent);
            _logger.LogInformation("SYMULACJA: Email zapisany do pliku: {FilePath}", filePath);
        }

        public async Task SendWelcomeEmailAsync(string to, string userName)
        {
            var subject = "Witamy w systemie ogłoszeń!";
            var body = $@"
                <h2>Witaj {userName}!</h2>
                <p>Dziękujemy za rejestrację w naszym systemie ogłoszeń.</p>
                <p>Możesz teraz przeglądać i dodawać ogłoszenia.</p>
            ";
            
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendOgloszenieNotificationAsync(string to, string ogloszenieTitle)
        {
            var subject = "Nowe ogłoszenie w Twojej kategorii";
            var body = $@"
                <h2>Nowe ogłoszenie!</h2>
                <p>Dodano nowe ogłoszenie: <strong>{ogloszenieTitle}</strong></p>
                <p>Sprawdź szczegóły na naszej stronie.</p>
            ";
            
            await SendEmailAsync(to, subject, body);
        }
    }
}