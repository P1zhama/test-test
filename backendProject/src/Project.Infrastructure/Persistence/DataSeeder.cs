using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Project.Domain.Common;
using Project.Domain.Entities;
using Project.Domain.ValueObjects;

using ProjectEntity = Project.Domain.Entities.Project;

namespace Project.Infrastructure.Persistence
{
    public class DataSeeder
    {
        public const string IvanovId = "660000000000000000000001";
        public const string PetrovaId = "660000000000000000000002";
        public const string Project001Id = "660000000000000000000011";
        public const string Project002Id = "660000000000000000000012";

        private readonly MongoContext _context;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(MongoContext context, ILogger<DataSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            await _context.TimeEntries.DeleteManyAsync(FilterDefinition<TimeEntry>.Empty, cancellationToken);
            await _context.Employees.DeleteManyAsync(FilterDefinition<Employee>.Empty, cancellationToken);
            await _context.Projects.DeleteManyAsync(FilterDefinition<ProjectEntity>.Empty, cancellationToken);
            await _context.ClosedPeriods.DeleteManyAsync(FilterDefinition<ClosedPeriod>.Empty, cancellationToken);

            var employees = new List<Employee>
            {
                Employee.Create(IvanovId, "Иванов И. И.", "Проектный", new[]
                {
                    Rate.Create(DateUtc.Day(2026, 1, 1), 500m),
                    Rate.Create(DateUtc.Day(2026, 3, 1), 600m)
                }),
                Employee.Create(PetrovaId, "Петрова А. С.", "Проектный", new[]
                {
                    Rate.Create(DateUtc.Day(2026, 2, 1), 700m)
                })
            };

            var projects = new List<ProjectEntity>
            {
                ProjectEntity.Create(Project001Id, "П-001", "Реконструкция цеха", 20000m,
                    DateUtc.Day(2026, 1, 1), DateUtc.Day(2026, 3, 31)),
                ProjectEntity.Create(Project002Id, "П-002", "Инженерные сети", 5000m,
                    DateUtc.Day(2026, 3, 1), null)
            };

            var now = DateTime.UtcNow;
            var entries = new List<TimeEntry>
            {
                TimeEntry.Create("660000000000000000000021", IvanovId, Project001Id,
                    DateUtc.Day(2026, 2, 20), 8m, "Обследование", now, "seed"),
                TimeEntry.Create("660000000000000000000022", IvanovId, Project001Id,
                    DateUtc.Day(2026, 3, 5), 8m, "Рабочая документация", now, "seed"),
                TimeEntry.Create("660000000000000000000023", PetrovaId, Project001Id,
                    DateUtc.Day(2026, 3, 5), 4m, "Согласования", now, "seed"),
                TimeEntry.Create("660000000000000000000024", PetrovaId, Project002Id,
                    DateUtc.Day(2026, 3, 6), 10m, "Расчёт сетей", now, "seed")
            };

            await _context.Employees.InsertManyAsync(employees, cancellationToken: cancellationToken);
            await _context.Projects.InsertManyAsync(projects, cancellationToken: cancellationToken);
            await _context.TimeEntries.InsertManyAsync(entries, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "База наполнена тестовыми данными: сотрудников {Employees}, проектов {Projects}, записей табеля {Entries}.",
                employees.Count,
                projects.Count,
                entries.Count);
        }
    }
}
