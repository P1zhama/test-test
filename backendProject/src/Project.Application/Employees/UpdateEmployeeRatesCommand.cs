using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Project.Application.Common.Interfaces;
using Project.Domain;
using Project.Domain.Exceptions;
using Project.Domain.ValueObjects;

namespace Project.Application.Employees
{
    public class UpdateEmployeeRatesCommand : IRequest<EmployeeDto>
    {
        public string Id { get; set; } = string.Empty;

        public List<RateDto> Rates { get; set; } = new List<RateDto>();
    }

    public class UpdateEmployeeRatesCommandValidator : AbstractValidator<UpdateEmployeeRatesCommand>
    {
        public UpdateEmployeeRatesCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Не указан сотрудник.");
            RuleFor(x => x.Rates).NotEmpty().WithMessage("История ставок не может быть пустой.");
            RuleForEach(x => x.Rates).ChildRules(rate =>
            {
                rate.RuleFor(r => r.Value).GreaterThan(0m).WithMessage("Ставка должна быть больше 0.");
                rate.RuleFor(r => r.From).NotEqual(default(DateTime)).WithMessage("Укажите дату начала действия ставки.");
            });
            RuleFor(x => x.Rates)
                .Must(rates => rates.Select(r => r.From.Date).Distinct().Count() == rates.Count)
                .WithMessage("Две ставки не могут начинаться в один день.");
        }
    }

    public class UpdateEmployeeRatesCommandHandler : IRequestHandler<UpdateEmployeeRatesCommand, EmployeeDto>
    {
        private readonly IEmployeeRepository _employees;

        public UpdateEmployeeRatesCommandHandler(IEmployeeRepository employees)
        {
            _employees = employees;
        }

        public async Task<EmployeeDto> Handle(UpdateEmployeeRatesCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employees.GetByIdAsync(request.Id, cancellationToken);
            if (employee == null)
                throw new BusinessRuleException(ErrorCodes.NotFound, "Сотрудник не найден.");

            employee.SetRates(request.Rates.Select(r => Rate.Create(r.From, r.Value)));
            await _employees.SaveRatesAsync(employee, cancellationToken);

            return new EmployeeDto
            {
                Id = employee.Id,
                FullName = employee.FullName,
                Department = employee.Department,
                Rates = employee.Rates.Select(r => new RateDto { From = r.From, Value = r.Value }).ToList()
            };
        }
    }
}
