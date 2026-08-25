using System;
using System.Threading;
using System.Threading.Tasks;
using Project.Domain.Entities;

namespace Project.Application.Common.Interfaces
{
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
