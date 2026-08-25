using System;
using MediatR;

namespace Project.Application.TimeEntries.Commands.UpdateTimeEntry
{
    public class UpdateTimeEntryCommand : IRequest<TimeEntryDto>
    {
        public string Id { get; set; } = string.Empty;

        public string EmployeeId { get; set; } = string.Empty;

        public string ProjectId { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public decimal Hours { get; set; }

        public string? Comment { get; set; }

        public int Version { get; set; }
    }
}
