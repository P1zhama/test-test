using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace Project.Application.Common.Exceptions
{
    public class ValidationError
    {
        public ValidationError(string field, string message)
        {
            Field = field;
            Message = message;
        }

        public string Field { get; }

        public string Message { get; }
    }

    public class ValidationException : Exception
    {
        public ValidationException(IEnumerable<ValidationFailure> failures)
            : base("Запрос содержит некорректные данные.")
        {
            Errors = failures
                .Select(f => new ValidationError(ToCamelCase(f.PropertyName), f.ErrorMessage))
                .ToList();
        }

        public IReadOnlyList<ValidationError> Errors { get; }

        private static string ToCamelCase(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }
    }
}
