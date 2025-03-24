using DeelRate.Domain.Common;

namespace DeelRate.Application.Abstractions.Services;

public interface IEmailService
{
    Task PublishDomainEventEmailAsync<TEvent>(TEvent domainEvent)
        where TEvent : IDomainEvent;
}
