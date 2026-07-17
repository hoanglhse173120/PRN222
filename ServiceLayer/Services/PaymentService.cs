using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAccessLayer.Context;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.Interfaces;

namespace ServiceLayer.Services;

public class PaymentService : IPaymentService
{
    private readonly ChatbotDbContext _context;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

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
        var semaphore = _semaphores.GetOrAdd(transactionRef, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        try
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
                    // Stacking: find latest active subscription for this user (even if starting in the future)
                    var latestSub = await _context.UserSubscriptions
                        .Where(s => s.UserId == txn.UserId && s.IsActive && s.EndDate > DateTime.Now)
                        .OrderByDescending(s => s.EndDate)
                        .FirstOrDefaultAsync();

                    DateTime startDate = DateTime.Now;
                    if (latestSub != null)
                    {
                        startDate = latestSub.EndDate;
                    }

                    // Add new subscription (stacked after the previous one)
                    var sub = new UserSubscription
                    {
                        UserId = txn.UserId,
                        PackageId = txn.PackageId,
                        StartDate = startDate,
                        EndDate = startDate.AddDays(package.DurationInDays),
                        IsActive = true
                    };
                    _context.UserSubscriptions.Add(sub);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        finally
        {
            semaphore.Release();
            _semaphores.TryRemove(transactionRef, out _);
        }
    }

    public async Task<UserSubscription?> GetActiveSubscriptionAsync(string userId)
    {
        return await _context.UserSubscriptions
            .Include(s => s.Package)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive && s.StartDate <= DateTime.Now && s.EndDate > DateTime.Now);
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
