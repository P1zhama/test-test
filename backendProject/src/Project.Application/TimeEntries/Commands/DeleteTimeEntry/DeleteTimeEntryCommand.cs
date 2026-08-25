using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Project.Application.Common.Interfaces;
using Project.Domain;
using Project.Domain.Exceptions;

namespace Project.Application.TimeEntries.Commands.DeleteTimeEntry
{
    public class DeleteTimeEntryCommand : IRequest<Unit>
    {
        public string Id { get; set; } = string.Empty;
    }

    public class DeleteTimeEntryCommandValidator : AbstractValidator<DeleteTimeEntryCommand>
    {
        public DeleteTimeEntryCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Не указан идентификатор записи.");
        }
    }

    public class DeleteTimeEntryCommandHandler : IRequestHandler<DeleteTimeEntryCommand, Unit>
    {
        private readonly ITimeEntryRepository _timeEntries;
        private readonly TimeEntryRuleChecker _rules;

        public DeleteTimeEntryCommandHandler(ITimeEntryRepository timeEntries, TimeEntryRuleChecker rules)
        {
            _timeEntries = timeEntries;
            _rules = rules;
        }

        public async Task<Unit> Handle(DeleteTimeEntryCommand request, CancellationToken cancellationToken)
        {
            var entry = await _timeEntries.GetByIdAsync(request.Id, cancellationToken);
            if (entry == null)
                throw new BusinessRuleException(ErrorCodes.NotFound, "Запись табеля не найдена.");

            await _rules.EnsurePeriodOpenAsync(entry.Date, cancellationToken);

            await _timeEntries.DeleteAsync(entry.Id, cancellationToken);
            return Unit.Value;
        }
    }
}
