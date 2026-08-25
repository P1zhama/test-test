namespace Project.Domain
{
    public static class ErrorCodes
    {
        public const string ValidationError = "VALIDATION_ERROR";
        public const string EmployeeRateNotFound = "EMPLOYEE_RATE_NOT_FOUND";
        public const string DateOutOfProjectPeriod = "DATE_OUT_OF_PROJECT_PERIOD";
        public const string DailyHoursLimitExceeded = "DAILY_HOURS_LIMIT_EXCEEDED";
        public const string PeriodClosed = "PERIOD_CLOSED";
        public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";
        public const string PeriodAlreadyClosed = "PERIOD_ALREADY_CLOSED";
        public const string NotFound = "NOT_FOUND";
        public const string InternalError = "INTERNAL_ERROR";
    }
}
