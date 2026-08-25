using System;
using System.Collections.Generic;
using System.Linq;
using Project.Domain.Common;
using Project.Domain.ValueObjects;

namespace Project.Domain.Entities
{
    public class Employee
    {
        private static readonly IReadOnlyList<Rate> NoRates = new Rate[0];

        private List<Rate>? _rates;

        private Employee(string id, string fullName, string department, IEnumerable<Rate> rates)
        {
            Id = id;
            FullName = fullName;
            Department = department;
            _rates = Sorted(rates);
        }

        public string Id { get; private set; }

        public string FullName { get; private set; }

        public string Department { get; private set; }

        public IReadOnlyList<Rate> Rates => _rates?.AsReadOnly() ?? NoRates;

        public static Employee Create(string id, string fullName, string department, IEnumerable<Rate> rates) =>
            new Employee(id, fullName, department, rates);

        public Rate? RateOn(DateTime date)
        {
            var day = DateUtc.Day(date);

            Rate? effective = null;
            foreach (var rate in Rates)
            {
                if (rate.From > day)
                    continue;
                if (effective == null || rate.From > effective.From)
                    effective = rate;
            }

            return effective;
        }

        public void SetRates(IEnumerable<Rate> rates) => _rates = Sorted(rates);

        private static List<Rate> Sorted(IEnumerable<Rate> rates) => rates.OrderBy(r => r.From).ToList();
    }
}
