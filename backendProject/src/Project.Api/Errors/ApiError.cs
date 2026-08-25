using System.Collections.Generic;

namespace Project.Api.Errors
{
    public class ApiError
    {
        public ApiError(string code, string message, IReadOnlyList<ApiFieldError>? errors = null)
        {
            Code = code;
            Message = message;
            Errors = errors;
        }

        public string Code { get; }

        public string Message { get; }

        public IReadOnlyList<ApiFieldError>? Errors { get; }
    }

    public class ApiFieldError
    {
        public ApiFieldError(string field, string message)
        {
            Field = field;
            Message = message;
        }

        public string Field { get; }

        public string Message { get; }
    }
}
