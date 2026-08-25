using MediatR;

namespace Project.Application.Reports.Queries.GetProjectReport
{
    public class GetProjectReportQuery : IRequest<ProjectReportDto>
    {
        public int Year { get; set; }

        public int Month { get; set; }
    }
}
