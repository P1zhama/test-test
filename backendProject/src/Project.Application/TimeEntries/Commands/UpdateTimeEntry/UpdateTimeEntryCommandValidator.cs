using System;
using FluentValidation;
using Project.Application.TimeEntries.Commands.CreateTimeEntry;

namespace Project.Application.TimeEntries.Commands.UpdateTimeEntry
{
    public class UpdateTimeEntryCommandValidator : AbstractValidator<UpdateTimeEntryCommand>
    {
        public UpdateTimeEntryCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Не указан идентификатор записи.");
            RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("Выберите сотрудника.");
            RuleFor(x => x.ProjectId).NotEmpty().WithMessage("Выберите проект.");
            RuleFor(x => x.Date).NotEqual(default(DateTime)).WithMessage("Укажите дату.");
            RuleFor(x => x.Hours).SetValidator(new HoursValidator());
            RuleFor(x => x.Comment).MaximumLength(500).WithMessage("Комментарий не длиннее 500 символов.");
            RuleFor(x => x.Version).GreaterThan(0).WithMessage("Не передана версия записи.");
        }
    }
}
