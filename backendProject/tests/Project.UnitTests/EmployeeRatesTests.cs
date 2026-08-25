using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Project.Application.Employees;
using Xunit;

namespace Project.UnitTests
{
    public class EmployeeRatesTests
    {
        [Fact(DisplayName = "Сценарий 8: ставка меняется задним числом")]
        public async Task Updates_rates()
        {
            var fakes = new Fakes();
            var handler = new UpdateEmployeeRatesCommandHandler(fakes.Employees.Object);

            var result = await handler.Handle(new UpdateEmployeeRatesCommand
            {
                Id = TestData.IvanovId,
                Rates = new List<RateDto>
                {
                    new RateDto { From = TestData.Day(2026, 1, 1), Value = 500m },
                    new RateDto { From = TestData.Day(2026, 3, 1), Value = 650m }
                }
            }, CancellationToken.None);

            result.Rates.Should().HaveCount(2);
            result.Rates[1].Value.Should().Be(650m);
        }
    }
}
