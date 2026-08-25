using System;
using System.Threading;
using System.Threading.Tasks;
using Project.Application.Common.Interfaces;
using Project.Domain;
using Project.Domain.Common;
using Project.Domain.Entities;
using Project.Domain.Exceptions;
using Project.Domain.Rules;
using Project.Domain.ValueObjects;

using ProjectEntity = Project.Domain.Entities.Project;

namespace Project.Application.TimeEntries
{
    public class TimeEntrySaveContext
    {
        public TimeEntrySaveContext(Employee employee, ProjectEntity project, Rate rate, decimal dayTotalHours)
        {
            Employee = employee;
            Project = project;
            Rate = rate;
            DayTotalHours = dayTotalHours;
        }

        public Employee Employee { get; }

        public ProjectEntity Project { get; }

        public Rate Rate { get; }

        public decimal DayTotalHours { get; }

        public bool IsOvertime => DailyWorkloadPolicy.IsOvertime(DayTotalHours);
    }

    public class TimeEntryRuleChecker
    {
        private readonly IEmployeeRepository _employees;
        private readonly IProjectRepository _projects;
        private readonly IClosedPeriodRepository _closedPeriods;
        private readonly ITimeEntryRepository _timeEntries;

        public TimeEntryRuleChecker(
            IEmployeeRepository employees,
            IProjectRepository projects,
            IClosedPeriodRepository closedPeriods,
            ITimeEntryRepository timeEntries)
        {
            _employees = employees;
            _projects = projects;
            _closedPeriods = closedPeriods;
            _timeEntries = timeEntries;
        }

        public async Task EnsurePeriodOpenAsync(DateTime date, CancellationToken cancellationToken)
        {
            var period = YearMonth.Of(date);
            var isClosed = await _closedPeriods.IsClosedAsync(period, cancellationToken);
            TimeEntryRules.EnsurePeriodOpen(isClosed, period);
        }

        public async Task<TimeEntrySaveContext> EnsureCanSaveAsync(
            string employeeId,
            string projectId,
            DateTime date,
            decimal hours,
            string? excludeEntryId,
            CancellationToken cancellationToken)
        {
            var day = DateUtc.Day(date);

            await EnsurePeriodOpenAsync(day, cancellationToken);

            var employee = await _employees.GetByIdAsync(employeeId, cancellationToken);
            if (employee == null)
                throw new BusinessRuleException(ErrorCodes.NotFound, "Сотрудник не найден.");

            var project = await _projects.GetByIdAsync(projectId, cancellationToken);
            if (project == null)
                throw new BusinessRuleException(ErrorCodes.NotFound, "Проект не найден.");

            TimeEntryRules.EnsureDateInProjectPeriod(project, day);
            var rate = TimeEntryRules.EnsureRateOnDate(employee, day);

            var otherHours = await _timeEntries.GetDayHoursAsync(employeeId, day, excludeEntryId, cancellationToken);
            DailyWorkloadPolicy.EnsureDailyLimit(otherHours, hours, day);

            return new TimeEntrySaveContext(employee, project, rate, otherHours + hours);
        }
    }
}
