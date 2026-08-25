using System;
using Project.Domain.Common;
using Project.Domain.Entities;
using Project.Domain.ValueObjects;

using ProjectEntity = Project.Domain.Entities.Project;

namespace Project.UnitTests
{
    public static class TestData
    {
        public const string IvanovId = "660000000000000000000001";
        public const string PetrovaId = "660000000000000000000002";
        public const string Project001Id = "660000000000000000000011";
        public const string Project002Id = "660000000000000000000012";

        public static DateTime Day(int year, int month, int day) => DateUtc.Day(year, month, day);

        public static Employee Ivanov() =>
            Employee.Create(IvanovId, "Иванов И. И.", "Проектный", new[]
            {
                Rate.Create(Day(2026, 1, 1), 500m),
                Rate.Create(Day(2026, 3, 1), 600m)
            });

        public static Employee Petrova() =>
            Employee.Create(PetrovaId, "Петрова А. С.", "Проектный", new[]
            {
                Rate.Create(Day(2026, 2, 1), 700m)
            });

        public static ProjectEntity Project001() =>
            ProjectEntity.Create(Project001Id, "П-001", "Реконструкция цеха", 20000m,
                Day(2026, 1, 1), Day(2026, 3, 31));

        public static ProjectEntity Project002() =>
            ProjectEntity.Create(Project002Id, "П-002", "Инженерные сети", 5000m, Day(2026, 3, 1), null);
    }
}
