using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Project.Application.Common.Interfaces;
using Project.Application.Reports.Queries.GetProjectReport;
using Project.Domain.Entities;
using Project.Domain.ValueObjects;

namespace Project.Infrastructure.Persistence.Readers
{
    public class ProjectReportReader : IProjectReportReader
    {
        private readonly MongoContext _context;

        public ProjectReportReader(MongoContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<ProjectReportAggregate>> AggregateAsync(
            YearMonth period,
            CancellationToken cancellationToken)
        {
            var stages = new List<BsonDocument>
            {
                new BsonDocument("$match", new BsonDocument("date", new BsonDocument
                {
                    { "$gte", new BsonDateTime(period.Start) },
                    { "$lt", new BsonDateTime(period.EndExclusive) }
                }))
            };

            stages.AddRange(TimeEntryPipeline.LookupEmployee());
            stages.Add(TimeEntryPipeline.EffectiveRateStage());
            stages.Add(TimeEntryPipeline.AmountStage());

            stages.Add(new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$projectId" },
                { "hours", new BsonDocument("$sum", "$hours") },
                { "amount", new BsonDocument("$sum", "$amount") }
            }));

            stages.Add(new BsonDocument("$lookup", new BsonDocument
            {
                { "from", MongoContext.ProjectsCollection },
                { "localField", "_id" },
                { "foreignField", "_id" },
                { "as", "prj" }
            }));
            stages.Add(new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$prj" },
                { "preserveNullAndEmptyArrays", true }
            }));

            stages.Add(new BsonDocument("$project", new BsonDocument
            {
                { "hours", 1 },
                { "amount", 1 },
                { "projectCode", "$prj.code" },
                { "projectName", "$prj.name" },
                { "budget", new BsonDocument("$ifNull", new BsonArray { "$prj.budget", new BsonDecimal128(Decimal128.Zero) }) }
            }));

            stages.Add(new BsonDocument("$sort", new BsonDocument("projectCode", 1)));

            var pipeline = PipelineDefinition<TimeEntry, BsonDocument>.Create(stages);
            var cursor = await _context.TimeEntries.AggregateAsync(
                pipeline,
                new AggregateOptions { AllowDiskUse = true },
                cancellationToken);

            var documents = await cursor.ToListAsync(cancellationToken);

            return documents
                .Select(d => new ProjectReportAggregate
                {
                    ProjectId = d["_id"].AsObjectId.ToString(),
                    ProjectCode = ReadString(d, "projectCode", "—"),
                    ProjectName = ReadString(d, "projectName", "— проект удалён —"),
                    Budget = ReadDecimal(d, "budget"),
                    Hours = ReadDecimal(d, "hours"),
                    Amount = ReadDecimal(d, "amount")
                })
                .ToList();
        }

        private static decimal ReadDecimal(BsonDocument document, string name) =>
            document.TryGetValue(name, out var value) && value.IsNumeric ? value.ToDecimal() : 0m;

        private static string ReadString(BsonDocument document, string name, string fallback) =>
            document.TryGetValue(name, out var value) && !value.IsBsonNull ? value.AsString : fallback;
    }
}
