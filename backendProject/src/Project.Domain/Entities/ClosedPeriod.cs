using System;
using Project.Domain.ValueObjects;

namespace Project.Domain.Entities
{
    public class ClosedPeriod
    {
        private ClosedPeriod(string id, int year, int month, DateTime closedAt)
        {
            Id = id;
            Year = year;
            Month = month;
            ClosedAt = closedAt;
        }

        public string Id { get; private set; }

        public int Year { get; private set; }

        public int Month { get; private set; }

        public DateTime ClosedAt { get; private set; }

        public YearMonth Period => new YearMonth(Year, Month);

        public static ClosedPeriod For(string id, YearMonth period, DateTime closedAt) =>
            new ClosedPeriod(id, period.Year, period.Month, closedAt);
    }
}
