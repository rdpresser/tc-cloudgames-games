////namespace TC.CloudGames.Games.Application.MessageBrokerHandlers
////{
////    public class ChargePaymentResponseHandler
////    {
////        ////private readonly IUserGameLibraryRepository _repository;
////        private readonly ILogger<ChargePaymentResponseHandler> _logger;

////        public ChargePaymentResponseHandler(IUserGameLibraryRepository repository, ILogger<ChargePaymentResponseHandler> logger)
////        {
////            ////_repository = repository ?? throw new ArgumentNullException(nameof(repository));
////            _logger = logger;
////        }

////        ////public async Task<ChargePaymentResponse> Handle(ChargePaymentRequest request, CancellationToken cancellationToken = default)
////        ////{
////        ////    try
////        ////    {
////        ////        var payment = new PaymentAggregate
////        ////        {
////        ////            Id = Guid.NewGuid(),
////        ////            GameId = request.GameId,
////        ////            UserId = request.UserId,
////        ////            Amount = request.Amount,
////        ////            GameName = string.Empty,
////        ////            PurchaseDate = DateTimeOffset.UtcNow
////        ////        };

////        ////        await _paymentRepository.SaveAsync(payment, cancellationToken);

////        ////        return new ChargePaymentResponse(
////        ////            success: true,
////        ////            paymentId: payment.Id,
////        ////            errorMessage: null
////        ////        );
////        ////    }
////        ////    catch (Exception ex)
////        ////    {
////        ////        return new ChargePaymentResponse(
////        ////            success: false,
////        ////            paymentId: null,
////        ////            errorMessage: ex.Message
////        ////        );
////        ////    }
////        ////}


////        //Wolverine RPC convention: método deve retornar a response esperada
////        public void Handle(ChargePaymentResponse response)
////        {

////            //create usergamelibrary entry
////            ////await _repository.SaveAsync(payment);

////            _logger.LogInformation("Received ChargePaymentResponse for PaymentId: {PaymentId}, Success: {Success}",
////                response.PaymentId, response.Success);

////        }
////    }
////}


