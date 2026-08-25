using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Project.Application.Common.Interfaces;
using Project.Application.Common.Models;
using Project.Application.TimeEntries;
using Project.Application.TimeEntries.Queries.GetTimeEntries;
using Project.Domain.Rules;
using Project.Domain.ValueObjects;

namespace Project.Infrastructure.Persistence.Readers
{
    public class TimeEntryReader : ITimeEntryReader
    {
        private readonly MongoContext _context;

        public TimeEntryReader(MongoContext context)
        {
            _context = context;
        }

        public async Task<TimeEntriesPageDto> ListAsync(GetTimeEntriesQuery query, CancellationToken cancellationToken)
        {
            var period = new YearMonth(query.Year, query.Month);
            var match = BuildMatch(period, query.EmployeeId, query.ProjectId);
            var skip = (query.Page - 1) * query.PageSize;

            var itemStages = new List<BsonDocument>
            {
                new BsonDocument("$match", match),
                new BsonDocument("$sort", new BsonDocument { { "date", 1 }, { "_id", 1 } }),
                new BsonDocument("$skip", skip),
                new BsonDocument("$limit", query.PageSize)
            };
            itemStages.AddRange(TimeEntryPipeline.LookupEmployee());
            itemStages.AddRange(TimeEntryPipeline.LookupProject());
            itemStages.Add(TimeEntryPipeline.EffectiveRateStage());
            itemStages.Add(TimeEntryPipeline.AmountStage());
            itemStages.Add(new BsonDocument("$project", new BsonDocument
            {
                { "employeeId", 1 },
                { "projectId", 1 },
                { "date", 1 },
                { "hours", 1 },
                { "comment", 1 },
                { "version", 1 },
                { "rateValue", 1 },
                { "amount", 1 },
                { "employeeName", "$emp.fullName" },
                { "projectCode", "$prj.code" },
                { "projectName", "$prj.name" }
            }));

            var statsStages = new List<BsonDocument> { new BsonDocument("$match", match) };
            statsStages.AddRange(TimeEntryPipeline.LookupEmployee());
            statsStages.Add(TimeEntryPipeline.EffectiveRateStage());
            statsStages.Add(TimeEntryPipeline.AmountStage());
            statsStages.Add(new BsonDocument("$group", new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "count", new BsonDocument("$sum", 1) },
                { "hours", new BsonDocument("$sum", "$hours") },
                { "amount", new BsonDocument("$sum", "$amount") }
            }));

            var documents = await AggregateAsync(itemStages, cancellationToken);
            var stats = (await AggregateAsync(statsStages, cancellationToken)).FirstOrDefault();

            var dayTotals = await LoadDayTotalsAsync(documents, cancellationToken);

            var items = documents.Select(d => MapItem(d, dayTotals)).ToList();

            return new TimeEntriesPageDto
            {
                Page = new PagedResult<TimeEntryDto>(
                    items,
                    stats == null ? 0 : stats["count"].ToInt64(),
                    query.Page,
                    query.PageSize),
                TotalHours = stats == null ? 0m : ReadDecimal(stats, "hours"),
                TotalAmount = stats == null ? 0m : Money.Round(ReadDecimal(stats, "amount"))
            };
        }

        private static BsonDocument BuildMatch(YearMonth period, string? employeeId, string? projectId)
        {
            var match = new BsonDocument
            {
                {
                    "date", new BsonDocument
                    {
                        { "$gte", new BsonDateTime(period.Start) },
                        { "$lt", new BsonDateTime(period.EndExclusive) }
                    }
                }
            };

            if (MongoId.IsValid(employeeId))
                match.Add("employeeId", MongoId.Parse(employeeId!));

            if (MongoId.IsValid(projectId))
                match.Add("projectId", MongoId.Parse(projectId!));

            return match;
        }

        private async Task<Dictionary<string, decimal>> LoadDayTotalsAsync(
            IReadOnlyList<BsonDocument> documents,
            CancellationToken cancellationToken)
        {
            var totals = new Dictionary<string, decimal>();
            if (documents.Count == 0)
                return totals;

            var employeeIds = new BsonArray(documents.Select(d => d["employeeId"]).Distinct());
            var dates = new BsonArray(documents.Select(d => d["date"]).Distinct());

            var stages = new List<BsonDocument>
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { "employeeId", new BsonDocument("$in", employeeIds) },
                    { "date", new BsonDocument("$in", dates) }
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    {
                        "_id", new BsonDocument
                        {
                            { "employeeId", "$employeeId" },
                            { "date", "$date" }
                        }
                    },
                    { "hours", new BsonDocument("$sum", "$hours") }
                })
            };

            foreach (var document in await AggregateAsync(stages, cancellationToken))
            {
                var key = DayKey(document["_id"]["employeeId"], document["_id"]["date"]);
                totals[key] = ReadDecimal(document, "hours");
            }

            return totals;
        }

        private async Task<IReadOnlyList<BsonDocument>> AggregateAsync(
            IEnumerable<BsonDocument> stages,
            CancellationToken cancellationToken)
        {
            var pipeline = PipelineDefinition<Domain.Entities.TimeEntry, BsonDocument>.Create(stages);
            var cursor = await _context.TimeEntries.AggregateAsync(
                pipeline,
                new AggregateOptions { AllowDiskUse = true },
                cancellationToken);

            return await cursor.ToListAsync(cancellationToken);
        }

        private static TimeEntryDto MapItem(BsonDocument document, IReadOnlyDictionary<string, decimal> dayTotals)
        {
            var dayKey = DayKey(document["employeeId"], document["date"]);
            var hours = ReadDecimal(document, "hours");
            var dayTotal = dayTotals.TryGetValue(dayKey, out var total) ? total : hours;

            return new TimeEntryDto
            {
                Id = document["_id"].AsObjectId.ToString(),
                EmployeeId = document["employeeId"].AsObjectId.ToString(),
                EmployeeName = ReadString(document, "employeeName", "— сотрудник удалён —"),
                ProjectId = document["projectId"].AsObjectId.ToString(),
                ProjectCode = ReadString(document, "projectCode", "—"),
                ProjectName = ReadString(document, "projectName", "— проект удалён —"),
                Date = document["date"].ToUniversalTime(),
                Hours = hours,
                Rate = document.TryGetValue("rateValue", out var rate) && !rate.IsBsonNull
                    ? rate.ToDecimal()
                    : (decimal?)null,
                Amount = ReadDecimal(document, "amount"),
                Comment = ReadNullableString(document, "comment"),
                DayTotalHours = dayTotal,
                IsOvertime = DailyWorkloadPolicy.IsOvertime(dayTotal),
                Version = document.TryGetValue("version", out var version) ? version.ToInt32() : 1
            };
        }

        private static string DayKey(BsonValue employeeId, BsonValue date) =>
            employeeId.AsObjectId + "|" + date.ToUniversalTime().ToString("yyyy-MM-dd");

        private static decimal ReadDecimal(BsonDocument document, string name) =>
            document.TryGetValue(name, out var value) && value.IsNumeric ? value.ToDecimal() : 0m;

        private static string ReadString(BsonDocument document, string name, string fallback) =>
            document.TryGetValue(name, out var value) && !value.IsBsonNull
                ? value.AsString
                : fallback;

        private static string? ReadNullableString(BsonDocument document, string name) =>
            document.TryGetValue(name, out var value) && !value.IsBsonNull
                ? value.AsString
                : null;
    }
}
