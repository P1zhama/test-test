using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Project.Domain.Entities;
using Project.Domain.ValueObjects;
using ProjectEntity = Project.Domain.Entities.Project;

namespace Project.Application.Common.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetByIdAsync(string id, CancellationToken cancellationToken);

        Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken);

        Task SaveRatesAsync(Employee employee, CancellationToken cancellationToken);
    }

    public interface IProjectRepository
    {
        Task<ProjectEntity?> GetByIdAsync(string id, CancellationToken cancellationToken);

        Task<IReadOnlyList<ProjectEntity>> GetAllAsync(CancellationToken cancellationToken);
    }
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
