using System;
using Project.Domain.Common;
using Project.Domain.Entities;
using Project.Domain.Exceptions;
using Project.Domain.ValueObjects;

using ProjectEntity = Project.Domain.Entities.Project;

namespace Project.Domain.Rules
{
    public static class TimeEntryRules
    {
        public static Rate EnsureRateOnDate(Employee employee, DateTime date)
        {
            var rate = employee.RateOn(date);
            if (rate == null)
            {
                throw new BusinessRuleException(
                    ErrorCodes.EmployeeRateNotFound,
                    $"У сотрудника «{employee.FullName}» нет часовой ставки, действующей на {DateUtc.Format(date)}.");
            }

            return rate;
        }

        public static void EnsureDateInProjectPeriod(ProjectEntity project, DateTime date)
        {
            if (project.IsDateInPeriod(date))
                return;

            throw new BusinessRuleException(
                ErrorCodes.DateOutOfProjectPeriod,
                $"Дата {DateUtc.Format(date)} вне периода проекта {project.Code} ({project.PeriodDescription()}).");
        }

        public static void EnsurePeriodOpen(bool isClosed, YearMonth period)
        {
            if (!isClosed)
                return;

            throw new BusinessRuleException(
                ErrorCodes.PeriodClosed,
                $"Период {period} закрыт бухгалтерией: записи табеля за этот месяц изменять нельзя.");
        }
    }
}
