using Microsoft.EntityFrameworkCore;
using NftsArt.Database.Data;
using NftsArt.Model.Dtos.Provider;
using NftsArt.Model.Entities;
using NftsArt.Model.Helpers;
using NftsArt.Model.Mapping;

namespace NftsArt.BL.Repositories;

public interface IProviderRepository
{
    Task<IEnumerable<Provider>> GetAllAsync();
    Task<Provider?> GetByIdAsync(int id);
    Task<Result<ProviderSummaryDto>> CreateAsync(ProviderCreateDto newProvider);
    Task<Result<ProviderSummaryDto>> DeleteAsync(int id);
}

public class ProviderRepository(AppDbContext context) : IProviderRepository
{
    public async Task<IEnumerable<Provider>> GetAllAsync()
    {
        return await context.Providers
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Provider?> GetByIdAsync(int id)
    {
        return await context.Providers
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<Result<ProviderSummaryDto>> CreateAsync(ProviderCreateDto providerCreateDto)
    {
        var provider = providerCreateDto.ToEntity();

        await context.Providers.AddAsync(provider);
        await context.SaveChangesAsync();

        return Result<ProviderSummaryDto>.Success(provider.ToSummaryDto(), "Provider created successfully");
    }

    public async Task<Result<ProviderSummaryDto>> DeleteAsync(int id)
    {
        var provider = await context.Providers.FindAsync(id);
        if (provider == null)
            return Result<ProviderSummaryDto>.Failure("Provider not found.");

        context.Providers.Remove(provider);
        await context.SaveChangesAsync();

        return Result<ProviderSummaryDto>.Success(null!, "Provider deleted successfully.");
    }
}
