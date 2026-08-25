using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Project.Application.Common.Interfaces;
using Project.Domain.Entities;

namespace Project.Infrastructure.Persistence.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly MongoContext _context;

        public EmployeeRepository(MongoContext context)
        {
            _context = context;
        }

        public async Task<Employee?> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            if (!MongoId.IsValid(id))
                return null;

            return await _context.Employees
                .Find(e => e.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken) =>
            await _context.Employees
                .Find(FilterDefinition<Employee>.Empty)
                .SortBy(e => e.FullName)
                .ToListAsync(cancellationToken);

        public Task SaveRatesAsync(Employee employee, CancellationToken cancellationToken)
        {
            var update = Builders<Employee>.Update.Set("rates", employee.Rates.ToList());

            return _context.Employees.UpdateOneAsync(
                e => e.Id == employee.Id,
                update,
                cancellationToken: cancellationToken);
        }
    }
}
