using System;

namespace Project.Application.Common.Interfaces
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }

    public interface ICurrentUser
    {
        string Name { get; }
    }

    public interface IIdGenerator
    {
        string NewId();
    }
}
