using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Project.Application.Common.Interfaces;
using Project.Domain;
using Project.Domain.Exceptions;

namespace Project.Application.TimeEntries.Commands.UpdateTimeEntry
{
    public class UpdateTimeEntryCommandHandler : IRequestHandler<UpdateTimeEntryCommand, TimeEntryDto>
    {
        private readonly ITimeEntryRepository _timeEntries;
        private readonly TimeEntryRuleChecker _rules;
        private readonly IDateTimeProvider _clock;
        private readonly ICurrentUser _currentUser;

        public UpdateTimeEntryCommandHandler(
            ITimeEntryRepository timeEntries,
            TimeEntryRuleChecker rules,
            IDateTimeProvider clock,
            ICurrentUser currentUser)
        {
            _timeEntries = timeEntries;
            _rules = rules;
            _clock = clock;
            _currentUser = currentUser;
        }

        public async Task<TimeEntryDto> Handle(UpdateTimeEntryCommand request, CancellationToken cancellationToken)
        {
            var entry = await _timeEntries.GetByIdAsync(request.Id, cancellationToken);
            if (entry == null)
                throw new BusinessRuleException(ErrorCodes.NotFound, "Запись табеля не найдена.");

            await _rules.EnsurePeriodOpenAsync(entry.Date, cancellationToken);

            var context = await _rules.EnsureCanSaveAsync(
                request.EmployeeId,
                request.ProjectId,
                request.Date,
                request.Hours,
                excludeEntryId: entry.Id,
                cancellationToken);

            entry.Update(
                request.EmployeeId,
                request.ProjectId,
                request.Date,
                request.Hours,
                request.Comment,
                _clock.UtcNow,
                _currentUser.Name);

            var updated = await _timeEntries.UpdateAsync(entry, request.Version, cancellationToken);
            if (!updated)
            {
                throw new BusinessRuleException(
                    ErrorCodes.ConcurrencyConflict,
                    "Запись уже изменена другим пользователем. Обновите страницу и повторите редактирование.");
            }

            return TimeEntryMapper.ToDto(entry, context, request.Version + 1);
        }
    }
}
