using Microsoft.EntityFrameworkCore;
using NftsArt.Database.Data;
using NftsArt.Model.Dtos.Auction;
using NftsArt.Model.Entities;
using NftsArt.Model.Mapping;
using NftsArt.Model.Helpers;

namespace NftsArt.BL.Repositories;

public interface IAuctionRepository
{
    Task<List<Auction>> GetAllAsync();
    Task<Auction?> GetByIdAsync(int id);
    Task<Result<AuctionDetailDto>> CreateAsync(AuctionCreateDto newAuction, string userId, int nftId);
    Task<Result<AuctionDetailDto>> UpdateAsync(int id, AuctionUpdateDto updatedAuction);
    Task<Result<AuctionDetailDto>> DeleteAsync(int id);
    Task<Result<AuctionDetailDto>> PurchaseAsync(string userId, int auctionId, int quantity);
    Task<List<Bid>> GetBidsAsync(int id);
}

public class AuctionRepository(AppDbContext context) : IAuctionRepository
{
    public async Task<List<Auction>> GetAllAsync()
    {
        return await context.Auctions
            .Include(a => a.Seller)
                .ThenInclude(u => u.Avatar)
            .Include(a => a.Nft)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Auction?> GetByIdAsync(int id)
    {
        return await context.Auctions
            .Include(a => a.Seller)
                .ThenInclude(u => u.Avatar)
            .Include(a => a.Nft)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<Result<AuctionDetailDto>> CreateAsync(AuctionCreateDto auctionCreateDto, string userId, int nftId)
    {
        var nft = await context.Nfts.FindAsync(nftId);
        if (nft == null)
            return Result<AuctionDetailDto>.Failure("NFT not found");

        var collector = await context.NftCollectors
            .FirstOrDefaultAsync(c => c.CollectorId == userId && c.NftId == nftId);

        if (collector == null)
            return Result<AuctionDetailDto>.Failure("Collector not found");

        if (collector.CollectorId != nft.CreatorId || collector.Quantity < auctionCreateDto.Quantity)
            return Result<AuctionDetailDto>.Failure("You do not own enough copies to auction.");

        var auction = auctionCreateDto.ToEntity(userId, nftId);
        nft.Auction = auction;

        collector.Quantity -= auction.Quantity;

        await context.SaveChangesAsync();

        return Result<AuctionDetailDto>.Success(auction.ToDetailDto(), "Auction created successfully");
    }

    public async Task<Result<AuctionDetailDto>> UpdateAsync(int id, AuctionUpdateDto updatedAuction)
    {
        var auction = await context.Auctions.FindAsync(id);
        if (auction == null)
            return Result<AuctionDetailDto>.Failure("Auction not found.");

        var collector = await context.NftCollectors
            .FirstOrDefaultAsync(c => c.CollectorId == auction.SellerId && c.NftId == auction.NftId);

        if (collector == null)
            return Result<AuctionDetailDto>.Failure("Collector not found");

        if (collector.CollectorId != auction.Nft.CreatorId || collector.Quantity + auction.Quantity < updatedAuction.Quantity)
            return Result<AuctionDetailDto>.Failure("You do not own enough copies to auction.");

        collector.Quantity += auction.Quantity - updatedAuction.Quantity;

        auction.UpdateEntity(updatedAuction);
        await context.SaveChangesAsync();

        return Result<AuctionDetailDto>.Success(auction.ToDetailDto(), "Auction updated successfully.");
    }

    public async Task<Result<AuctionDetailDto>> DeleteAsync(int id)
    {
        var auction = await context.Auctions.FindAsync(id);

        if (auction == null)
            return Result<AuctionDetailDto>.Failure("Auction not found.");

        var collector = await context.NftCollectors
            .FirstOrDefaultAsync(c => c.CollectorId == auction.SellerId && c.NftId == auction.NftId);

        if (collector == null)
            return Result<AuctionDetailDto>.Failure("Collector not found");

        collector.Quantity += auction.Quantity;

        context.Auctions.Remove(auction);
        await context.SaveChangesAsync();

        return Result<AuctionDetailDto>.Success(null!, "Auction deleted successfully.");
    }

    public async Task<Result<AuctionDetailDto>> PurchaseAsync(string userId, int auctionId, int quantity)
    {
        var auction = await context.Auctions
            .Include(a => a.Seller)
            .Include(a => a.Nft)
                .ThenInclude(n => n.NftCollectors)
            .FirstOrDefaultAsync(a => a.Id == auctionId);

        if (auction == null)
            return Result<AuctionDetailDto>.Failure("Auction not found.");

        if (DateTime.UtcNow <= auction.StartTime)
            return Result<AuctionDetailDto>.Failure($"The auction not started yet. Starts on {auction.StartTime}");

        if (DateTime.UtcNow > auction.EndTime)
            return Result<AuctionDetailDto>.Failure("The auction has already ended.");

        if (quantity > auction.Quantity)
            return Result<AuctionDetailDto>.Failure("Not enough quantity available.");

        var buyer = await context.Users
            .Include(u => u.Wallets)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (buyer == null)
            return Result<AuctionDetailDto>.Failure("Buyer not found.");

        var buyerWallets = buyer.Wallets
            .Where(w => w.Blockchain == auction.Nft.Blockchain && w.Currency == auction.Currency)
            .ToList();

        if (buyerWallets == null || buyerWallets.Count == 0)
            return Result<AuctionDetailDto>.Failure($"No {auction.Nft.Blockchain} blockchain wallet or {auction.Currency} currency");

        var buyerWallet = buyerWallets.FirstOrDefault(w => w.Balance < auction.Price * quantity);

        if (buyerWallet == null)
            return Result<AuctionDetailDto>.Failure("Insufficient funds.");

        buyerWallet.Balance -= auction.Price * quantity;
        auction.Quantity -= quantity;


        var collector = auction.Nft.NftCollectors
                     .FirstOrDefault(c => c.CollectorId == buyer.Id);

        if (collector != null)
        {
            collector.Quantity += quantity;
        }
        else
        {
            var nftCollector = new NftCollector
            {
                CollectorId = buyer.Id,
                NftId = auction.NftId,
                Quantity = quantity
            };
            context.NftCollectors.Add(nftCollector);
        }

        await context.SaveChangesAsync();
        return Result<AuctionDetailDto>.Success(auction.ToDetailDto(), "Purchase successful.");
    }

    public async Task<List<Bid>> GetBidsAsync(int id)
    {
        return await context.Bids
            .Where(b => b.AuctionId == id)
            .Include(b => b.Bidder)
            .AsNoTracking()
            .ToListAsync();
    }
}