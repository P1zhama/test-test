using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Project.Application.Reports.Queries.GetProjectReport;
using Project.Application.TimeEntries;
using Project.Application.TimeEntries.Queries.GetTimeEntries;
using Project.Domain.ValueObjects;

namespace Project.Application.Common.Interfaces
{
    public interface ITimeEntryReader
    {
        Task<TimeEntriesPageDto> ListAsync(GetTimeEntriesQuery query, CancellationToken cancellationToken);
    }

    public interface IProjectReportReader
    {
        Task<IReadOnlyList<ProjectReportAggregate>> AggregateAsync(
            YearMonth period,
            CancellationToken cancellationToken);
    }
}
