namespace TicketSystem.Aplicacion.DTOs;

public class AttachmentDto
{
    public Guid Id { get; set; }
    public string OriginalName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
    public string Url { get; set; } = null!;
}
