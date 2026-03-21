using Microsoft.Extensions.Configuration;
using TicketSystem.Aplicacion.Interfaces;
using TicketSystem.Dominio.Entidades;
using TicketSystem.Dominio.Enumeraciones;

namespace TicketSystem.Infraestructura.Servicios;

public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;
    private readonly string _frontendBase;
    private readonly string _appName;

    public NotificationService(IEmailService emailService, IConfiguration configuration)
    {
        _emailService = emailService;
        _frontendBase = configuration["AppSettings:FrontendBaseUrl"] ?? "http://localhost:5173";
        _appName = configuration["AppSettings:AppName"] ?? "TicketSystem";
    }

    public async Task NotifyTicketCreatedAsync(Ticket ticket, User user)
    {
        var subject = $"[{_appName}] Your ticket #{ticket.Id.ToString()[..8].ToUpper()} has been received";
        var body = BuildEmail(
            title: "Ticket Received!",
            intro: $"Hello <strong>{user.Name}</strong>, we have successfully registered your ticket.",
            content: $@"
                <table style='width:100%;border-collapse:collapse;margin:16px 0;'>
                    <tr><td style='padding:8px;background:#f3f4f6;font-weight:bold;width:140px;'>ID</td>
                        <td style='padding:8px;'>{ticket.Id.ToString()[..8].ToUpper()}</td></tr>
                    <tr><td style='padding:8px;background:#f3f4f6;font-weight:bold;'>Subject</td>
                        <td style='padding:8px;'>{ticket.Title}</td></tr>
                    <tr><td style='padding:8px;background:#f3f4f6;font-weight:bold;'>Priority</td>
                        <td style='padding:8px;'>{TranslatePriority(ticket.Priority)}</td></tr>
                    <tr><td style='padding:8px;background:#f3f4f6;font-weight:bold;'>Status</td>
                        <td style='padding:8px;'><span style='color:#2563eb;font-weight:bold;'>{TranslateStatus(ticket.Status)}</span></td></tr>
                </table>
                <p style='color:#555;'>Our team will review your request shortly. We will notify you of any changes.</p>",
            buttonUrl: $"{_frontendBase}/my-tickets/{ticket.Id}",
            buttonText: "View my ticket"
        );

        await SendWithLogging(user.Email, subject, body);
    }

    public async Task NotifyStatusChangeAsync(
        Ticket ticket, User user,
        TicketStatus oldStatus, TicketStatus newStatus)
    {
        var (newColor, emoji) = GetColorAndEmojiStatus(newStatus);

        var subject = $"[{_appName}] {emoji} Your ticket status changed to \"{TranslateStatus(newStatus)}\"";
        var body = BuildEmail(
            title: $"{emoji} Status Updated",
            intro: $"Hello <strong>{user.Name}</strong>, your ticket status has been updated.",
            content: $@"
                <div style='display:flex;gap:12px;align-items:center;margin:20px 0;'>
                    <div style='flex:1;padding:12px;background:#f3f4f6;border-radius:6px;text-align:center;'>
                        <div style='font-size:12px;color:#777;margin-bottom:4px;'>Previous</div>
                        <div style='font-weight:bold;color:#555;'>{TranslateStatus(oldStatus)}</div>
                    </div>
                    <div style='font-size:20px;color:#9ca3af;'>→</div>
                    <div style='flex:1;padding:12px;background:{newColor}20;border:2px solid {newColor};border-radius:6px;text-align:center;'>
                        <div style='font-size:12px;color:#777;margin-bottom:4px;'>New</div>
                        <div style='font-weight:bold;color:{newColor};'>{TranslateStatus(newStatus)}</div>
                    </div>
                </div>
                <table style='width:100%;border-collapse:collapse;'>
                    <tr><td style='padding:8px;background:#f3f4f6;font-weight:bold;width:140px;'>Ticket</td>
                        <td style='padding:8px;'>{ticket.Title}</td></tr>
                    <tr><td style='padding:8px;background:#f3f4f6;font-weight:bold;'>ID</td>
                        <td style='padding:8px;'>{ticket.Id.ToString()[..8].ToUpper()}</td></tr>
                </table>",
            buttonUrl: $"{_frontendBase}/my-tickets/{ticket.Id}",
            buttonText: "View ticket"
        );

        await SendWithLogging(user.Email, subject, body);
    }

    public async Task NotifyOperatorAssignmentAsync(Ticket ticket, User @operator)
    {
        var subject = $"[{_appName}] 📋 New ticket assigned: {ticket.Title}";
        var body = BuildEmail(
            title: "📋 A ticket has been assigned to you",
            intro: $"Hello <strong>{@operator.Name}</strong>, a ticket has been assigned to you for management.",
            content: $@"
                <table style='width:100%;border-collapse:collapse;margin:16px 0;'>
                    <tr><td style='padding:8px;background:#f3f4f6;font-weight:bold;width:140px;'>ID</td>
                        <td style='padding:8px;'>{ticket.Id.ToString()[..8].ToUpper()}</td></tr>
                    <tr><td style='padding:8px;background:#f3f4f6;font-weight:bold;'>Subject</td>
                        <td style='padding:8px;'>{ticket.Title}</td></tr>
                    <tr><td style='padding:8px;background:#f3f4f6;font-weight:bold;'>Priority</td>
                        <td style='padding:8px;'><strong style='color:{GetPriorityColor(ticket.Priority)};'>{TranslatePriority(ticket.Priority)}</strong></td></tr>
                    <tr><td style='padding:8px;background:#f3f4f6;font-weight:bold;'>Status</td>
                        <td style='padding:8px;'>{TranslateStatus(ticket.Status)}</td></tr>
                    <tr><td style='padding:8px;background:#f3f4f6;font-weight:bold;'>Date</td>
                        <td style='padding:8px;'>{ticket.CreatedAt:MM/dd/yyyy HH:mm}</td></tr>
                </table>
                <p style='color:#555;'>Log in to the dashboard to see full details and manage the ticket.</p>",
            buttonUrl: $"{_frontendBase}/operator/tickets/{ticket.Id}",
            buttonText: "Manage ticket"
        );

        await SendWithLogging(@operator.Email, subject, body);
    }

    public async Task NotifyNewCommentAsync(
        Ticket ticket, User recipient,
        TicketComment comment, User author)
    {
        if (author.Id == recipient.Id) return;

        var subject = $"[{_appName}] 💬 New comment on your ticket: {ticket.Title}";
        var body = BuildEmail(
            title: "💬 New Comment",
            intro: $"Hello <strong>{recipient.Name}</strong>, there is a new reply to your ticket.",
            content: $@"
                <div style='border-left:4px solid #2563eb;padding:12px 16px;background:#f0f7ff;border-radius:0 6px 6px 0;margin:16px 0;'>
                    <div style='font-size:12px;color:#777;margin-bottom:6px;'>
                        <strong>{author.Name}</strong> · {comment.CreatedAt:MM/dd/yyyy HH:mm}
                    </div>
                    <div style='color:#333;white-space:pre-wrap;'>{EscapeHtml(comment.Message)}</div>
                </div>
                <table style='width:100%;border-collapse:collapse;margin-top:16px;'>
                    <tr><td style='padding:8px;background:#f3f4f6;font-weight:bold;width:140px;'>Ticket</td>
                        <td style='padding:8px;'>{ticket.Title}</td></tr>
                    <tr><td style='padding:8px;background:#f3f4f6;font-weight:bold;'>ID</td>
                        <td style='padding:8px;'>{ticket.Id.ToString()[..8].ToUpper()}</td></tr>
                </table>",
            buttonUrl: $"{_frontendBase}/my-tickets/{ticket.Id}",
            buttonText: "Reply"
        );

        await SendWithLogging(recipient.Email, subject, body);
    }

    private string BuildEmail(string title, string intro, string content, string buttonUrl, string buttonText)
    {
        return $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head><meta charset='UTF-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'></head>
        <body style='margin:0;padding:0;background-color:#f4f5f7;font-family:Arial,sans-serif;'>
            <table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f5f7;padding:32px 0;'>
                <tr><td align='center'>
                    <table width='600' cellpadding='0' cellspacing='0' style='max-width:600px;width:100%;background:#ffffff;border-radius:10px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.08);'>
                        <tr>
                            <td style='background:linear-gradient(135deg,#1d4ed8,#2563eb);padding:28px 32px;'>
                                <h1 style='margin:0;color:white;font-size:22px;letter-spacing:0.5px;'>{_appName}</h1>
                                <p style='margin:6px 0 0;color:#bfdbfe;font-size:13px;'>Ticket Management System</p>
                            </td>
                        </tr>
                        <tr>
                            <td style='padding:28px 32px 8px;'>
                                <h2 style='margin:0;color:#1e293b;font-size:20px;'>{title}</h2>
                            </td>
                        </tr>
                        <tr>
                            <td style='padding:8px 32px;color:#475569;font-size:15px;line-height:1.6;'>
                                {intro}
                            </td>
                        </tr>
                        <tr>
                            <td style='padding:12px 32px;'>
                                {content}
                            </td>
                        </tr>
                        <tr>
                            <td style='padding:20px 32px 32px;text-align:center;'>
                                <a href='{buttonUrl}'
                                   style='display:inline-block;background:#2563eb;color:white;padding:13px 32px;
                                          text-decoration:none;border-radius:6px;font-weight:bold;font-size:15px;
                                          letter-spacing:0.3px;'>
                                    {buttonText}
                                </a>
                            </td>
                        </tr>
                        <tr>
                            <td style='background:#f8fafc;padding:16px 32px;border-top:1px solid #e2e8f0;text-align:center;'>
                                <p style='margin:0;color:#94a3b8;font-size:12px;'>
                                    This is an automatic message from <strong>{_appName}</strong>. Please do not reply to this email.
                                </p>
                            </td>
                        </tr>
                    </table>
                    <p style='color:#94a3b8;font-size:11px;margin-top:16px;'>© {DateTime.UtcNow.Year} {_appName}. All rights reserved.</p>
                </td></tr>
            </table>
        </body>
        </html>";
    }

    private async Task SendWithLogging(string recipient, string subject, string body)
    {
        try
        {
            await _emailService.SendEmailAsync(recipient, subject, body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NOTIFICATION ERROR] → {recipient} | {subject} | {ex.Message}");
        }
    }

    private static string TranslateStatus(TicketStatus status) => status switch
    {
        TicketStatus.Pending => "Pending",
        TicketStatus.InProgress => "In Progress",
        TicketStatus.WaitingForUser => "Waiting for User",
        TicketStatus.Resolved => "Resolved",
        TicketStatus.Closed => "Closed",
        _ => status.ToString()
    };

    private static string TranslatePriority(TicketPriority priority) => priority switch
    {
        TicketPriority.Low => "🟢 Low",
        TicketPriority.Medium => "🟡 Medium",
        TicketPriority.High => "🟠 High",
        TicketPriority.Critical => "🔴 Critical",
        _ => priority.ToString()
    };

    private static (string color, string emoji) GetColorAndEmojiStatus(TicketStatus status) => status switch
    {
        TicketStatus.Pending => ("#f59e0b", "⏳"),
        TicketStatus.InProgress => ("#3b82f6", "🔄"),
        TicketStatus.WaitingForUser => ("#a855f7", "⏸️"),
        TicketStatus.Resolved => ("#10b981", "✅"),
        TicketStatus.Closed => ("#6b7280", "🔒"),
        _ => ("#2563eb", "📋")
    };

    private static string GetPriorityColor(TicketPriority priority) => priority switch
    {
        TicketPriority.Low => "#10b981",
        TicketPriority.Medium => "#f59e0b",
        TicketPriority.High => "#ef4444",
        TicketPriority.Critical => "#7c3aed",
        _ => "#2563eb"
    };

    private static string EscapeHtml(string text) =>
        text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
}
