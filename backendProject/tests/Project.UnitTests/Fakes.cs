using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Project.Application.Common.Interfaces;
using Project.Application.TimeEntries;
using Project.Application.TimeEntries.Commands.CreateTimeEntry;
using Project.Application.TimeEntries.Commands.DeleteTimeEntry;
using Project.Application.TimeEntries.Commands.UpdateTimeEntry;
using Project.Domain.Entities;
using Project.Domain.ValueObjects;

using ProjectEntity = Project.Domain.Entities.Project;

namespace Project.UnitTests
{
    public class Fakes
    {
        public Fakes()
        {
            Employees = new Mock<IEmployeeRepository>();
            Projects = new Mock<IProjectRepository>();
            ClosedPeriods = new Mock<IClosedPeriodRepository>();
            TimeEntries = new Mock<ITimeEntryRepository>();

            WithEmployee(TestData.Ivanov());
            WithEmployee(TestData.Petrova());
            WithProject(TestData.Project001());
            WithProject(TestData.Project002());

            ClosedPeriods
                .Setup(r => r.IsClosedAsync(It.IsAny<YearMonth>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((YearMonth period, CancellationToken _) => ClosedMonths.Contains(period));

            TimeEntries
                .Setup(r => r.GetDayHoursAsync(
                    It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => DayHours);

            TimeEntries
                .Setup(r => r.InsertAsync(It.IsAny<TimeEntry>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            TimeEntries
                .Setup(r => r.UpdateAsync(It.IsAny<TimeEntry>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => UpdateSucceeds);

            TimeEntries
                .Setup(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            Employees
                .Setup(r => r.SaveRatesAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        public Mock<IEmployeeRepository> Employees { get; }

        public Mock<IProjectRepository> Projects { get; }

        public Mock<IClosedPeriodRepository> ClosedPeriods { get; }

        public Mock<ITimeEntryRepository> TimeEntries { get; }

        public HashSet<YearMonth> ClosedMonths { get; } = new HashSet<YearMonth>();

        public decimal DayHours { get; set; }

        public bool UpdateSucceeds { get; set; } = true;

        public IDateTimeProvider Clock { get; } = new FixedClock(new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Utc));

        public ICurrentUser CurrentUser { get; } = new FixedUser("tester");

        public IIdGenerator Ids { get; } = new FixedIdGenerator();

        public TimeEntryRuleChecker Checker() =>
            new TimeEntryRuleChecker(Employees.Object, Projects.Object, ClosedPeriods.Object, TimeEntries.Object);

        public CreateTimeEntryCommandHandler CreateHandler() =>
            new CreateTimeEntryCommandHandler(TimeEntries.Object, Checker(), Clock, CurrentUser, Ids);

        public UpdateTimeEntryCommandHandler UpdateHandler() =>
            new UpdateTimeEntryCommandHandler(TimeEntries.Object, Checker(), Clock, CurrentUser);

        public DeleteTimeEntryCommandHandler DeleteHandler() =>
            new DeleteTimeEntryCommandHandler(TimeEntries.Object, Checker());

        public void WithEmployee(Employee employee) =>
            Employees
                .Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(employee);

        public void WithProject(ProjectEntity project) =>
            Projects
                .Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(project);

        public void WithExistingEntry(TimeEntry entry) =>
            TimeEntries
                .Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entry);

        public void ClosePeriod(int year, int month) => ClosedMonths.Add(new YearMonth(year, month));

        private class FixedClock : IDateTimeProvider
        {
            public FixedClock(DateTime utcNow) => UtcNow = utcNow;

            public DateTime UtcNow { get; }
        }

        private class FixedUser : ICurrentUser
        {
            public FixedUser(string name) => Name = name;

            public string Name { get; }
        }

        private class FixedIdGenerator : IIdGenerator
        {
            public string NewId() => "660000000000000000000099";
        }
    }
}
