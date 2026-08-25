using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Periods;

namespace Project.Api.Controllers
{
    [ApiController]
    [Route("api/periods")]
    public class PeriodsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PeriodsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public Task<List<ClosedPeriodDto>> Get(CancellationToken cancellationToken) =>
            _mediator.Send(new GetClosedPeriodsQuery(), cancellationToken);

        [HttpPost("close")]
        public Task<ClosedPeriodDto?> Close(
            [FromBody] ClosePeriodCommand command,
            CancellationToken cancellationToken) =>
            _mediator.Send(command, cancellationToken);

        [HttpPost("open")]
        public async Task<IActionResult> Open(
            [FromBody] OpenPeriodCommand command,
            CancellationToken cancellationToken)
        {
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
