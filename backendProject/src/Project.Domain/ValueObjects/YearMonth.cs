using System;
using Project.Domain.Common;

namespace Project.Domain.ValueObjects
{
    public readonly struct YearMonth : IEquatable<YearMonth>
    {
        public YearMonth(int year, int month)
        {
            if (year < 2000 || year > 2100)
                throw new ArgumentOutOfRangeException(nameof(year), year, "Год должен быть в диапазоне 2000–2100.");
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), month, "Месяц должен быть в диапазоне 1–12.");

            Year = year;
            Month = month;
        }

        public int Year { get; }

        public int Month { get; }

        public DateTime Start => DateUtc.Day(Year, Month, 1);

        public DateTime EndExclusive => Start.AddMonths(1);

        public static YearMonth Of(DateTime date) => new YearMonth(date.Year, date.Month);

        public bool Contains(DateTime date) => date.Year == Year && date.Month == Month;

        public bool Equals(YearMonth other) => Year == other.Year && Month == other.Month;

        public override bool Equals(object? obj) => obj is YearMonth other && Equals(other);

        public override int GetHashCode() => Year * 100 + Month;

        public override string ToString() => $"{Month:00}.{Year}";
    }
}
