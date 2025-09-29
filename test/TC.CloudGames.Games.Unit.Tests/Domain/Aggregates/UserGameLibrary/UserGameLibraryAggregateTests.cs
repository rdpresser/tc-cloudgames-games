using Shouldly;
using TC.CloudGames.Games.Domain.Aggregates.UserGameLibrary;

namespace TC.CloudGames.Games.Unit.Tests.Domain.Aggregates.UserGameLibrary
{
    public class UserGameLibraryAggregateTests
    {
        [Fact]
        public void Create_ShouldReturnSuccess_WhenValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var paymentId = Guid.NewGuid();

            // Act
            var result = UserGameLibraryAggregate.Create(userId, gameId, paymentId, "Elden Ring", 299.99m);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var aggregate = result.Value;

            aggregate.UserId.ShouldBe(userId);
            aggregate.GameId.ShouldBe(gameId);
            aggregate.PaymentId.ShouldBe(paymentId);
            aggregate.GameName.ShouldBe("Elden Ring");
            aggregate.Amount.ShouldBe(299.99m);
            aggregate.IsApproved.ShouldBeFalse();
            aggregate.ErrorMessage.ShouldBeNull();
            aggregate.PurchaseDate.ShouldNotBe(default);
        }

        [Fact]
        public void Apply_ShouldPopulateProperties_WhenCreatedEventApplied()
        {
            // Arrange
            var aggregate = new UserGameLibraryAggregate();
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var paymentId = Guid.NewGuid();

            var createdEvent = new UserGameLibraryAggregate.UserGameLibraryCreatedDomainEvent(
                Guid.NewGuid(), userId, gameId, paymentId, "Game", 150, DateTimeOffset.UtcNow);

            // Act
            aggregate.Apply(createdEvent);

            // Assert
            aggregate.UserId.ShouldBe(userId);
            aggregate.GameId.ShouldBe(gameId);
            aggregate.PaymentId.ShouldBe(paymentId);
            aggregate.GameName.ShouldBe("Game");
            aggregate.Amount.ShouldBe(150);
            aggregate.IsApproved.ShouldBeFalse();
            aggregate.ErrorMessage.ShouldBeNull();
        }

        [Fact]
        public void UpdateGamePaymentStatus_ShouldUpdateApprovalAndErrorMessage()
        {
            // Arrange
            var aggregate = UserGameLibraryAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Game", 200).Value;

            // Act
            var result = aggregate.UpdateGamePaymentStatus(true, "Approved by provider");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            aggregate.IsApproved.ShouldBeTrue();
            aggregate.ErrorMessage.ShouldBe("Approved by provider");
        }

        [Fact]
        public void Apply_ShouldUpdateValues_WhenStatusUpdateEventApplied()
        {
            // Arrange
            var aggregate = UserGameLibraryAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Game", 200).Value;
            var updateEvent = new UserGameLibraryAggregate.UserGameLibraryGamePaymentStatusUpdateDomainEvent(
                aggregate.Id, aggregate.UserId, aggregate.GameId, aggregate.PaymentId, false, "Payment failed");

            // Act
            aggregate.Apply(updateEvent);

            // Assert
            aggregate.IsApproved.ShouldBeFalse();
            aggregate.ErrorMessage.ShouldBe("Payment failed");
        }
    }
}
