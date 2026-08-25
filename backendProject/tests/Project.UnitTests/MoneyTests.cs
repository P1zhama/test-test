using FluentAssertions;
using Project.Domain.Rules;
using Xunit;

namespace Project.UnitTests
{
    public class MoneyTests
    {
        [Theory(DisplayName = "Стоимость записи из приёмочных данных ТЗ")]
        [InlineData(8, 500, 4000)]
        [InlineData(8, 600, 4800)]
        [InlineData(10, 700, 7000)]
        public void Calculates_amount_from_acceptance_data(decimal hours, decimal rate, decimal expected)
        {
            Money.Amount(hours, rate).Should().Be(expected);
        }

        [Fact(DisplayName = "Округление до копеек — арифметическое, половина уходит от нуля")]
        public void Rounds_half_away_from_zero()
        {
            Money.Round(166.665m).Should().Be(166.67m);
            Money.Round(2.344m).Should().Be(2.34m);
            Money.Amount(7.5m, 333.33m).Should().Be(2499.98m);
        }

        [Theory(DisplayName = "Процент освоения бюджета из приёмочных данных ТЗ")]
        [InlineData(7600, 20000, 38)]
        [InlineData(7000, 5000, 140)]
        public void Calculates_budget_percent(decimal amount, decimal budget, decimal expected)
        {
            Money.Percent(amount, budget).Should().Be(expected);
        }

        [Fact(DisplayName = "Нулевой бюджет не делим — процент неизвестен")]
        public void Returns_null_percent_for_zero_budget()
        {
            Money.Percent(1000m, 0m).Should().BeNull();
        }
    }
}
