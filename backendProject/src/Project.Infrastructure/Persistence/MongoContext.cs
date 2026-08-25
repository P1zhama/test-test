using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Project.Domain.Entities;

using ProjectEntity = Project.Domain.Entities.Project;

namespace Project.Infrastructure.Persistence
{
    public class MongoContext
    {
        public const string EmployeesCollection = "employees";
        public const string ProjectsCollection = "projects";
        public const string TimeEntriesCollection = "time_entries";
        public const string ClosedPeriodsCollection = "closed_periods";

        public MongoContext(IOptions<MongoSettings> options)
        {
            MongoMappings.Register();

            var settings = options.Value;
            var client = new MongoClient(settings.ConnectionString);
            Database = client.GetDatabase(settings.DatabaseName);
        }

        public IMongoDatabase Database { get; }

        public IMongoCollection<Employee> Employees => Database.GetCollection<Employee>(EmployeesCollection);

        public IMongoCollection<ProjectEntity> Projects => Database.GetCollection<ProjectEntity>(ProjectsCollection);

        public IMongoCollection<TimeEntry> TimeEntries => Database.GetCollection<TimeEntry>(TimeEntriesCollection);

        public IMongoCollection<ClosedPeriod> ClosedPeriods =>
            Database.GetCollection<ClosedPeriod>(ClosedPeriodsCollection);
    }
}
