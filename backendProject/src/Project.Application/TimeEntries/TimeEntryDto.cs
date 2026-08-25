using System;

namespace Project.Application.TimeEntries
{
    public class TimeEntryDto
    {
        public string Id { get; set; } = string.Empty;

        public string EmployeeId { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public string ProjectId { get; set; } = string.Empty;

        public string ProjectCode { get; set; } = string.Empty;

        public string ProjectName { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public decimal Hours { get; set; }

        public decimal? Rate { get; set; }

        public decimal Amount { get; set; }

        public string? Comment { get; set; }

        public bool IsOvertime { get; set; }

        public decimal DayTotalHours { get; set; }

        public int Version { get; set; }
    }
}
