using System;
using FluentAssertions;
using Project.Application.TimeEntries.Commands.CreateTimeEntry;
using Project.Domain;
using Project.Domain.Exceptions;
using Project.Domain.Rules;
using Xunit;

namespace Project.UnitTests
{
    public class HoursRulesTests
    {
        [Theory(DisplayName = "Допустимые значения часов — границы диапазона")]
        [InlineData(0.5)]
        [InlineData(24)]
        public void Accepts_valid_hours(double hours)
        {
            HoursRules.IsValid((decimal)hours).Should().BeTrue();
        }

        [Theory(DisplayName = "Сценарий 6: 0 и 3,7 часа отклоняются, как и выход за 24")]
        [InlineData(0)]
        [InlineData(3.7)]
        [InlineData(24.5)]
        public void Rejects_invalid_hours(double hours)
        {
            HoursRules.IsValid((decimal)hours).Should().BeFalse();

            Action act = () => HoursRules.Ensure((decimal)hours);
            act.Should().Throw<BusinessRuleException>().Where(e => e.Code == ErrorCodes.ValidationError);
        }

        [Fact(DisplayName = "Валидатор команды сообщает про кратность 0,5 в поле hours")]
        public void Validator_reports_step_error_on_hours_field()
        {
            var validator = new CreateTimeEntryCommandValidator();

            var result = validator.Validate(new CreateTimeEntryCommand
            {
                EmployeeId = TestData.IvanovId,
                ProjectId = TestData.Project001Id,
                Date = TestData.Day(2026, 3, 5),
                Hours = 3.7m
            });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.PropertyName.Should().Be("Hours");
            result.Errors[0].ErrorMessage.Should().Contain("0,5");
        }

        [Fact(DisplayName = "Валидатор требует выбрать сотрудника и проект")]
        public void Validator_requires_employee_and_project()
        {
            var validator = new CreateTimeEntryCommandValidator();

            var result = validator.Validate(new CreateTimeEntryCommand { Hours = 8m, Date = TestData.Day(2026, 3, 5) });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(2);
        }
    }
}
