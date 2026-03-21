using Microsoft.EntityFrameworkCore;
using TicketSystem.Dominio.Entidades;

namespace TicketSystem.Infraestructura.Datos;

public class TicketSystemDbContext : DbContext
{
    public TicketSystemDbContext(DbContextOptions<TicketSystemDbContext> options)
        : base(options) { }

    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<TicketComment> TicketComments { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Attachment> Attachments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Map entity names to existing Spanish table names from migrations
        modelBuilder.Entity<User>().ToTable("Usuarios");
        modelBuilder.Entity<Ticket>().ToTable("Tickets");
        modelBuilder.Entity<TicketComment>().ToTable("ComentariosTicket");
        modelBuilder.Entity<AuditLog>().ToTable("AuditLogs");
        modelBuilder.Entity<Attachment>().ToTable("ArchivosAdjuntos");

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasOne(t => t.AssignedOperator)
                .WithMany()
                .HasForeignKey(t => t.AssignedOperatorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TicketComment>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Message).IsRequired().HasMaxLength(2000);

            entity.HasOne(c => c.Ticket)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Author)
                .WithMany()
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(c => c.Attachments)
                .WithOne(a => a.Comment)
                .HasForeignKey(a => a.CommentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.OriginalName).IsRequired().HasMaxLength(255);
            entity.Property(a => a.StoredName).IsRequired().HasMaxLength(255);
            entity.Property(a => a.ContentType).IsRequired().HasMaxLength(100);
        });
    }
}