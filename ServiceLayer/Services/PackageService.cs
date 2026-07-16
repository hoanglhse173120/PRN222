using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer.Context;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.Interfaces;

namespace ServiceLayer.Services;

public class PackageService : IPackageService
{
    private readonly ChatbotDbContext _context;

    public PackageService(ChatbotDbContext context)
    {
        _context = context;
    }

    public async Task<List<Package>> GetAllActivePackagesAsync()
    {
        return await _context.Packages
            .Where(p => p.IsActive)
            .OrderBy(p => p.Price)
            .ToListAsync();
    }

    public async Task<Package?> GetPackageByIdAsync(int packageId)
    {
        return await _context.Packages.FindAsync(packageId);
    }
}
