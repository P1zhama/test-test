using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Project.Application.TimeEntries.Commands.CreateTimeEntry;
using Project.Application.TimeEntries.Commands.UpdateTimeEntry;
using Project.Domain;
using Project.Domain.Entities;
using Project.Domain.Exceptions;
using Xunit;

namespace Project.UnitTests
{
    public class TimeEntryHandlerTests
    {
        private const string EntryId = "660000000000000000000022";

        private static CreateTimeEntryCommand Command(
            string employeeId,
            string projectId,
            DateTime date,
            decimal hours) =>
            new CreateTimeEntryCommand
            {
                EmployeeId = employeeId,
                ProjectId = projectId,
                Date = date,
                Hours = hours
            };

        private static TimeEntry ExistingEntry(Fakes fakes)
        {
            var entry = TimeEntry.Create(
                EntryId, TestData.IvanovId, TestData.Project001Id,
                TestData.Day(2026, 3, 5), 8m, null, fakes.Clock.UtcNow, "seed");
            fakes.WithExistingEntry(entry);
            return entry;
        }

        private static UpdateTimeEntryCommand UpdateWith(decimal hours) =>
            new UpdateTimeEntryCommand
            {
                Id = EntryId,
                EmployeeId = TestData.IvanovId,
                ProjectId = TestData.Project001Id,
                Date = TestData.Day(2026, 3, 5),
                Hours = hours,
                Version = 1
            };

        [Fact(DisplayName = "Запись считается по ставке на дату: 05.03.2026 — 8 ч × 600 ₽")]
        public async Task Calculates_amount_by_rate_on_entry_date()
        {
            var fakes = new Fakes();

            var result = await fakes.CreateHandler().Handle(
                Command(TestData.IvanovId, TestData.Project001Id, TestData.Day(2026, 3, 5), 8m),
                CancellationToken.None);

            result.Rate.Should().Be(600m);
            result.Amount.Should().Be(4800m);
            result.IsOvertime.Should().BeFalse();
        }

        [Fact(DisplayName = "Сценарий 1: у Петровой на 15.01.2026 нет ставки — отказ")]
        public async Task Rejects_entry_without_rate()
        {
            var fakes = new Fakes();

            Func<Task> act = () => fakes.CreateHandler().Handle(
                Command(TestData.PetrovaId, TestData.Project001Id, TestData.Day(2026, 1, 15), 8m),
                CancellationToken.None);

            (await act.Should().ThrowAsync<BusinessRuleException>())
                .Where(e => e.Code == ErrorCodes.EmployeeRateNotFound);
        }

        [Fact(DisplayName = "Сценарий 2: 20 ч за день сохраняются и помечаются переработкой")]
        public async Task Saves_twenty_hours_and_flags_overtime()
        {
            var fakes = new Fakes();

            var result = await fakes.CreateHandler().Handle(
                Command(TestData.IvanovId, TestData.Project001Id, TestData.Day(2026, 3, 6), 20m),
                CancellationToken.None);

            result.Hours.Should().Be(20m);
            result.DayTotalHours.Should().Be(20m);
            result.IsOvertime.Should().BeTrue();
            result.Amount.Should().Be(12000m);
        }

        [Fact(DisplayName = "При изменении записи её собственные часы не учитываются дважды")]
        public async Task Own_hours_are_excluded_from_daily_limit_on_update()
        {
            var fakes = new Fakes();
            ExistingEntry(fakes);

            var result = await fakes.UpdateHandler().Handle(UpdateWith(24m), CancellationToken.None);

            result.Hours.Should().Be(24m);
            result.Version.Should().Be(2);
        }

        [Fact(DisplayName = "Сценарий 7: сохранение устаревшей версии записи отклоняется")]
        public async Task Rejects_stale_version()
        {
            var fakes = new Fakes { UpdateSucceeds = false };
            ExistingEntry(fakes);

            Func<Task> act = () => fakes.UpdateHandler().Handle(UpdateWith(6m), CancellationToken.None);

            (await act.Should().ThrowAsync<BusinessRuleException>())
                .Where(e => e.Code == ErrorCodes.ConcurrencyConflict);
        }
    }
}
