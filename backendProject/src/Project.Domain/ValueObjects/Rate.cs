using System;
using Project.Domain.Common;

namespace Project.Domain.ValueObjects
{
    public sealed class Rate : IEquatable<Rate>
    {
        private Rate(DateTime from, decimal value)
        {
            From = DateUtc.Day(from);
            Value = value;
        }

        public DateTime From { get; private set; }

        public decimal Value { get; private set; }

        public static Rate Create(DateTime from, decimal value)
        {
            if (value <= 0m)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Ставка должна быть положительной.");

            return new Rate(from, value);
        }

        public bool Equals(Rate? other) =>
            !(other is null) && From == other.From && Value == other.Value;

        public override bool Equals(object? obj) => Equals(obj as Rate);

        public override int GetHashCode() => HashCode.Combine(From, Value);

        public override string ToString() => $"{Value} с {DateUtc.Format(From)}";

        public static bool operator ==(Rate? left, Rate? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (left is null || right is null)
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(Rate? left, Rate? right) => !(left == right);
    }
}
