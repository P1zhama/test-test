using System;
using Project.Domain.Common;
using Project.Domain.Rules;

namespace Project.Domain.Entities
{
    public class TimeEntry
    {
        private TimeEntry(
            string id,
            string employeeId,
            string projectId,
            DateTime date,
            decimal hours,
            string? comment,
            DateTime createdAt,
            string createdBy)
        {
            HoursRules.Ensure(hours);

            Id = id;
            EmployeeId = employeeId;
            ProjectId = projectId;
            Date = DateUtc.Day(date);
            Hours = hours;
            Comment = comment;
            CreatedAt = createdAt;
            CreatedBy = createdBy;
            Version = 1;
        }

        public string Id { get; private set; }

        public string EmployeeId { get; private set; }

        public string ProjectId { get; private set; }

        public DateTime Date { get; private set; }

        public decimal Hours { get; private set; }

        public string? Comment { get; private set; }

        public int Version { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public string CreatedBy { get; private set; }

        public DateTime? UpdatedAt { get; private set; }

        public string? UpdatedBy { get; private set; }

        public static TimeEntry Create(
            string id,
            string employeeId,
            string projectId,
            DateTime date,
            decimal hours,
            string? comment,
            DateTime now,
            string user) =>
            new TimeEntry(id, employeeId, projectId, date, hours, comment, now, user);

        public void Update(
            string employeeId,
            string projectId,
            DateTime date,
            decimal hours,
            string? comment,
            DateTime now,
            string user)
        {
            HoursRules.Ensure(hours);

            EmployeeId = employeeId;
            ProjectId = projectId;
            Date = DateUtc.Day(date);
            Hours = hours;
            Comment = comment;
            UpdatedAt = now;
            UpdatedBy = user;
        }
    }
}
