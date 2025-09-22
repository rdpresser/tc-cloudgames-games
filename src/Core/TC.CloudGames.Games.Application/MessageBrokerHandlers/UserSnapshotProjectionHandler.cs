namespace TC.CloudGames.Games.Application.MessageBrokerHandlers
{
    /// <summary>
    /// Handles user integration events from the Users microservice and updates
    /// the UserSnapshot store accordingly.
    /// This class projects external events into a read-optimized snapshot
    /// for the Games microservice.
    /// </summary>
    public class UserSnapshotProjectionHandler : IWolverineHandler
    {
        private readonly IUserSnapshotStore _store;

        public UserSnapshotProjectionHandler(IUserSnapshotStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        // ------------------------- 
        // User Created
        // -------------------------
        public async Task HandleAsync(EventContext<UserCreatedIntegrationEvent> @event, CancellationToken cancellationToken)
        {
            // Map integration event to snapshot
            var snapshot = new UserSnapshot
            {
                Id = @event.EventData.AggregateId,
                Name = @event.EventData.Name,
                Email = @event.EventData.Email,
                Username = @event.EventData.Username,
                Role = @event.EventData.Role,
                IsActive = true,
                CreatedAt = @event.EventData.OccurredOn,
                UpdatedAt = null
            };

            // Save snapshot
            await _store.SaveAsync(snapshot);
        }

        // -------------------------
        // User Updated
        // -------------------------
        public async Task HandleAsync(EventContext<UserUpdatedIntegrationEvent> @event, CancellationToken cancellationToken)
        {
            // Load existing snapshot
            var snapshot = await _store.LoadAsync(@event.EventData.Id);
            if (snapshot == null) return;

            // Update relevant fields
            snapshot.Name = @event.EventData.Name;
            snapshot.Email = @event.EventData.Email;
            snapshot.Username = @event.EventData.Username;
            snapshot.UpdatedAt = @event.EventData.OccurredOn;

            // Save updated snapshot
            await _store.SaveAsync(snapshot);
        }

        // -------------------------
        // User Role Changed
        // -------------------------
        public async Task HandleAsync(EventContext<UserRoleChangedIntegrationEvent> @event, CancellationToken cancellationToken)
        {
            var snapshot = await _store.LoadAsync(@event.EventData.Id);
            if (snapshot == null) return;

            snapshot.Role = @event.EventData.NewRole;
            snapshot.UpdatedAt = @event.EventData.OccurredOn;

            await _store.SaveAsync(snapshot);
        }

        // -------------------------
        // User Activated
        // -------------------------
        public async Task HandleAsync(EventContext<UserActivatedIntegrationEvent> @event, CancellationToken cancellationToken)
        {
            var snapshot = await _store.LoadAsync(@event.EventData.Id);
            if (snapshot == null) return;

            snapshot.IsActive = true;
            snapshot.UpdatedAt = @event.EventData.OccurredOn;

            await _store.SaveAsync(snapshot);
        }

        // -------------------------
        // User Deactivated
        // -------------------------
        public async Task HandleAsync(EventContext<UserDeactivatedIntegrationEvent> @event, CancellationToken cancellationToken)
        {
            var snapshot = await _store.LoadAsync(@event.EventData.Id);
            if (snapshot == null) return;

            snapshot.IsActive = false;
            snapshot.UpdatedAt = @event.EventData.OccurredOn;

            await _store.SaveAsync(snapshot);
        }
    }
}
