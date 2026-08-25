using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Project.Application.Common.Interfaces;
using Project.Domain.Entities;

namespace Project.Application.TimeEntries.Commands.CreateTimeEntry
{
    public class CreateTimeEntryCommandHandler : IRequestHandler<CreateTimeEntryCommand, TimeEntryDto>
    {
        private readonly ITimeEntryRepository _timeEntries;
        private readonly TimeEntryRuleChecker _rules;
        private readonly IDateTimeProvider _clock;
        private readonly ICurrentUser _currentUser;
        private readonly IIdGenerator _ids;

        public CreateTimeEntryCommandHandler(
            ITimeEntryRepository timeEntries,
            TimeEntryRuleChecker rules,
            IDateTimeProvider clock,
            ICurrentUser currentUser,
            IIdGenerator ids)
        {
            _timeEntries = timeEntries;
            _rules = rules;
            _clock = clock;
            _currentUser = currentUser;
            _ids = ids;
        }

        public async Task<TimeEntryDto> Handle(CreateTimeEntryCommand request, CancellationToken cancellationToken)
        {
            var context = await _rules.EnsureCanSaveAsync(
                request.EmployeeId,
                request.ProjectId,
                request.Date,
                request.Hours,
                excludeEntryId: null,
                cancellationToken);

            var entry = TimeEntry.Create(
                _ids.NewId(),
                request.EmployeeId,
                request.ProjectId,
                request.Date,
                request.Hours,
                request.Comment,
                _clock.UtcNow,
                _currentUser.Name);

            await _timeEntries.InsertAsync(entry, cancellationToken);

            return TimeEntryMapper.ToDto(entry, context, entry.Version);
        }
    }
}
