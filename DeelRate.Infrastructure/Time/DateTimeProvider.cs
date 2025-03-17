using DeelRate.Domain.Common;

namespace DeelRate.Infrastructure.Time;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.Now;
}
