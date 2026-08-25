using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Project.Domain.Entities;

using ProjectEntity = Project.Domain.Entities.Project;

namespace Project.Infrastructure.Persistence
{
    public class MongoIndexInitializer : IHostedService
    {
        private readonly MongoContext _context;
        private readonly ILogger<MongoIndexInitializer> _logger;

        public MongoIndexInitializer(MongoContext context, ILogger<MongoIndexInitializer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _context.TimeEntries.Indexes.CreateManyAsync(
                new[]
                {
                    new CreateIndexModel<TimeEntry>(
                        Builders<TimeEntry>.IndexKeys.Ascending(e => e.Date).Ascending(e => e.ProjectId),
                        new CreateIndexOptions { Name = "ix_date_project" }),

                    new CreateIndexModel<TimeEntry>(
                        Builders<TimeEntry>.IndexKeys.Ascending(e => e.EmployeeId).Ascending(e => e.Date),
                        new CreateIndexOptions { Name = "ix_employee_date" }),

                    new CreateIndexModel<TimeEntry>(
                        Builders<TimeEntry>.IndexKeys.Ascending(e => e.ProjectId).Ascending(e => e.Date),
                        new CreateIndexOptions { Name = "ix_project_date" })
                },
                cancellationToken);

            await _context.Projects.Indexes.CreateOneAsync(
                new CreateIndexModel<ProjectEntity>(
                    Builders<ProjectEntity>.IndexKeys.Ascending(p => p.Code),
                    new CreateIndexOptions { Name = "ux_code", Unique = true }),
                cancellationToken: cancellationToken);

            await _context.Employees.Indexes.CreateOneAsync(
                new CreateIndexModel<Employee>(
                    Builders<Employee>.IndexKeys.Ascending(e => e.FullName),
                    new CreateIndexOptions { Name = "ix_full_name" }),
                cancellationToken: cancellationToken);

            await _context.ClosedPeriods.Indexes.CreateOneAsync(
                new CreateIndexModel<ClosedPeriod>(
                    Builders<ClosedPeriod>.IndexKeys.Ascending(p => p.Year).Ascending(p => p.Month),
                    new CreateIndexOptions { Name = "ux_year_month", Unique = true }),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Индексы MongoDB проверены и созданы.");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
