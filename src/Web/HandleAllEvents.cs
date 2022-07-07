using MediatR;
using PocketCqrs;

namespace Web;

public class HandleAllEvents : IEventHandler<IEvent>
{
    private readonly IMediator _mediator;

    public HandleAllEvents(IMediator mediator)
    {
        _mediator = mediator;
    }

    public void Handle(IEvent @event)
    {
        System.Console.WriteLine($"Handling event: {@event.GetType().Name}");
        _mediator.Publish(@event);
    }
}