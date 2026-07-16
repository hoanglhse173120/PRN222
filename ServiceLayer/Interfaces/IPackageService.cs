using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLayer.Entities;

namespace ServiceLayer.Interfaces;

public interface IPackageService
{
    Task<List<Package>> GetAllActivePackagesAsync();
    Task<Package?> GetPackageByIdAsync(int packageId);
}
