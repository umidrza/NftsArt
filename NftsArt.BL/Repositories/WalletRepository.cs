﻿using Microsoft.EntityFrameworkCore;
using NftsArt.Database.Data;
using NftsArt.Model.Dtos.Wallet;
using NftsArt.Model.Entities;
using NftsArt.Model.Mapping;
using NftsArt.Model.Helpers;

namespace NftsArt.BL.Repositories;

public interface IWalletRepository
{
    Task<IEnumerable<Wallet>> GetAllAsync();
    Task<Wallet?> GetByIdAsync(int id);
    Task<Result> CreateAsync(WalletCreateDto newWallet, string userId);
    Task<Result> UpdateAsync(int id, WalletUpdateDto updatedWallet);
    Task<Result> DeleteAsync(int id);
}

public class WalletRepository(AppDbContext context) : IWalletRepository
{
    public async Task<IEnumerable<Wallet>> GetAllAsync()
    {
        return await context.Wallets
            .Include(w => w.User)
                .ThenInclude(u => u.Avatar)
            .Include(w => w.Provider)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Wallet?> GetByIdAsync(int id)
    {
        return await context.Wallets
            .Include(w => w.User)
                .ThenInclude(u => u.Avatar)
            .Include(w => w.Provider)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<Result> CreateAsync(WalletCreateDto walletCreateDto, string userId)
    {
        var wallet = walletCreateDto.ToEntity(userId);

        await context.Wallets.AddAsync(wallet);
        await context.SaveChangesAsync();

        return Result.Success(wallet, "Wallet created successfully");
    }

    public async Task<Result> UpdateAsync(int id, WalletUpdateDto updatedWallet)
    {
        var wallet = await context.Wallets.FindAsync(id);
        if (wallet == null)
            return Result.Failure("Wallet not found.");

        wallet.UpdateEntity(updatedWallet);

        await context.SaveChangesAsync();

        return Result.Success(wallet, "Wallet updated successfully.");
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var wallet = await context.Wallets.FindAsync(id);
        if (wallet == null)
            return Result.Failure("Wallet not found.");

        context.Wallets.Remove(wallet);
        await context.SaveChangesAsync();

        return Result.Success(null!, "Wallet deleted successfully.");
    }
}