using System;
using Project.Application.Common.Interfaces;
using Project.Infrastructure.Persistence;

namespace Project.Infrastructure.Services
{
    public class SystemDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    public class ObjectIdGenerator : IIdGenerator
    {
        public string NewId() => MongoId.NewId();
    }
}
