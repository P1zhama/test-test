using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Employees;
using Project.Application.Projects;

namespace Project.Api.Controllers
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EmployeesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public Task<List<EmployeeDto>> Get(CancellationToken cancellationToken) =>
            _mediator.Send(new GetEmployeesQuery(), cancellationToken);

        [HttpPost("{id}/rates")]
        public Task<EmployeeDto> UpdateRates(
            string id,
            [FromBody] UpdateEmployeeRatesCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            return _mediator.Send(command, cancellationToken);
        }
    }

    [ApiController]
    [Route("api/projects")]
    public class ProjectsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public Task<List<ProjectDto>> Get(CancellationToken cancellationToken) =>
            _mediator.Send(new GetProjectsQuery(), cancellationToken);
    }
}
