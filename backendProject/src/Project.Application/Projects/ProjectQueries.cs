using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Project.Application.Common.Interfaces;

namespace Project.Application.Projects
{
    public class ProjectDto
    {
        public string Id { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public decimal Budget { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }

    public class GetProjectsQuery : IRequest<List<ProjectDto>>
    {
    }

    public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, List<ProjectDto>>
    {
        private readonly IProjectRepository _projects;

        public GetProjectsQueryHandler(IProjectRepository projects)
        {
            _projects = projects;
        }

        public async Task<List<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
        {
            var projects = await _projects.GetAllAsync(cancellationToken);
            return projects
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Budget = p.Budget,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate
                })
                .OrderBy(p => p.Code, StringComparer.CurrentCulture)
                .ToList();
        }
    }
}
