using Project.Domain.Entities;
using Project.Domain.Rules;

namespace Project.Application.TimeEntries
{
    public static class TimeEntryMapper
    {
        public static TimeEntryDto ToDto(TimeEntry entry, TimeEntrySaveContext context, int version) =>
            new TimeEntryDto
            {
                Id = entry.Id,
                EmployeeId = entry.EmployeeId,
                EmployeeName = context.Employee.FullName,
                ProjectId = entry.ProjectId,
                ProjectCode = context.Project.Code,
                ProjectName = context.Project.Name,
                Date = entry.Date,
                Hours = entry.Hours,
                Rate = context.Rate.Value,
                Amount = Money.Amount(entry.Hours, context.Rate.Value),
                Comment = entry.Comment,
                IsOvertime = context.IsOvertime,
                DayTotalHours = context.DayTotalHours,
                Version = version
            };
    }
}
