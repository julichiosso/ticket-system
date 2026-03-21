namespace TicketSystem.Dominio.Entidades;

public class Attachment
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public TicketComment Comment { get; set; } = null!;
    public string OriginalName { get; set; } = null!;
    public string StoredName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
}
