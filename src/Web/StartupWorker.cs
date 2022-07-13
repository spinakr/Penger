using Domain;
using MediatR;
using PocketCqrs.EventStore;

namespace Web;

public class StartupWorker : IHostedService, IDisposable
{
    private Timer _timer;
    private IEventStore _eventStore { get; }
    private IMediator _mediator { get; }

    public StartupWorker(IEventStore eventStore, IMediator mediator)
    {
        _eventStore = eventStore;
        _mediator = mediator;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(1));
        return Task.CompletedTask;
    }

    private void DoWork(object state)
    {
        var stream = _eventStore.LoadEventStream("kofoed");
        if (!stream.Events.Any())
        {
            var port = Portfolio.CreateNew("kofoed");
            _eventStore.AppendToStream(port.Id, port.PendingEvents, 0);
        }

        var allEvents = _eventStore.LoadAllEvents();
        System.Console.WriteLine($"{allEvents.Count} events loaded");
        foreach (var @event in allEvents)
        {
            _mediator.Publish(@event).GetAwaiter().GetResult();
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}