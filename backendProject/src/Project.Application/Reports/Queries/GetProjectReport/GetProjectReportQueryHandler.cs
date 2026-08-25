using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Project.Application.Common.Interfaces;
using Project.Domain.Rules;
using Project.Domain.ValueObjects;

namespace Project.Application.Reports.Queries.GetProjectReport
{
    public class GetProjectReportQueryHandler : IRequestHandler<GetProjectReportQuery, ProjectReportDto>
    {
        public const decimal RiskThresholdPercent = 80m;

        public const decimal OverspentThresholdPercent = 100m;

        private readonly IProjectReportReader _reader;

        public GetProjectReportQueryHandler(IProjectReportReader reader)
        {
            _reader = reader;
        }

        public async Task<ProjectReportDto> Handle(GetProjectReportQuery request, CancellationToken cancellationToken)
        {
            var period = new YearMonth(request.Year, request.Month);
            var aggregates = await _reader.AggregateAsync(period, cancellationToken);

            var rows = aggregates
                .Select(a =>
                {
                    var percent = Money.Percent(a.Amount, a.Budget);
                    return new ProjectReportRowDto
                    {
                        ProjectId = a.ProjectId,
                        ProjectCode = a.ProjectCode,
                        ProjectName = a.ProjectName,
                        Hours = a.Hours,
                        Amount = a.Amount,
                        Budget = a.Budget,
                        Percent = percent,
                        IsOverspent = percent > OverspentThresholdPercent,
                        IsAtRisk = percent > RiskThresholdPercent
                    };
                })
                .OrderBy(r => r.ProjectCode)
                .ToList();

            return new ProjectReportDto
            {
                Year = period.Year,
                Month = period.Month,
                Rows = rows,
                TotalHours = rows.Sum(r => r.Hours),
                TotalAmount = Money.Round(rows.Sum(r => r.Amount))
            };
        }
    }
}
