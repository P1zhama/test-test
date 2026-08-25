using Project.Domain.Exceptions;

namespace Project.Domain.Rules
{
    public static class HoursRules
    {
        public const decimal Step = 0.5m;
        public const decimal MaxPerEntry = 24m;

        public static bool IsValid(decimal hours) =>
            hours > 0m && hours <= MaxPerEntry && hours % Step == 0m;

        public static void Ensure(decimal hours)
        {
            if (!IsValid(hours))
            {
                throw new BusinessRuleException(
                    ErrorCodes.ValidationError,
                    "Часы должны быть больше 0, кратны 0,5 и не превышать 24 за одну запись. " +
                    $"Получено: {hours:0.##}.");
            }
        }
    }
}
