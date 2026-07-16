using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLayer.Entities;

namespace ServiceLayer.Interfaces;

public interface IPaymentService
{
    Task<PaymentTransaction> CreatePendingTransactionAsync(string userId, int packageId, string paymentMethod, string transactionRef);
    Task<bool> CompleteTransactionAsync(string transactionRef, bool isSuccess);
    Task<UserSubscription?> GetActiveSubscriptionAsync(string userId);
    Task<List<PaymentTransaction>> GetAllTransactionsAsync();
}
