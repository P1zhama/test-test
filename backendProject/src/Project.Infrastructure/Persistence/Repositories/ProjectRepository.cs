using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Project.Application.Common.Interfaces;
using Project.Domain.Entities;

using ProjectEntity = Project.Domain.Entities.Project;

namespace Project.Infrastructure.Persistence.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly MongoContext _context;

        public ProjectRepository(MongoContext context)
        {
            _context = context;
        }

        public async Task<ProjectEntity?> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            if (!MongoId.IsValid(id))
                return null;

            return await _context.Projects
                .Find(p => p.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ProjectEntity>> GetAllAsync(CancellationToken cancellationToken) =>
            await _context.Projects
                .Find(FilterDefinition<ProjectEntity>.Empty)
                .SortBy(p => p.Code)
                .ToListAsync(cancellationToken);
    }
}
