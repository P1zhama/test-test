using System;
using MediatR;

namespace Project.Application.TimeEntries.Commands.CreateTimeEntry
{
    public class CreateTimeEntryCommand : IRequest<TimeEntryDto>
    {
        public string EmployeeId { get; set; } = string.Empty;

        public string ProjectId { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public decimal Hours { get; set; }

        public string? Comment { get; set; }
    }
}
