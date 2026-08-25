using System;

namespace Project.Domain.Common
{
    public static class DateUtc
    {
        public static DateTime Day(DateTime value) =>
            new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime Day(int year, int month, int day) =>
            new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);

        public static string Format(DateTime value) => value.ToString("dd.MM.yyyy");
    }
}
