using System;
using FluentAssertions;
using Project.Domain;
using Project.Domain.Exceptions;
using Project.Domain.Rules;
using Xunit;

namespace Project.UnitTests
{
    public class ProjectPeriodTests
    {
        [Theory(DisplayName = "Границы периода проекта включаются, соседние дни — нет")]
        [InlineData(2025, 12, 31, false)]
        [InlineData(2026, 1, 1, true)]
        [InlineData(2026, 2, 20, true)]
        [InlineData(2026, 3, 31, true)]
        [InlineData(2026, 4, 1, false)]
        public void Checks_project_period_bounds(int year, int month, int day, bool expected)
        {
            TestData.Project001().IsDateInPeriod(TestData.Day(year, month, day)).Should().Be(expected);
        }

        [Fact(DisplayName = "У бессрочного проекта верхней границы нет")]
        public void Open_ended_project_has_no_upper_bound()
        {
            var project = TestData.Project002();

            project.IsDateInPeriod(TestData.Day(2026, 3, 1)).Should().BeTrue();
            project.IsDateInPeriod(TestData.Day(2030, 12, 31)).Should().BeTrue();
        }

        [Fact(DisplayName = "Сценарий 4: запись раньше начала проекта отклоняется")]
        public void Throws_before_project_start()
        {
            Action act = () => TimeEntryRules.EnsureDateInProjectPeriod(TestData.Project002(), TestData.Day(2026, 2, 20));

            act.Should().Throw<BusinessRuleException>()
                .Where(e => e.Code == ErrorCodes.DateOutOfProjectPeriod)
                .Where(e => e.Message.Contains("П-002"));
        }
    }
}
