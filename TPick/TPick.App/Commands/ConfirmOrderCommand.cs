﻿using CsMicro.Cqrs.Commands;
using CsMicro.IntegrationEvent;
using CsMicro.Persistence;
using TPick.Domain.Aggregates;
using TPick.Domain.IntegrationEvents;

namespace TPick.App.Commands;

public class ConfirmOrderCommand : ICommand
{
    public Guid OrderId { get; init; }
}

public class ConfirmOrderCommandHandler : ICommandHandler<ConfirmOrderCommand>
{
    private readonly IGenericRepository<Order, Guid> _orderRepo;
    private readonly IEventPublisher _eventPublisher;
    private readonly IIdempotentExecutor _idempotentExecutor;

    public ConfirmOrderCommandHandler(IGenericRepository<Order, Guid> orderRepo, IEventPublisher eventPublisher,
        IIdempotentExecutor idempotentExecutor)
    {
        _orderRepo = orderRepo;
        _eventPublisher = eventPublisher;
        _idempotentExecutor = idempotentExecutor;
    }

    public async Task<CommandResult> HandleAsync(ConfirmOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await _orderRepo.FindOneAsync(command.OrderId, cancellationToken);
        if (order is null) return CommandResult.Error("Order is not valid!");

        if (order.IsConfirm) return CommandResult.Success();

        var idempotentKey = $"ConfirmOrder_OrderId_{order.Id}";
        _idempotentExecutor.Setup(idempotentKey, TimeSpan.FromSeconds(5));
        order.Confirm();
        await _orderRepo.SaveAsync(order, cancellationToken);

        var @event = new OrderConfirmedEvent()
        {
            OrderId = order.Id
        };
        await _eventPublisher.PublishAsync(@event, cancellationToken);

        return CommandResult.Success();
    }
}