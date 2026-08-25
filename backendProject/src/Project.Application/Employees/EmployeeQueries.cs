using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Project.Application.Common.Interfaces;

namespace Project.Application.Employees
{
    public class RateDto
    {
        public DateTime From { get; set; }

        public decimal Value { get; set; }
    }

    public class EmployeeDto
    {
        public string Id { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public List<RateDto> Rates { get; set; } = new List<RateDto>();
    }

    public class GetEmployeesQuery : IRequest<List<EmployeeDto>>
    {
    }

    public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, List<EmployeeDto>>
    {
        private readonly IEmployeeRepository _employees;

        public GetEmployeesQueryHandler(IEmployeeRepository employees)
        {
            _employees = employees;
        }

        public async Task<List<EmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
        {
            var employees = await _employees.GetAllAsync(cancellationToken);
            return employees
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    Department = e.Department,
                    Rates = e.Rates
                        .OrderBy(r => r.From)
                        .Select(r => new RateDto { From = r.From, Value = r.Value })
                        .ToList()
                })
                .OrderBy(e => e.FullName, StringComparer.CurrentCulture)
                .ToList();
        }
    }
}
