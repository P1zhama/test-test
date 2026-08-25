using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Project.Application.Common.Interfaces;
using Project.Domain.Entities;
using Project.Domain.ValueObjects;

namespace Project.Infrastructure.Persistence.Repositories
{
    public class ClosedPeriodRepository : IClosedPeriodRepository
    {
        private readonly MongoContext _context;

        public ClosedPeriodRepository(MongoContext context)
        {
            _context = context;
        }

        public async Task<bool> IsClosedAsync(YearMonth period, CancellationToken cancellationToken)
        {
            var count = await _context.ClosedPeriods.CountDocumentsAsync(
                p => p.Year == period.Year && p.Month == period.Month,
                cancellationToken: cancellationToken);

            return count > 0;
        }

        public async Task<IReadOnlyList<ClosedPeriod>> GetAllAsync(CancellationToken cancellationToken) =>
            await _context.ClosedPeriods
                .Find(FilterDefinition<ClosedPeriod>.Empty)
                .ToListAsync(cancellationToken);

        public async Task<bool> CloseAsync(
            YearMonth period,
            DateTime closedAt,
            string id,
            CancellationToken cancellationToken)
        {
            try
            {
                await _context.ClosedPeriods.InsertOneAsync(
                    ClosedPeriod.For(id, period, closedAt),
                    cancellationToken: cancellationToken);
                return true;
            }
            catch (MongoWriteException e) when (e.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                return false;
            }
        }

        public async Task<bool> OpenAsync(YearMonth period, CancellationToken cancellationToken)
        {
            var result = await _context.ClosedPeriods.DeleteOneAsync(
                p => p.Year == period.Year && p.Month == period.Month,
                cancellationToken);

            return result.DeletedCount > 0;
        }
    }
}
