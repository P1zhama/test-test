using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Project.Application.Common.Interfaces;
using Project.Application.Reports.Queries.GetProjectReport;
using Project.Domain.ValueObjects;
using Xunit;

namespace Project.UnitTests
{
    public class ProjectReportTests
    {
        private class StubReader : IProjectReportReader
        {
            private readonly IReadOnlyList<ProjectReportAggregate> _rows;

            public StubReader(params ProjectReportAggregate[] rows) => _rows = rows;

            public Task<IReadOnlyList<ProjectReportAggregate>> AggregateAsync(
                YearMonth period,
                CancellationToken cancellationToken) => Task.FromResult(_rows);
        }

        [Fact(DisplayName = "Отчёт за март: П-001 — 38 %, П-002 — 140 % с перерасходом, итого 22 ч / 14 600 ₽")]
        public async Task Builds_march_report()
        {
            var handler = new GetProjectReportQueryHandler(new StubReader(
                new ProjectReportAggregate
                {
                    ProjectId = TestData.Project001Id,
                    ProjectCode = "П-001",
                    ProjectName = "Реконструкция цеха",
                    Budget = 20000m,
                    Hours = 12m,
                    Amount = 7600m
                },
                new ProjectReportAggregate
                {
                    ProjectId = TestData.Project002Id,
                    ProjectCode = "П-002",
                    ProjectName = "Инженерные сети",
                    Budget = 5000m,
                    Hours = 10m,
                    Amount = 7000m
                }));

            var report = await handler.Handle(
                new GetProjectReportQuery { Year = 2026, Month = 3 },
                CancellationToken.None);

            report.Rows.Should().HaveCount(2);

            report.Rows[0].Percent.Should().Be(38m);
            report.Rows[0].IsOverspent.Should().BeFalse();
            report.Rows[0].IsAtRisk.Should().BeFalse();

            report.Rows[1].Percent.Should().Be(140m);
            report.Rows[1].IsOverspent.Should().BeTrue();
            report.Rows[1].IsAtRisk.Should().BeTrue();

            report.TotalHours.Should().Be(22m);
            report.TotalAmount.Should().Be(14600m);
        }

        [Fact(DisplayName = "Отчёт за февраль: П-001 — 8 ч, 4 000 ₽, освоено 20 %")]
        public async Task Builds_february_report()
        {
            var handler = new GetProjectReportQueryHandler(new StubReader(
                new ProjectReportAggregate
                {
                    ProjectId = TestData.Project001Id,
                    ProjectCode = "П-001",
                    ProjectName = "Реконструкция цеха",
                    Budget = 20000m,
                    Hours = 8m,
                    Amount = 4000m
                }));

            var report = await handler.Handle(
                new GetProjectReportQuery { Year = 2026, Month = 2 },
                CancellationToken.None);

            report.Rows.Should().ContainSingle();
            report.Rows[0].Hours.Should().Be(8m);
            report.Rows[0].Amount.Should().Be(4000m);
            report.Rows[0].Percent.Should().Be(20m);
        }

        [Fact(DisplayName = "Освоение больше 80 % помечается риском, больше 100 % — перерасходом")]
        public async Task Flags_risk_and_overspend()
        {
            var handler = new GetProjectReportQueryHandler(new StubReader(
                new ProjectReportAggregate
                {
                    ProjectId = TestData.Project001Id,
                    ProjectCode = "П-001",
                    ProjectName = "Реконструкция цеха",
                    Budget = 10000m,
                    Hours = 10m,
                    Amount = 8500m
                }));

            var report = await handler.Handle(
                new GetProjectReportQuery { Year = 2026, Month = 3 },
                CancellationToken.None);

            report.Rows[0].Percent.Should().Be(85m);
            report.Rows[0].IsAtRisk.Should().BeTrue();
            report.Rows[0].IsOverspent.Should().BeFalse();
        }
    }
}
