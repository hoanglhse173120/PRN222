using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer.Context;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.Interfaces;

namespace ServiceLayer.Services;

public class PaymentService : IPaymentService
{
    private readonly ChatbotDbContext _context;

    public PaymentService(ChatbotDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentTransaction> CreatePendingTransactionAsync(string userId, int packageId, string paymentMethod, string transactionRef)
    {
        var package = await _context.Packages.FindAsync(packageId);
        if (package == null)
            throw new ArgumentException("Gói cước không tồn tại.");

        var txn = new PaymentTransaction
        {
            UserId = userId,
            PackageId = packageId,
            Amount = package.Price,
            PaymentMethod = paymentMethod,
            TransactionDate = DateTime.Now,
            TransactionReference = transactionRef,
            Status = "Pending"
        };

        _context.PaymentTransactions.Add(txn);
        await _context.SaveChangesAsync();
        return txn;
    }

    public async Task<bool> CompleteTransactionAsync(string transactionRef, bool isSuccess)
    {
        var txn = await _context.PaymentTransactions
            .FirstOrDefaultAsync(t => t.TransactionReference == transactionRef);

        if (txn == null || txn.Status != "Pending")
            return false;

        txn.Status = isSuccess ? "Success" : "Failed";

        if (isSuccess)
        {
            var package = await _context.Packages.FindAsync(txn.PackageId);
            if (package != null)
            {
                // Deactivate previous active subscriptions for this user
                var oldSubs = await _context.UserSubscriptions
                    .Where(s => s.UserId == txn.UserId && s.IsActive)
                    .ToListAsync();
                foreach (var s in oldSubs)
                {
                    s.IsActive = false;
                }

                // Add new subscription
                var sub = new UserSubscription
                {
                    UserId = txn.UserId,
                    PackageId = txn.PackageId,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(package.DurationInDays),
                    IsActive = true
                };
                _context.UserSubscriptions.Add(sub);
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UserSubscription?> GetActiveSubscriptionAsync(string userId)
    {
        return await _context.UserSubscriptions
            .Include(s => s.Package)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive && s.EndDate > DateTime.Now);
    }

    public async Task<List<PaymentTransaction>> GetAllTransactionsAsync()
    {
        return await _context.PaymentTransactions
            .Include(t => t.User)
            .Include(t => t.Package)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }
}
