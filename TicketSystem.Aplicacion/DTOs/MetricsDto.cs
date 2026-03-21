using System;
using System.Collections.Generic;

namespace TicketSystem.Aplicacion.DTOs
{
    public class MetricsDto
    {
        public int TotalTickets { get; set; }
        public int PendingTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int WaitingForUserTickets { get; set; }
        
        public double SlaComplianceRate { get; set; }
        public string AverageResolutionTime { get; set; } = "0h 0m";
        
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
        public int ResolvedToday { get; set; }
    }
}
