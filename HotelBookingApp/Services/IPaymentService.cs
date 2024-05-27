public interface IPaymentService
{
    Task<bool> ProcessPaymentAsync(decimal amount, string cardToken);
}

public class MockPaymentService : IPaymentService
{
    public Task<bool> ProcessPaymentAsync(decimal amount, string cardToken)
    {
        // Simulate payment processing delay
        Task.Delay(1000).Wait();

        // Always return true for successful payment
        return Task.FromResult(true);
    }
}
