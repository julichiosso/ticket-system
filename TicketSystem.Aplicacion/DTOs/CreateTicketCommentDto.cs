namespace TicketSystem.Aplicacion.DTOs
{
    public class CreateTicketCommentDto
    {
        public string Message { get; set; } = string.Empty;
        public bool IsInternal { get; set; }
    }
}
