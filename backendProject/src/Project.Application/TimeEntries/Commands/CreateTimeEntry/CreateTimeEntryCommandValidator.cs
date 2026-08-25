using System;
using FluentValidation;
using Project.Domain.Rules;

namespace Project.Application.TimeEntries.Commands.CreateTimeEntry
{
    public class CreateTimeEntryCommandValidator : AbstractValidator<CreateTimeEntryCommand>
    {
        public CreateTimeEntryCommandValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("Выберите сотрудника.");
            RuleFor(x => x.ProjectId).NotEmpty().WithMessage("Выберите проект.");
            RuleFor(x => x.Date).NotEqual(default(DateTime)).WithMessage("Укажите дату.");
            RuleFor(x => x.Hours).SetValidator(new HoursValidator());
            RuleFor(x => x.Comment).MaximumLength(500).WithMessage("Комментарий не длиннее 500 символов.");
        }
    }

    public class HoursValidator : AbstractValidator<decimal>
    {
        public HoursValidator()
        {
            RuleFor(x => x)
                .GreaterThan(0m).WithMessage("Часы должны быть больше 0.")
                .LessThanOrEqualTo(HoursRules.MaxPerEntry)
                .WithMessage("За одну запись нельзя указать больше 24 ч.")
                .Must(h => h % HoursRules.Step == 0m)
                .WithMessage("Часы должны быть кратны 0,5 (например 0,5; 1; 7,5).");
        }
    }
}
