using MongoDB.Bson;

namespace Project.Infrastructure.Persistence.Readers
{
    internal static class TimeEntryPipeline
    {
        public static BsonDocument[] LookupEmployee() =>
            new[]
            {
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", MongoContext.EmployeesCollection },
                    { "localField", "employeeId" },
                    { "foreignField", "_id" },
                    { "as", "emp" }
                }),
                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$emp" },
                    { "preserveNullAndEmptyArrays", true }
                })
            };

        public static BsonDocument[] LookupProject() =>
            new[]
            {
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", MongoContext.ProjectsCollection },
                    { "localField", "projectId" },
                    { "foreignField", "_id" },
                    { "as", "prj" }
                }),
                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$prj" },
                    { "preserveNullAndEmptyArrays", true }
                })
            };

        public static BsonDocument EffectiveRateStage() =>
            new BsonDocument("$addFields", new BsonDocument("effectiveRate",
                new BsonDocument("$arrayElemAt", new BsonArray
                {
                    new BsonDocument("$filter", new BsonDocument
                    {
                        { "input", new BsonDocument("$ifNull", new BsonArray { "$emp.rates", new BsonArray() }) },
                        { "as", "r" },
                        { "cond", new BsonDocument("$lte", new BsonArray { "$$r.from", "$date" }) }
                    }),
                    -1
                })));

        public static BsonDocument AmountStage() =>
            new BsonDocument("$addFields", new BsonDocument
            {
                { "rateValue", "$effectiveRate.value" },
                {
                    "amount", new BsonDocument("$divide", new BsonArray
                    {
                        new BsonDocument("$trunc", new BsonDocument("$add", new BsonArray
                        {
                            new BsonDocument("$multiply", new BsonArray
                            {
                                "$hours",
                                new BsonDocument("$ifNull", new BsonArray { "$effectiveRate.value", new BsonDecimal128(Decimal128.Zero) }),
                                100
                            }),
                            new BsonDecimal128(new Decimal128(0.5m))
                        })),
                        100
                    })
                }
            });
    }
}
