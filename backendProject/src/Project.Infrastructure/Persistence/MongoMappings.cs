using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using Project.Domain.Entities;
using Project.Domain.ValueObjects;

using ProjectEntity = Project.Domain.Entities.Project;

namespace Project.Infrastructure.Persistence
{
    public static class MongoMappings
    {
        private static readonly object Gate = new object();
        private static bool _registered;

        public static void Register()
        {
            lock (Gate)
            {
                if (_registered)
                    return;

                var conventions = new ConventionPack
                {
                    new CamelCaseElementNameConvention(),
                    new IgnoreExtraElementsConvention(true)
                };
                ConventionRegistry.Register(
                    "timetracking",
                    conventions,
                    type => type.FullName != null && type.FullName.StartsWith("Project.Domain", StringComparison.Ordinal));

                BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));
                BsonSerializer.RegisterSerializer(
                    new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));

                BsonSerializer.RegisterSerializer(new DateTimeSerializer(DateTimeKind.Utc));
                BsonSerializer.RegisterSerializer(
                    new NullableSerializer<DateTime>(new DateTimeSerializer(DateTimeKind.Utc)));

                BsonClassMap.RegisterClassMap<Employee>(map =>
                {
                    map.AutoMap();
                    map.MapIdMember(e => e.Id).SetSerializer(new StringSerializer(BsonType.ObjectId));
                    map.MapField("_rates").SetElementName("rates");
                });

                BsonClassMap.RegisterClassMap<Rate>(map => map.AutoMap());

                BsonClassMap.RegisterClassMap<ProjectEntity>(map =>
                {
                    map.AutoMap();
                    map.MapIdMember(p => p.Id).SetSerializer(new StringSerializer(BsonType.ObjectId));
                });

                BsonClassMap.RegisterClassMap<TimeEntry>(map =>
                {
                    map.AutoMap();
                    map.MapIdMember(e => e.Id).SetSerializer(new StringSerializer(BsonType.ObjectId));
                    map.MapMember(e => e.EmployeeId).SetSerializer(new StringSerializer(BsonType.ObjectId));
                    map.MapMember(e => e.ProjectId).SetSerializer(new StringSerializer(BsonType.ObjectId));
                });

                BsonClassMap.RegisterClassMap<ClosedPeriod>(map =>
                {
                    map.AutoMap();
                    map.MapIdMember(p => p.Id).SetSerializer(new StringSerializer(BsonType.ObjectId));
                });

                _registered = true;
            }
        }
    }
}
