using MediatR;
using Project.Application.Common.Models;

namespace Project.Application.TimeEntries.Queries.GetTimeEntries
{
    public class GetTimeEntriesQuery : IRequest<TimeEntriesPageDto>
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public string? EmployeeId { get; set; }

        public string? ProjectId { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class TimeEntriesPageDto
    {
        public PagedResult<TimeEntryDto> Page { get; set; } = new PagedResult<TimeEntryDto>(new TimeEntryDto[0], 0, 1, 25);

        public decimal TotalHours { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
