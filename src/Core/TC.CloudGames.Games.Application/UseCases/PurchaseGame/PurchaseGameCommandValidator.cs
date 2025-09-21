namespace TC.CloudGames.Games.Application.UseCases.PurchaseGame
{
    public sealed class PurchaseGameCommandValidator : Validator<PurchaseGameCommand>
    {
        public PurchaseGameCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                    .WithMessage("UserId is required.")
                    .WithErrorCode($"{nameof(PurchaseGameCommand.UserId)}.Required");

            RuleFor(x => x.GameId)
                .NotEmpty()
                    .WithMessage("GameId is required.")
                    .WithErrorCode($"{nameof(PurchaseGameCommand.GameId)}.Required");

            RuleFor(x => x.PaymentMethod)
                .NotNull()
                    .WithMessage("PaymentMethod is required.")
                    .WithErrorCode($"{nameof(PurchaseGameCommand.PaymentMethod)}.Required")
                .DependentRules(() =>
                {
                    RuleFor(x => x.PaymentMethod.Method)
                        .NotEmpty()
                            .WithMessage("Payment method type is required.")
                            .WithErrorCode($"{nameof(PurchaseGameCommand.PaymentMethod)}.TypeRequired")
                        .MaximumLength(50)
                            .WithMessage("PaymentMethod cannot exceed 50 characters.")
                            .WithErrorCode($"{nameof(PurchaseGameCommand.PaymentMethod)}.MaximumLength");
                });
        }
    }
}
