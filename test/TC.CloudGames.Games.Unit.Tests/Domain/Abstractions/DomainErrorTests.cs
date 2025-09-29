using Shouldly;
using TC.CloudGames.Games.Domain.Abstractions;

namespace TC.CloudGames.Games.Unit.Tests.Domain.Abstractions
{
    public class DomainErrorTests
    {
        [Fact]
        public void None_ShouldBeEmptyError()
        {
            // Act
            var error = DomainError.None;

            // Assert
            error.Property.ShouldBe(string.Empty);
            error.ErrorMessage.ShouldBe(string.Empty);
            error.ErrorCode.ShouldBe(string.Empty);
        }

        [Fact]
        public void NullValue_ShouldHavePredefinedValues()
        {
            // Act
            var error = DomainError.NullValue;

            // Assert
            error.Property.ShouldBe("Error.NullValue");
            error.ErrorMessage.ShouldBe("Null value was provided");
            error.ErrorCode.ShouldBe(string.Empty);
        }

        [Fact]
        public void Constructor_ShouldSetValuesCorrectly()
        {
            // Arrange
            var property = "Test.Property";
            var message = "Some error occurred";
            var code = "ERR001";

            // Act
            var error = new DomainError(property, message, code);

            // Assert
            error.Property.ShouldBe(property);
            error.ErrorMessage.ShouldBe(message);
            error.ErrorCode.ShouldBe(code);
        }

        [Fact]
        public void Equality_ShouldReturnTrue_ForSameValues()
        {
            var error1 = new DomainError("Prop", "Message", "Code");
            var error2 = new DomainError("Prop", "Message", "Code");

            error1.ShouldBe(error2);
        }

        [Fact]
        public void Equality_ShouldReturnFalse_ForDifferentValues()
        {
            var error1 = new DomainError("Prop1", "Message1", "Code1");
            var error2 = new DomainError("Prop2", "Message2", "Code2");

            error1.ShouldNotBe(error2);
        }

        [Fact]
        public void ToString_ShouldContainPropertyAndMessage()
        {
            var error = new DomainError("Prop", "Message", "Code");

            var str = error.ToString();

            str.ShouldContain("Prop");
            str.ShouldContain("Message");
            str.ShouldContain("Code");
        }
    }
}
