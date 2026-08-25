using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Project.Domain.Entities;
using Project.Domain.ValueObjects;
using Xunit;

using ProjectEntity = Project.Domain.Entities.Project;

namespace Project.UnitTests
{
    public class DomainConstructionTests
    {
        private static readonly Type[] PersistedTypes =
        {
            typeof(Employee),
            typeof(ProjectEntity),
            typeof(TimeEntry),
            typeof(ClosedPeriod),
            typeof(Rate)
        };

        [Fact(DisplayName = "У хранимых доменных типов нет публичных конструкторов")]
        public void Persisted_types_have_no_public_constructors()
        {
            foreach (var type in PersistedTypes)
            {
                type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                    .Should()
                    .BeEmpty(
                        "иначе Mongo-драйвер начнёт вызывать конструктор {0} при чтении документа " +
                        "и доменная валидация уронит запрос на историческом документе",
                        type.Name);
            }
        }

        [Fact(DisplayName = "Каждый хранимый тип создаётся статической фабрикой")]
        public void Persisted_types_expose_a_factory()
        {
            foreach (var type in PersistedTypes)
            {
                type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Any(m => m.ReturnType == type)
                    .Should()
                    .BeTrue("создание {0} должно проходить через домен, а не через конструктор", type.Name);
            }
        }
    }
}
