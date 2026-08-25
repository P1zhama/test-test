using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Project.Application.Common.Interfaces;
using Project.Domain.Common;
using Project.Domain.Entities;

namespace Project.Infrastructure.Persistence.Repositories
{
    public class TimeEntryRepository : ITimeEntryRepository
    {
        private readonly MongoContext _context;

        public TimeEntryRepository(MongoContext context)
        {
            _context = context;
        }

        public async Task<TimeEntry?> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            if (!MongoId.IsValid(id))
                return null;

            return await _context.TimeEntries
                .Find(e => e.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task InsertAsync(TimeEntry entry, CancellationToken cancellationToken) =>
            _context.TimeEntries.InsertOneAsync(entry, cancellationToken: cancellationToken);

        public async Task<bool> UpdateAsync(TimeEntry entry, int expectedVersion, CancellationToken cancellationToken)
        {
            var filter = Builders<TimeEntry>.Filter.And(
                Builders<TimeEntry>.Filter.Eq(e => e.Id, entry.Id),
                Builders<TimeEntry>.Filter.Eq(e => e.Version, expectedVersion));

            var update = Builders<TimeEntry>.Update
                .Set(e => e.EmployeeId, entry.EmployeeId)
                .Set(e => e.ProjectId, entry.ProjectId)
                .Set(e => e.Date, entry.Date)
                .Set(e => e.Hours, entry.Hours)
                .Set(e => e.Comment, entry.Comment)
                .Set(e => e.UpdatedAt, entry.UpdatedAt)
                .Set(e => e.UpdatedBy, entry.UpdatedBy)
                .Inc(e => e.Version, 1);

            var result = await _context.TimeEntries.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return result.MatchedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
        {
            if (!MongoId.IsValid(id))
                return false;

            var result = await _context.TimeEntries.DeleteOneAsync(e => e.Id == id, cancellationToken);
            return result.DeletedCount > 0;
        }

        public async Task<decimal> GetDayHoursAsync(
            string employeeId,
            DateTime date,
            string? excludeEntryId,
            CancellationToken cancellationToken)
        {
            if (!MongoId.IsValid(employeeId))
                return 0m;

            var day = DateUtc.Day(date);
            var match = new BsonDocument
            {
                { "employeeId", MongoId.Parse(employeeId) },
                { "date", new BsonDateTime(day) }
            };

            if (MongoId.IsValid(excludeEntryId))
                match.Add("_id", new BsonDocument("$ne", MongoId.Parse(excludeEntryId!)));

            var pipeline = new[]
            {
                new BsonDocument("$match", match),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "hours", new BsonDocument("$sum", "$hours") }
                })
            };

            var cursor = await _context.TimeEntries.AggregateAsync<BsonDocument>(
                pipeline,
                cancellationToken: cancellationToken);
            var result = await cursor.FirstOrDefaultAsync(cancellationToken);

            return result == null ? 0m : result["hours"].ToDecimal();
        }
    }
}
