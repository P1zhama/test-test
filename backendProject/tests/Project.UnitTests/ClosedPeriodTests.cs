using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Project.Application.TimeEntries.Commands.CreateTimeEntry;
using Project.Application.TimeEntries.Commands.DeleteTimeEntry;
using Project.Application.TimeEntries.Commands.UpdateTimeEntry;
using Project.Domain;
using Project.Domain.Entities;
using Project.Domain.Exceptions;
using Xunit;

namespace Project.UnitTests
{
    public class ClosedPeriodTests
    {
        private const string EntryId = "660000000000000000000021";

        private static TimeEntry FebruaryEntry() =>
            TimeEntry.Create(
                EntryId,
                TestData.IvanovId,
                TestData.Project001Id,
                TestData.Day(2026, 2, 20),
                8m,
                "Обследование",
                new DateTime(2026, 2, 20, 10, 0, 0, DateTimeKind.Utc),
                "seed");

        private static Fakes WithClosedFebruary()
        {
            var fakes = new Fakes();
            fakes.WithExistingEntry(FebruaryEntry());
            fakes.ClosePeriod(2026, 2);
            return fakes;
        }

        private static UpdateTimeEntryCommand UpdateTo(DateTime date, decimal hours) =>
            new UpdateTimeEntryCommand
            {
                Id = EntryId,
                EmployeeId = TestData.IvanovId,
                ProjectId = TestData.Project001Id,
                Date = date,
                Hours = hours,
                Version = 1
            };

        private static async Task ShouldBeRejectedAsClosed(Func<Task> act) =>
            (await act.Should().ThrowAsync<BusinessRuleException>())
                .Where(e => e.Code == ErrorCodes.PeriodClosed);

        [Fact(DisplayName = "В закрытом месяце нельзя создать запись")]
        public async Task Create_is_rejected_in_closed_period()
        {
            var fakes = WithClosedFebruary();

            Func<Task> act = () => fakes.CreateHandler().Handle(new CreateTimeEntryCommand
            {
                EmployeeId = TestData.IvanovId,
                ProjectId = TestData.Project001Id,
                Date = TestData.Day(2026, 2, 20),
                Hours = 8m
            }, CancellationToken.None);

            (await act.Should().ThrowAsync<BusinessRuleException>())
                .Where(e => e.Code == ErrorCodes.PeriodClosed)
                .Where(e => e.Message.Contains("02.2026"));
        }

        [Fact(DisplayName = "Сценарий 5: запись из закрытого февраля изменить нельзя")]
        public Task Update_is_rejected_in_closed_period()
        {
            var fakes = WithClosedFebruary();

            return ShouldBeRejectedAsClosed(() =>
                fakes.UpdateHandler().Handle(UpdateTo(TestData.Day(2026, 2, 20), 4m), CancellationToken.None));
        }

        [Fact(DisplayName = "Запись нельзя перенести из закрытого месяца в открытый")]
        public Task Update_cannot_move_entry_out_of_closed_period()
        {
            var fakes = WithClosedFebruary();

            return ShouldBeRejectedAsClosed(() =>
                fakes.UpdateHandler().Handle(UpdateTo(TestData.Day(2026, 3, 5), 8m), CancellationToken.None));
        }

        [Fact(DisplayName = "В закрытом месяце нельзя удалить запись")]
        public Task Delete_is_rejected_in_closed_period()
        {
            var fakes = WithClosedFebruary();

            return ShouldBeRejectedAsClosed(() =>
                fakes.DeleteHandler().Handle(new DeleteTimeEntryCommand { Id = EntryId }, CancellationToken.None));
        }

        [Fact(DisplayName = "Открытый месяц редактируется как обычно")]
        public async Task Open_period_allows_editing()
        {
            var fakes = new Fakes();
            fakes.WithExistingEntry(FebruaryEntry());

            Func<Task> act = () =>
                fakes.DeleteHandler().Handle(new DeleteTimeEntryCommand { Id = EntryId }, CancellationToken.None);

            await act.Should().NotThrowAsync();
        }
    }
}
