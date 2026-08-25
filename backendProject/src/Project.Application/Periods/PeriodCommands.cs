using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Project.Application.Common.Interfaces;
using Project.Domain;
using Project.Domain.Exceptions;
using Project.Domain.ValueObjects;

namespace Project.Application.Periods
{
    public class ClosedPeriodDto
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public DateTime ClosedAt { get; set; }
    }

    public class PeriodCommand : IRequest<ClosedPeriodDto?>
    {
        public int Year { get; set; }

        public int Month { get; set; }
    }

    public class ClosePeriodCommand : PeriodCommand
    {
    }

    public class OpenPeriodCommand : PeriodCommand
    {
    }

    public class GetClosedPeriodsQuery : IRequest<List<ClosedPeriodDto>>
    {
    }

    public class PeriodCommandValidator<T> : AbstractValidator<T>
        where T : PeriodCommand
    {
        public PeriodCommandValidator()
        {
            RuleFor(x => x.Year).InclusiveBetween(2000, 2100).WithMessage("Год должен быть в диапазоне 2000–2100.");
            RuleFor(x => x.Month).InclusiveBetween(1, 12).WithMessage("Месяц должен быть в диапазоне 1–12.");
        }
    }

    public class ClosePeriodCommandValidator : PeriodCommandValidator<ClosePeriodCommand>
    {
    }

    public class OpenPeriodCommandValidator : PeriodCommandValidator<OpenPeriodCommand>
    {
    }

    public class ClosePeriodCommandHandler : IRequestHandler<ClosePeriodCommand, ClosedPeriodDto?>
    {
        private readonly IClosedPeriodRepository _periods;
        private readonly IDateTimeProvider _clock;
        private readonly IIdGenerator _ids;

        public ClosePeriodCommandHandler(IClosedPeriodRepository periods, IDateTimeProvider clock, IIdGenerator ids)
        {
            _periods = periods;
            _clock = clock;
            _ids = ids;
        }

        public async Task<ClosedPeriodDto?> Handle(ClosePeriodCommand request, CancellationToken cancellationToken)
        {
            var period = new YearMonth(request.Year, request.Month);
            var closedAt = _clock.UtcNow;

            var closed = await _periods.CloseAsync(period, closedAt, _ids.NewId(), cancellationToken);
            if (!closed)
                throw new BusinessRuleException(ErrorCodes.PeriodAlreadyClosed, $"Период {period} уже закрыт.");

            return new ClosedPeriodDto { Year = period.Year, Month = period.Month, ClosedAt = closedAt };
        }
    }

    public class OpenPeriodCommandHandler : IRequestHandler<OpenPeriodCommand, ClosedPeriodDto?>
    {
        private readonly IClosedPeriodRepository _periods;

        public OpenPeriodCommandHandler(IClosedPeriodRepository periods)
        {
            _periods = periods;
        }

        public async Task<ClosedPeriodDto?> Handle(OpenPeriodCommand request, CancellationToken cancellationToken)
        {
            var period = new YearMonth(request.Year, request.Month);
            await _periods.OpenAsync(period, cancellationToken);
            return null;
        }
    }

    public class GetClosedPeriodsQueryHandler : IRequestHandler<GetClosedPeriodsQuery, List<ClosedPeriodDto>>
    {
        private readonly IClosedPeriodRepository _periods;

        public GetClosedPeriodsQueryHandler(IClosedPeriodRepository periods)
        {
            _periods = periods;
        }

        public async Task<List<ClosedPeriodDto>> Handle(GetClosedPeriodsQuery request, CancellationToken cancellationToken)
        {
            var periods = await _periods.GetAllAsync(cancellationToken);
            return periods
                .Select(p => new ClosedPeriodDto { Year = p.Year, Month = p.Month, ClosedAt = p.ClosedAt })
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .ToList();
        }
    }
}
