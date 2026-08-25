using System;
using Project.Domain.Common;

namespace Project.Domain.Entities
{
    public class Project
    {
        private Project(string id, string code, string name, decimal budget, DateTime startDate, DateTime? endDate)
        {
            Id = id;
            Code = code;
            Name = name;
            Budget = budget;
            StartDate = DateUtc.Day(startDate);
            EndDate = endDate.HasValue ? DateUtc.Day(endDate.Value) : (DateTime?)null;
        }

        public string Id { get; private set; }

        public string Code { get; private set; }

        public string Name { get; private set; }

        public decimal Budget { get; private set; }

        public DateTime StartDate { get; private set; }

        public DateTime? EndDate { get; private set; }

        public static Project Create(
            string id,
            string code,
            string name,
            decimal budget,
            DateTime startDate,
            DateTime? endDate)
        {
            if (endDate.HasValue && DateUtc.Day(endDate.Value) < DateUtc.Day(startDate))
                throw new ArgumentException("Дата окончания проекта не может быть раньше даты начала.", nameof(endDate));

            return new Project(id, code, name, budget, startDate, endDate);
        }

        public bool IsDateInPeriod(DateTime date)
        {
            var day = DateUtc.Day(date);
            if (day < StartDate)
                return false;
            return !EndDate.HasValue || day <= EndDate.Value;
        }

        public string PeriodDescription() =>
            EndDate.HasValue
                ? $"с {DateUtc.Format(StartDate)} по {DateUtc.Format(EndDate.Value)}"
                : $"с {DateUtc.Format(StartDate)}, без даты окончания";
    }
}
