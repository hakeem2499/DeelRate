namespace DeelRate.Domain.Common
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
