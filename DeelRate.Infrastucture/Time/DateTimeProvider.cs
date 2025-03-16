using DeelRate.Domain.Common;

namespace DeelRate.Infrastucture.Time;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.Now;
}
