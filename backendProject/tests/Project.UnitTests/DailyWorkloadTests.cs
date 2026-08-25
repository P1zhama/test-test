using System;
using System.Globalization;
using FluentAssertions;
using Project.Domain;
using Project.Domain.Exceptions;
using Project.Domain.Rules;
using Xunit;

namespace Project.UnitTests
{
    public class DailyWorkloadTests
    {
        [Fact(DisplayName = "Ровно 24 часа за день — допустимо")]
        public void Allows_exactly_24_hours()
        {
            Action act = () => DailyWorkloadPolicy.EnsureDailyLimit(16m, 8m, TestData.Day(2026, 3, 6));

            act.Should().NotThrow();
        }

        [Fact(DisplayName = "Сценарий 3: 20 ч + 6 ч = 26 ч за день — отказ с понятным текстом")]
        public void Rejects_more_than_24_hours_per_day()
        {
            Action act = () => DailyWorkloadPolicy.EnsureDailyLimit(20m, 6m, TestData.Day(2026, 3, 6));

            act.Should().Throw<BusinessRuleException>()
                .Where(e => e.Code == ErrorCodes.DailyHoursLimitExceeded)
                .Where(e => e.Message.Contains("06.03.2026"))
                .Where(e => e.Message.Contains("26"));
        }

        [Fact(DisplayName = "Лимит считается по всем проектам сотрудника вместе")]
        public void Limit_is_shared_across_projects()
        {
            Action act = () => DailyWorkloadPolicy.EnsureDailyLimit(12m + 8m, 5m, TestData.Day(2026, 3, 6));

            act.Should().Throw<BusinessRuleException>();
        }

        [Theory(DisplayName = "Переработкой считается день строго больше 12 часов")]
        [InlineData(8, false)]
        [InlineData(12, false)]
        [InlineData(12.5, true)]
        [InlineData(20, true)]
        public void Marks_overtime_above_12_hours(double dayHours, bool expected)
        {
            DailyWorkloadPolicy.IsOvertime((decimal)dayHours).Should().Be(expected);
        }

        [Fact(DisplayName = "Дробные часы в тексте ошибки форматируются по-русски")]
        public void Formats_fractional_hours_in_russian()
        {
            var previous = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
            try
            {
                Action act = () => DailyWorkloadPolicy.EnsureDailyLimit(20.5m, 6m, TestData.Day(2026, 3, 6));

                act.Should().Throw<BusinessRuleException>()
                    .Where(e => e.Message.Contains("20,5"))
                    .Where(e => e.Message.Contains("26,5"));
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

        [Fact(DisplayName = "Сценарий 2: 20 ч за день сохраняются, но день помечен переработкой")]
        public void Twenty_hours_is_saved_and_flagged()
        {
            Action act = () => DailyWorkloadPolicy.EnsureDailyLimit(0m, 20m, TestData.Day(2026, 3, 6));

            act.Should().NotThrow();
            DailyWorkloadPolicy.IsOvertime(20m).Should().BeTrue();
        }
    }
}
