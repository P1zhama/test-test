using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Project.Application.TimeEntries;
using Project.Application.TimeEntries.Commands.CreateTimeEntry;
using Project.Application.TimeEntries.Commands.DeleteTimeEntry;
using Project.Application.TimeEntries.Commands.UpdateTimeEntry;
using Project.Application.TimeEntries.Queries.GetTimeEntries;

namespace Project.Api.Controllers
{
    [ApiController]
    [Route("api/time-entries")]
    public class TimeEntriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TimeEntriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public Task<TimeEntriesPageDto> Get([FromQuery] GetTimeEntriesQuery query, CancellationToken cancellationToken) =>
            _mediator.Send(query, cancellationToken);

        [HttpPut]
        public async Task<ActionResult<TimeEntryDto>> Create(
            [FromBody] CreateTimeEntryCommand command,
            CancellationToken cancellationToken)
        {
            var created = await _mediator.Send(command, cancellationToken);
            return StatusCode(201, created);
        }

        [HttpPost("{id}")]
        public Task<TimeEntryDto> Update(
            string id,
            [FromBody] UpdateTimeEntryCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            return _mediator.Send(command, cancellationToken);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteTimeEntryCommand { Id = id }, cancellationToken);
            return NoContent();
        }
    }
}
