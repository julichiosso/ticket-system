const fs = require('fs');
const path = require('path');

const filePath = path.join(__dirname, 'TicketSystem.Aplicacion', 'Servicios', 'ServicioTickets.cs');
let content = fs.readFileSync(filePath, 'utf8');

// Remove all single line comments
content = content.replace(/\/\/[^\n]*\n/g, '\n');

// Replace ObtenerPorUsuarioAsync
content = content.replace(
    /public async Task<IEnumerable<TicketDto>> ObtenerPorUsuarioAsync\(Guid usuarioId\)\s*{\s*var tickets = await _repositorioTickets\.ObtenerPorUsuarioAsync\(usuarioId\);\s*return MapToDto\(tickets\);\s*}/g,
    `public async Task<PagedResult<TicketDto>> ObtenerPorUsuarioAsync(Guid usuarioId, int page = 1, int pageSize = 10)\n        {\n            var (tickets, total) = await _repositorioTickets.ObtenerPorUsuarioAsync(usuarioId, page, pageSize);\n            return new PagedResult<TicketDto>\n            {\n                Data = MapToDto(tickets),\n                Page = page,\n                PageSize = pageSize,\n                TotalRecords = total\n            };\n        }`
);

// Replace ObtenerPorOperadorAsync
content = content.replace(
    /public async Task<IEnumerable<TicketDto>> ObtenerPorOperadorAsync\(Guid operadorId\)\s*{\s*var tickets = await _repositorioTickets\.ObtenerPorOperadorAsync\(operadorId\);\s*return MapToDto\(tickets\);\s*}/g,
    `public async Task<PagedResult<TicketDto>> ObtenerPorOperadorAsync(Guid operadorId, int page = 1, int pageSize = 10)\n        {\n            var (tickets, total) = await _repositorioTickets.ObtenerPorOperadorAsync(operadorId, page, pageSize);\n            return new PagedResult<TicketDto>\n            {\n                Data = MapToDto(tickets),\n                Page = page,\n                PageSize = pageSize,\n                TotalRecords = total\n            };\n        }`
);

fs.writeFileSync(filePath, content, 'utf8');
console.log('ServicioTickets updated successfully with no comments.');
