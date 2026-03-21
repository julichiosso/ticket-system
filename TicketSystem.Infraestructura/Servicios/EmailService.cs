using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using TicketSystem.Aplicacion.Interfaces;

namespace TicketSystem.Infraestructura.Servicios;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string recipient, string subject, string htmlBody)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var host = emailSettings["Host"];
        var port = int.Parse(emailSettings["Port"] ?? "587");
        var username = emailSettings["Username"];
        var password = emailSettings["Password"];
        var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");
        var displayName = emailSettings["DisplayName"] ?? "TicketSystem";

        using var client = new SmtpClient(host, port);
        client.UseDefaultCredentials = false;
        client.Credentials = new NetworkCredential(username, password);
        client.EnableSsl = enableSsl;
        client.DeliveryMethod = SmtpDeliveryMethod.Network;

        var pickupDirectory = emailSettings["PickupDirectoryLocation"];
        if (!string.IsNullOrEmpty(pickupDirectory))
        {
            if (!Directory.Exists(pickupDirectory))
            {
                Directory.CreateDirectory(pickupDirectory);
            }
            client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
            client.PickupDirectoryLocation = pickupDirectory;
        }

        var mailMessage = new MailMessage
        {
            From = new MailAddress(username!, displayName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        mailMessage.To.Add(recipient);

        try {
            await client.SendMailAsync(mailMessage);
            Console.WriteLine($"[DEBUG] Email sent successfully to {recipient} using {username}");
        } catch (Exception ex) {
            Console.WriteLine($"[ERROR] SMTP Failed to {recipient} using {username}. Error: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"[ERROR] Inner Exception: {ex.InnerException.Message}");
            throw;
        }
    }
}
