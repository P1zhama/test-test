using System.Collections.Generic;

namespace Project.Application.Reports.Queries.GetProjectReport
{
    public class ProjectReportAggregate
    {
        public string ProjectId { get; set; } = string.Empty;

        public string ProjectCode { get; set; } = string.Empty;

        public string ProjectName { get; set; } = string.Empty;

        public decimal Budget { get; set; }

        public decimal Hours { get; set; }

        public decimal Amount { get; set; }
    }

    public class ProjectReportRowDto
    {
        public string ProjectId { get; set; } = string.Empty;

        public string ProjectCode { get; set; } = string.Empty;

        public string ProjectName { get; set; } = string.Empty;

        public decimal Hours { get; set; }

        public decimal Amount { get; set; }

        public decimal Budget { get; set; }

        public decimal? Percent { get; set; }

        public bool IsOverspent { get; set; }

        public bool IsAtRisk { get; set; }
    }

    public class ProjectReportDto
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public List<ProjectReportRowDto> Rows { get; set; } = new List<ProjectReportRowDto>();

        public decimal TotalHours { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
