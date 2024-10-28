using Microsoft.EntityFrameworkCore;
using NftsArt.Database.Data;
using NftsArt.Model.Dtos.Bid;
using NftsArt.Model.Entities;
using NftsArt.Model.Mapping;
using NftsArt.Model.Helpers;

namespace NftsArt.BL.Repositories;


public interface IBidRepository
{
    Task<List<Bid>> GetAllAsync();
    Task<Bid?> GetByIdAsync(int id);
    Task<Result<BidSummaryDto>> CreateAsync(BidCreateDto newBid, string userId, int nftId);
}

public class BidRepository(AppDbContext context) : IBidRepository
{
    public async Task<List<Bid>> GetAllAsync()
    {
        return await context.Bids
            .Include(b => b.Auction)
            .Include(b => b.Bidder)
                .ThenInclude(u => u.Avatar)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Bid?> GetByIdAsync(int id)
    {
        return await context.Bids
            .Include(b => b.Auction)
            .Include(b => b.Bidder)
                .ThenInclude(u => u.Avatar)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<Result<BidSummaryDto>> CreateAsync(BidCreateDto bidCreateDto, string userId, int auctionId)
    {
        var auction = await context.Auctions.FindAsync(auctionId);
        if (auction == null)
            return Result<BidSummaryDto>.Failure("Auction has not found");

        if (DateTime.Now < auction.StartTime)
            return Result<BidSummaryDto>.Failure("Auction has not started.");

        if (DateTime.Now > auction.EndTime)
            return Result<BidSummaryDto>.Failure("Auction has ended.");

        if (bidCreateDto.Quantity > auction.Quantity)
            return Result<BidSummaryDto>.Failure("Not enough copies available for the requested quantity.");

        if (bidCreateDto.Amount < auction.CurrentBid)
            return Result<BidSummaryDto>.Failure("You cannot bid lower than the current bid");

        var buyer = await context.Users
            .Include(u => u.Wallets)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (buyer == null)
            return Result<BidSummaryDto>.Failure("Buyer not found.");

        if (buyer.Wallets == null || buyer.Wallets.Count == 0)
            return Result<BidSummaryDto>.Failure("No buyer wallet");

        var buyerWallets = buyer.Wallets
            .Where(w => w.Blockchain == auction.Nft.Blockchain && w.Currency == auction.Currency)
            .ToList();

        if (buyerWallets == null || buyerWallets.Count == 0)
            return Result<BidSummaryDto>.Failure($"No {auction.Nft.Blockchain} blockchain wallet or {auction.Currency} currency");

        var buyerWallet = buyerWallets.FirstOrDefault(w => w.Balance < bidCreateDto.Amount * bidCreateDto.Quantity);

        if (buyerWallet == null)
            return Result<BidSummaryDto>.Failure("Insufficient funds.");


        var bid = bidCreateDto.ToEntity(userId, auctionId);

        auction.CurrentBid = bid.Amount;

        await context.Bids.AddAsync(bid);
        await context.SaveChangesAsync();

        return Result<BidSummaryDto>.Success(bid.ToSummaryDto(), "Bid created successfully");
    }
}