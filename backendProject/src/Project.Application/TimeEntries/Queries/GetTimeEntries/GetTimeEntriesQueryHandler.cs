using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Project.Application.Common.Interfaces;

namespace Project.Application.TimeEntries.Queries.GetTimeEntries
{
    public class GetTimeEntriesQueryHandler : IRequestHandler<GetTimeEntriesQuery, TimeEntriesPageDto>
    {
        private readonly ITimeEntryReader _reader;

        public GetTimeEntriesQueryHandler(ITimeEntryReader reader)
        {
            _reader = reader;
        }

        public Task<TimeEntriesPageDto> Handle(GetTimeEntriesQuery request, CancellationToken cancellationToken) =>
            _reader.ListAsync(request, cancellationToken);
    }

    public class GetTimeEntriesQueryValidator : AbstractValidator<GetTimeEntriesQuery>
    {
        public const int MaxPageSize = 200;

        public GetTimeEntriesQueryValidator()
        {
            RuleFor(x => x.Year).InclusiveBetween(2000, 2100).WithMessage("Год должен быть в диапазоне 2000–2100.");
            RuleFor(x => x.Month).InclusiveBetween(1, 12).WithMessage("Месяц должен быть в диапазоне 1–12.");
            RuleFor(x => x.Page).GreaterThan(0).WithMessage("Номер страницы начинается с 1.");
            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, MaxPageSize)
                .WithMessage($"Размер страницы — от 1 до {MaxPageSize}.");
        }
    }
}
