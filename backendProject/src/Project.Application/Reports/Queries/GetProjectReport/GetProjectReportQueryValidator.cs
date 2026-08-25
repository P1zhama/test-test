using FluentValidation;

namespace Project.Application.Reports.Queries.GetProjectReport
{
    public class GetProjectReportQueryValidator : AbstractValidator<GetProjectReportQuery>
    {
        public GetProjectReportQueryValidator()
        {
            RuleFor(x => x.Year).InclusiveBetween(2000, 2100).WithMessage("Год должен быть в диапазоне 2000–2100.");
            RuleFor(x => x.Month).InclusiveBetween(1, 12).WithMessage("Месяц должен быть в диапазоне 1–12.");
        }
    }
}
