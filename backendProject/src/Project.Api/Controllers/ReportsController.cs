using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Reports.Queries.GetProjectReport;

namespace Project.Api.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("projects")]
        public Task<ProjectReportDto> GetProjectReport(
            [FromQuery] GetProjectReportQuery query,
            CancellationToken cancellationToken) =>
            _mediator.Send(query, cancellationToken);
    }
}
