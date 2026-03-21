namespace TicketSystem.Aplicacion.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string recipient, string subject, string htmlBody);
}
