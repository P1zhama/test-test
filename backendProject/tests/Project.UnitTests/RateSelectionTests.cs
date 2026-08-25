using System;
using FluentAssertions;
using Project.Domain;
using Project.Domain.Exceptions;
using Project.Domain.Rules;
using Project.Domain.ValueObjects;
using Xunit;

namespace Project.UnitTests
{
    public class RateSelectionTests
    {
        [Fact(DisplayName = "Запись до смены ставки считается по старой ставке")]
        public void Uses_rate_effective_before_change()
        {
            TestData.Ivanov().RateOn(TestData.Day(2026, 2, 20))!.Value.Should().Be(500m);
        }

        [Fact(DisplayName = "В день начала новой ставки действует уже новая")]
        public void New_rate_applies_from_its_first_day()
        {
            TestData.Ivanov().RateOn(TestData.Day(2026, 3, 1))!.Value.Should().Be(600m);
        }

        [Fact(DisplayName = "После смены ставки берётся новая, а не первая из списка")]
        public void Uses_latest_applicable_rate()
        {
            TestData.Ivanov().RateOn(TestData.Day(2026, 3, 5))!.Value.Should().Be(600m);
        }

        [Fact(DisplayName = "На дату раньше первой ставки ставки нет")]
        public void Returns_null_before_first_rate()
        {
            TestData.Petrova().RateOn(TestData.Day(2026, 1, 15)).Should().BeNull();
        }

        [Fact(DisplayName = "Сценарий 1: без ставки на дату запись создать нельзя")]
        public void Throws_when_no_rate_on_date()
        {
            var employee = TestData.Petrova();

            Action act = () => TimeEntryRules.EnsureRateOnDate(employee, TestData.Day(2026, 1, 15));

            act.Should().Throw<BusinessRuleException>()
                .Where(e => e.Code == ErrorCodes.EmployeeRateNotFound)
                .Where(e => e.Message.Contains("15.01.2026"));
        }

        [Fact(DisplayName = "Сценарий 8: правка ставки задним числом меняет применяемую ставку")]
        public void Retroactive_rate_change_is_applied()
        {
            var employee = TestData.Ivanov();
            employee.SetRates(new[]
            {
                Rate.Create(TestData.Day(2026, 1, 1), 500m),
                Rate.Create(TestData.Day(2026, 3, 1), 650m)
            });

            var rate = employee.RateOn(TestData.Day(2026, 3, 5))!.Value;

            rate.Should().Be(650m);
            Money.Amount(8m, rate).Should().Be(5200m);
        }
    }
}
