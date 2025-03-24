using DeelRate.Application.Abstractions.Services;
using DeelRate.Domain.Common;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DeelRate.Infrastructure.Services.Messaging;

public class EmailService(IPublishEndpoint publishEndpoint, ILogger logger) : IEmailService
{
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public async Task PublishDomainEventEmailAsync<TEvent>(TEvent domainEvent)
        where TEvent : IDomainEvent
    {
        await _publishEndpoint.Publish(domainEvent);
        logger.LogInformation("Published domain event {EventType}", typeof(TEvent).Name);
    }
}
