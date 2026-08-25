using System;

namespace Project.Domain.Rules
{
    public static class Money
    {
        public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

        public static decimal Amount(decimal hours, decimal rate) => Round(hours * rate);

        public static decimal? Percent(decimal amount, decimal budget) =>
            budget == 0m ? (decimal?)null : Round(amount / budget * 100m);
    }
}
