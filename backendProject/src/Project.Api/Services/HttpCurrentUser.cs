using Microsoft.AspNetCore.Http;
using Project.Application.Common.Interfaces;

namespace Project.Api.Services
{
    public class HttpCurrentUser : ICurrentUser
    {
        public const string HeaderName = "X-User";

        private readonly IHttpContextAccessor _accessor;

        public HttpCurrentUser(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public string Name
        {
            get
            {
                var header = _accessor.HttpContext?.Request.Headers[HeaderName].ToString();
                return string.IsNullOrWhiteSpace(header) ? "system" : header!;
            }
        }
    }
}
