using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project.Application.Common.Interfaces;
using Project.Infrastructure.Persistence;
using Project.Infrastructure.Persistence.Readers;
using Project.Infrastructure.Persistence.Repositories;
using Project.Infrastructure.Services;

namespace Project.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<MongoSettings>(configuration.GetSection(MongoSettings.SectionName));

            services.AddSingleton<MongoContext>();

            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IClosedPeriodRepository, ClosedPeriodRepository>();
            services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();

            services.AddScoped<ITimeEntryReader, TimeEntryReader>();
            services.AddScoped<IProjectReportReader, ProjectReportReader>();

            services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
            services.AddSingleton<IIdGenerator, ObjectIdGenerator>();
            services.AddScoped<DataSeeder>();

            services.AddSingleton<MongoIndexInitializer>();

            return services;
        }

        public static IServiceCollection AddMongoIndexInitializer(this IServiceCollection services)
        {
            services.AddHostedService(provider => provider.GetRequiredService<MongoIndexInitializer>());
            return services;
        }
    }
}
