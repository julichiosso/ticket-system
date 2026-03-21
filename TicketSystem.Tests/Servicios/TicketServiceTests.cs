using Moq;
using TicketSystem.Aplicacion.DTOs;
using TicketSystem.Aplicacion.Interfaces;
using TicketSystem.Aplicacion.Servicios;
using TicketSystem.Dominio.Entidades;
using TicketSystem.Dominio.Enumeraciones;
using Xunit;

namespace TicketSystem.Tests.Servicios
{
    public class TicketServiceTests
    {
        private readonly Mock<ITicketRepository> _ticketRepo;
        private readonly Mock<IUserRepository> _userRepo;
        private readonly Mock<INotificationService> _notifications;
        private readonly TicketService _service;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _operatorId = Guid.NewGuid();
        private readonly Guid _ticketId = Guid.NewGuid();
        private readonly User _user;
        private readonly User _operator;
        private readonly Ticket _ticket;

        public TicketServiceTests()
        {
            _ticketRepo = new Mock<ITicketRepository>();
            _userRepo = new Mock<IUserRepository>();
            _notifications = new Mock<INotificationService>();

            _service = new TicketService(
                _ticketRepo.Object,
                _userRepo.Object,
                _notifications.Object);

            _user = new User
            {
                Id = _userId,
                Name = "John Doe",
                Email = "john@company.com",
                Role = UserRole.User
            };

            _operator = new User
            {
                Id = _operatorId,
                Name = "Jane Smith",
                Email = "jane@company.com",
                Role = UserRole.Operator
            };

            _ticket = new Ticket
            {
                Id = _ticketId,
                Title = "Cannot log in",
                Description = "It says incorrect password",
                Status = TicketStatus.Pending,
                Priority = TicketPriority.High,
                UserId = _userId,
                User = _user,
                CreatedAt = DateTime.UtcNow,
                Deadline = DateTime.UtcNow.AddHours(4),
                SlaComplied = true
            };
        }

        [Fact]
        public async Task CreateAsync_ValidUser_ReturnsGuid()
        {
            var dto = new CreateTicketDto
            {
                Title = "Printer problem",
                Description = "Not printing in color",
                Priority = TicketPriority.Medium,
                UserId = _userId
            };
            _userRepo.Setup(r => r.GetByIdAsync(_userId)).ReturnsAsync(_user);
            _ticketRepo.Setup(r => r.CreateAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);
            _ticketRepo.Setup(r => r.RecordAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);

            var result = await _service.CreateAsync(dto);

            Assert.NotEqual(Guid.Empty, result);
        }

        [Fact]
        public async Task CreateAsync_ValidUser_CallsNotification()
        {
            var dto = new CreateTicketDto
            {
                Title = "Test ticket",
                Description = "Description",
                Priority = TicketPriority.Low,
                UserId = _userId
            };
            _userRepo.Setup(r => r.GetByIdAsync(_userId)).ReturnsAsync(_user);
            _ticketRepo.Setup(r => r.CreateAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);
            _ticketRepo.Setup(r => r.RecordAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);

            await _service.CreateAsync(dto);

            _notifications.Verify(
                n => n.NotifyTicketCreatedAsync(It.IsAny<Ticket>(), _user),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_NonExistentUser_ThrowsException()
        {
            var dto = new CreateTicketDto { UserId = Guid.NewGuid() };
            _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateAsync(dto));
        }

        [Theory]
        [InlineData(TicketPriority.Critical, 2)]
        [InlineData(TicketPriority.High, 4)]
        [InlineData(TicketPriority.Medium, 24)]
        [InlineData(TicketPriority.Low, 48)]
        public async Task CreateAsync_AssignsDeadlineBasedOnPriority(TicketPriority priority, int expectedHours)
        {
            var dto = new CreateTicketDto
            {
                Title = "SLA Test",
                Description = "Test",
                Priority = priority,
                UserId = _userId
            };
            _userRepo.Setup(r => r.GetByIdAsync(_userId)).ReturnsAsync(_user);

            Ticket? capturedTicket = null;
            _ticketRepo
                .Setup(r => r.CreateAsync(It.IsAny<Ticket>()))
                .Callback<Ticket>(t => capturedTicket = t)
                .Returns(Task.CompletedTask);
            _ticketRepo.Setup(r => r.RecordAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);

            await _service.CreateAsync(dto);

            Assert.NotNull(capturedTicket!.Deadline);
            var actualHours = (capturedTicket.Deadline!.Value - capturedTicket.CreatedAt).TotalHours;
            Assert.Equal(expectedHours, actualHours, precision: 0);
        }

        [Fact]
        public async Task ChangeStatusAsync_ValidTicket_UpdatesStatus()
        {
            _ticketRepo.Setup(r => r.GetByIdAsync(_ticketId)).ReturnsAsync(_ticket);
            _ticketRepo.Setup(r => r.UpdateAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);
            _ticketRepo.Setup(r => r.RecordAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);
            _userRepo.Setup(r => r.GetByIdAsync(_userId)).ReturnsAsync(_user);

            await _service.ChangeStatusAsync(_ticketId, TicketStatus.InProgress, _operatorId);

            Assert.Equal(TicketStatus.InProgress, _ticket.Status);
        }

        [Fact]
        public async Task ChangeStatusAsync_ClosedTicket_ThrowsException()
        {
            _ticket.Status = TicketStatus.Closed;
            _ticketRepo.Setup(r => r.GetByIdAsync(_ticketId)).ReturnsAsync(_ticket);

            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.ChangeStatusAsync(_ticketId, TicketStatus.InProgress, _operatorId));
        }

        [Fact]
        public async Task ChangeStatusAsync_NonExistentTicket_ThrowsException()
        {
            _ticketRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Ticket?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.ChangeStatusAsync(Guid.NewGuid(), TicketStatus.InProgress, _operatorId));
        }

        [Fact]
        public async Task ChangeStatusAsync_ToResolved_AssignsResolutionDate()
        {
            _ticketRepo.Setup(r => r.GetByIdAsync(_ticketId)).ReturnsAsync(_ticket);
            _ticketRepo.Setup(r => r.UpdateAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);
            _ticketRepo.Setup(r => r.RecordAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);
            _userRepo.Setup(r => r.GetByIdAsync(_userId)).ReturnsAsync(_user);

            await _service.ChangeStatusAsync(_ticketId, TicketStatus.Resolved, _operatorId);

            Assert.NotNull(_ticket.ResolvedAt);
        }

        [Fact]
        public async Task ChangeStatusAsync_ActorIsOwner_DoesNotSendNotification()
        {
            _ticketRepo.Setup(r => r.GetByIdAsync(_ticketId)).ReturnsAsync(_ticket);
            _ticketRepo.Setup(r => r.UpdateAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);
            _ticketRepo.Setup(r => r.RecordAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);
            _userRepo.Setup(r => r.GetByIdAsync(_userId)).ReturnsAsync(_user);

            await _service.ChangeStatusAsync(_ticketId, TicketStatus.InProgress, _userId);

            _notifications.Verify(
                n => n.NotifyStatusChangeAsync(It.IsAny<Ticket>(), It.IsAny<User>(), It.IsAny<TicketStatus>(), It.IsAny<TicketStatus>()),
                Times.Never);
        }

        [Fact]
        public async Task ChangeStatusAsync_ActorIsOperator_NotifiesOwner()
        {
            _ticketRepo.Setup(r => r.GetByIdAsync(_ticketId)).ReturnsAsync(_ticket);
            _ticketRepo.Setup(r => r.UpdateAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);
            _ticketRepo.Setup(r => r.RecordAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);
            _userRepo.Setup(r => r.GetByIdAsync(_userId)).ReturnsAsync(_user);

            await _service.ChangeStatusAsync(_ticketId, TicketStatus.InProgress, _operatorId);

            _notifications.Verify(
                n => n.NotifyStatusChangeAsync(_ticket, _user, TicketStatus.Pending, TicketStatus.InProgress),
                Times.Once);
        }

        [Fact]
        public async Task AssignOperatorAsync_ValidOperator_AssignsAndNotifies()
        {
            _ticketRepo.Setup(r => r.GetByIdAsync(_ticketId)).ReturnsAsync(_ticket);
            _ticketRepo.Setup(r => r.UpdateAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);
            _ticketRepo.Setup(r => r.RecordAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);
            _userRepo.Setup(r => r.GetByIdAsync(_operatorId)).ReturnsAsync(_operator);

            await _service.AssignOperatorAsync(_ticketId, _operatorId, Guid.NewGuid());

            Assert.Equal(_operatorId, _ticket.AssignedOperatorId);

            _notifications.Verify(
                n => n.NotifyOperatorAssignmentAsync(_ticket, _operator),
                Times.Once);
        }

        [Fact]
        public async Task AssignOperatorAsync_UserNotAnOperator_ThrowsException()
        {
            var regularUser = new User { Id = Guid.NewGuid(), Role = UserRole.User };
            _ticketRepo.Setup(r => r.GetByIdAsync(_ticketId)).ReturnsAsync(_ticket);
            _userRepo.Setup(r => r.GetByIdAsync(regularUser.Id)).ReturnsAsync(regularUser);

            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.AssignOperatorAsync(_ticketId, regularUser.Id, Guid.NewGuid()));
        }

        [Fact]
        public async Task AssignOperatorAsync_NullOperator_UnassignsOperator()
        {
            _ticket.AssignedOperatorId = _operatorId;
            _ticketRepo.Setup(r => r.GetByIdAsync(_ticketId)).ReturnsAsync(_ticket);
            _ticketRepo.Setup(r => r.UpdateAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);
            _ticketRepo.Setup(r => r.RecordAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);

            await _service.AssignOperatorAsync(_ticketId, null, Guid.NewGuid());

            Assert.Null(_ticket.AssignedOperatorId);
            Assert.Null(_ticket.AssignedAt);
        }

        [Fact]
        public async Task DeleteAsync_ExistingTicket_MarksAsDeleted()
        {
            _ticketRepo.Setup(r => r.GetByIdAsync(_ticketId)).ReturnsAsync(_ticket);
            _ticketRepo.Setup(r => r.UpdateAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);
            _ticketRepo.Setup(r => r.RecordAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);

            await _service.DeleteAsync(_ticketId, _userId);

            Assert.True(_ticket.IsDeleted);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentTicket_ThrowsException()
        {
            _ticketRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Ticket?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.DeleteAsync(Guid.NewGuid(), _userId));
        }

        [Fact]
        public async Task AddCommentAsync_PublicComment_NotifiesOwner()
        {
            var dto = new CreateTicketCommentDto { Message = "Checking your case.", IsInternal = false };
            _ticketRepo.Setup(r => r.GetByIdAsync(_ticketId)).ReturnsAsync(_ticket);
            _userRepo.Setup(r => r.GetByIdAsync(_operatorId)).ReturnsAsync(_operator);
            _userRepo.Setup(r => r.GetByIdAsync(_userId)).ReturnsAsync(_user);
            _ticketRepo.Setup(r => r.AddCommentAsync(It.IsAny<TicketComment>())).Returns(Task.CompletedTask);
            _ticketRepo.Setup(r => r.RecordAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);

            await _service.AddCommentAsync(_ticketId, _operatorId, dto);

            _notifications.Verify(
                n => n.NotifyNewCommentAsync(_ticket, _user, It.IsAny<TicketComment>(), _operator),
                Times.Once);
        }

        [Fact]
        public async Task AddCommentAsync_InternalComment_DoesNotNotify()
        {
            var dto = new CreateTicketCommentDto { Message = "Internal note.", IsInternal = true };
            _ticketRepo.Setup(r => r.GetByIdAsync(_ticketId)).ReturnsAsync(_ticket);
            _userRepo.Setup(r => r.GetByIdAsync(_operatorId)).ReturnsAsync(_operator);
            _ticketRepo.Setup(r => r.AddCommentAsync(It.IsAny<TicketComment>())).Returns(Task.CompletedTask);
            _ticketRepo.Setup(r => r.RecordAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);

            await _service.AddCommentAsync(_ticketId, _operatorId, dto);

            _notifications.Verify(
                n => n.NotifyNewCommentAsync(It.IsAny<Ticket>(), It.IsAny<User>(), It.IsAny<TicketComment>(), It.IsAny<User>()),
                Times.Never);
        }

        [Fact]
        public async Task AddCommentAsync_NonExistentTicket_ThrowsException()
        {
            _ticketRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Ticket?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.AddCommentAsync(Guid.NewGuid(), _operatorId, new CreateTicketCommentDto()));
        }

        [Fact]
        public async Task ChangeStatusAsync_ResolvedWithinSLA_SlaCompliedTrue()
        {
            _ticket.Deadline = DateTime.UtcNow.AddHours(2);
            _ticketRepo.Setup(r => r.GetByIdAsync(_ticketId)).ReturnsAsync(_ticket);
            _ticketRepo.Setup(r => r.UpdateAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);
            _ticketRepo.Setup(r => r.RecordAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);
            _userRepo.Setup(r => r.GetByIdAsync(_userId)).ReturnsAsync(_user);

            await _service.ChangeStatusAsync(_ticketId, TicketStatus.Resolved, _operatorId);

            Assert.True(_ticket.SlaComplied);
        }

        [Fact]
        public async Task ChangeStatusAsync_ResolvedPastSLA_SlaCompliedFalse()
        {
            _ticket.Deadline = DateTime.UtcNow.AddHours(-1);
            _ticketRepo.Setup(r => r.GetByIdAsync(_ticketId)).ReturnsAsync(_ticket);
            _ticketRepo.Setup(r => r.UpdateAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);
            _ticketRepo.Setup(r => r.RecordAuditLogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);
            _userRepo.Setup(r => r.GetByIdAsync(_userId)).ReturnsAsync(_user);

            await _service.ChangeStatusAsync(_ticketId, TicketStatus.Resolved, _operatorId);

            Assert.False(_ticket.SlaComplied);
        }
    }
}
