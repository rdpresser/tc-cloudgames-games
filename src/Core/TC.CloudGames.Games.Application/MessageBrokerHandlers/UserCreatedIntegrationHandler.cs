using Marten;
using TC.CloudGames.Contracts.Events.Users;
using Wolverine.Attributes;

namespace TC.CloudGames.Games.Application.MessageBrokerHandlers
{

    [WolverineHandler]
    public class UserCreatedIntegrationHandler
    {
        private readonly IDocumentSession _session;

        public UserCreatedIntegrationHandler(IDocumentSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task Handle(EventContext<UserCreatedIntegrationEvent> @event, CancellationToken cancellationToken)
        {
            // Example logic: Log the event details
            Console.WriteLine($"User Created Event Received: {@event.EventData.AggregateId}, {@event.EventData.Name}, {@event.EventData.Email}");
            await Task.CompletedTask;
            ////// Example logic: Store event in Marten for auditing
            _session.Events.Append(@event.EventData.AggregateId, @event.EventData);
            ////await _session.SaveChangesAsync(cancellationToken);
        }
    }
}
