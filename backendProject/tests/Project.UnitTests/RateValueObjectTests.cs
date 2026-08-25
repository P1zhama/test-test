using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Project.Domain.ValueObjects;
using Xunit;

namespace Project.UnitTests
{
    public class RateValueObjectTests
    {
        [Fact(DisplayName = "Ставки сравниваются по значению, а не по ссылке")]
        public void Compared_by_value()
        {
            var first = Rate.Create(TestData.Day(2026, 1, 1), 500m);
            var same = Rate.Create(TestData.Day(2026, 1, 1), 500m);
            var other = Rate.Create(TestData.Day(2026, 1, 1), 600m);

            (first == same).Should().BeTrue();
            first.GetHashCode().Should().Be(same.GetHashCode());
            (first != other).Should().BeTrue();
            (first == null).Should().BeFalse();
        }

        [Fact(DisplayName = "Нулевая и отрицательная ставка не создаются")]
        public void Rejects_non_positive_value()
        {
            Action zero = () => Rate.Create(TestData.Day(2026, 1, 1), 0m);
            Action negative = () => Rate.Create(TestData.Day(2026, 1, 1), -100m);

            zero.Should().Throw<ArgumentOutOfRangeException>();
            negative.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact(DisplayName = "История ставок наружу отдаётся только для чтения и отсортированной")]
        public void Rate_history_is_read_only_and_sorted()
        {
            var employee = TestData.Ivanov();
            employee.SetRates(new[]
            {
                Rate.Create(TestData.Day(2026, 3, 1), 600m),
                Rate.Create(TestData.Day(2026, 1, 1), 500m)
            });

            employee.Rates.Select(r => r.Value).Should().ContainInOrder(500m, 600m);
            employee.Rates.Should().NotBeOfType<List<Rate>>();
        }
    }
}
