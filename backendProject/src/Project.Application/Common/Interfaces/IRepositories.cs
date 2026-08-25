using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Project.Domain.Entities;
using Project.Domain.ValueObjects;

namespace Project.Application.Common.Interfaces
{   
    public interface IClosedPeriodRepository
    {
        Task<bool> IsClosedAsync(YearMonth period, CancellationToken cancellationToken);

        Task<IReadOnlyList<ClosedPeriod>> GetAllAsync(CancellationToken cancellationToken);

        Task<bool> CloseAsync(YearMonth period, DateTime closedAt, string id, CancellationToken cancellationToken);

        Task<bool> OpenAsync(YearMonth period, CancellationToken cancellationToken);
    }

    public interface ITimeEntryRepository
    {
        Task<TimeEntry?> GetByIdAsync(string id, CancellationToken cancellationToken);

        Task InsertAsync(TimeEntry entry, CancellationToken cancellationToken);

        Task<bool> UpdateAsync(TimeEntry entry, int expectedVersion, CancellationToken cancellationToken);

        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);

        Task<decimal> GetDayHoursAsync(
            string employeeId,
            DateTime date,
            string? excludeEntryId,
            CancellationToken cancellationToken);
    }
}
