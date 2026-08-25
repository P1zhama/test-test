using System;
using Project.Domain.Common;
using Project.Domain.Exceptions;

namespace Project.Domain.Rules
{
    public static class DailyWorkloadPolicy
    {
        public const decimal MaxHoursPerDay = 24m;
        public const decimal OvertimeThreshold = 12m;

        public static bool IsOvertime(decimal dayTotalHours) => dayTotalHours > OvertimeThreshold;

        public static void EnsureDailyLimit(decimal otherEntriesHours, decimal newHours, DateTime date)
        {
            var total = otherEntriesHours + newHours;
            if (total <= MaxHoursPerDay)
                return;

            throw new BusinessRuleException(
                ErrorCodes.DailyHoursLimitExceeded,
                $"За {DateUtc.Format(date)} у сотрудника уже учтено {otherEntriesHours:0.##} ч; " +
                $"с этой записью ({newHours:0.##} ч) получится {total:0.##} ч " +
                $"при дневном лимите {MaxHoursPerDay:0.##} ч.");
        }
    }
}
