using Microsoft.EntityFrameworkCore;
using NftsArt.Database.Data;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Entities;
using NftsArt.Model.Enums;
using NftsArt.Model.Helpers;
using NftsArt.Model.Mapping;

namespace NftsArt.BL.Repositories;


public interface INftRepository
{
    Task<IEnumerable<Nft>> GetAllAsync(NftQueryDto query);
    Task<IEnumerable<Nft>> GetAllByUserAsync(string userId);
    Task<IEnumerable<Nft>> GetPopularsAsync();
    Task<Nft?> GetByIdAsync(int id);
    Task<Result<NftSummaryDto>> CreateAsync(NftCreateDto newNft, string userId);
    Task<Result<NftSummaryDto>> UpdateAsync(int id, NftUpdateDto updatedNft);
    Task<Result<NftSummaryDto>> DeleteAsync(int id);
    Task<Result<Like>> LikeAsync(string userId, int nftId);
    Task<int> GetLikesCountAsync(int nftId);
    Task<bool> HasUserLikedAsync(string userId, int nftId);
}

public class NftRepository(AppDbContext context) : INftRepository
{
    public async Task<IEnumerable<Nft>> GetAllAsync(NftQueryDto query)
    {
        var nfts = context.Nfts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            nfts = nfts.Where(n => n.Name.ToLower().Contains(query.SearchTerm.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(query.Statuses))
        {
            var statusList = query.Statuses.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var validStatuses = new List<NftStatus>();

            foreach (var status in statusList)
            {
                if (Enum.TryParse<NftStatus>(status, true, out var statusEnum))
                {
                    validStatuses.Add(statusEnum);
                }
            }

            if (validStatuses.Count > 0)
            {
                nfts = nfts.Where(n => validStatuses.Contains(n.GetAuctionStatus()));
            }
        }

        if (!string.IsNullOrWhiteSpace(query.CurrencyName))
        {
            if (Enum.TryParse<Currency>(query.CurrencyName, true, out var currencyEnum))
            {
                nfts = nfts.Where(n => n.Auction != null && n.Auction.Currency == currencyEnum);
            }
        }

        if (!string.IsNullOrWhiteSpace(query.Quantity))
        {
            if (Enum.TryParse<NftQuantity>(query.Quantity, true, out var quantityEnum))
            {
                nfts = quantityEnum switch
                {
                    NftQuantity.Single_Items => nfts = nfts.Where(n => n.Auction != null && n.Auction.Quantity == 1),
                    NftQuantity.Bundles => nfts = nfts.Where(n => n.Auction != null && n.Auction.Quantity > 1),
                    _ => nfts
                };
            }
        }

        if (!string.IsNullOrWhiteSpace(query.MinPrice))
        {
            if (decimal.TryParse(query.MinPrice, out var minPrice))
            {
                nfts = nfts.Where(n => n.Auction != null && n.Auction.Price >= minPrice);
            }
        }

        if (!string.IsNullOrWhiteSpace(query.MaxPrice))
        {
            if (decimal.TryParse(query.MaxPrice, out var maxPrice))
            {
                nfts = nfts.Where(n => n.Auction != null && n.Auction.Price <= maxPrice);
            }
        }


        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            nfts = query.SortBy switch
            {
                "name-asc" => nfts.OrderBy(n => n.Name),
                "name-desc" => nfts.OrderByDescending(n => n.Name),
                "price-asc" => nfts.Where(n => n.Auction != null)
                                   .OrderBy(n => n.Auction.Price),
                "price-desc" => nfts.Where(n => n.Auction != null)
                                   .OrderByDescending(n => n.Auction.Price),
                _ => nfts
            };
        }

        return await nfts
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Include(n => n.Creator)
                .ThenInclude(u => u.Avatar)
            .Include(n => n.Auction)
                .ThenInclude(a => a.Seller)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Nft?> GetByIdAsync(int id)
    {
        return await context.Nfts
            .Include(n => n.Creator)
                .ThenInclude(u => u.Avatar)
            .Include(n => n.Auction)
                .ThenInclude(a => a.Seller)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<Result<NftSummaryDto>> CreateAsync(NftCreateDto nftCreateDto, string userId)
    {
        var nft = nftCreateDto.ToEntity(userId);

        nft.NftCollectors.Add(new NftCollector { CollectorId = userId, NftId = nft.Id, Quantity = int.MaxValue });

        nft.CollectionNfts = nftCreateDto.Collections
            .Where(cId => context.Collections.Any(c => c.Id == cId))
            .Select(cId => new CollectionNft { NftId = nft.Id, CollectionId = cId })
            .ToList();

        await context.Nfts.AddAsync(nft);
        await context.SaveChangesAsync();

        return Result<NftSummaryDto>.Success(nft.ToSummaryDto(), "NFT created successfully");
    }

    public async Task<Result<NftSummaryDto>> UpdateAsync(int id, NftUpdateDto updatedNft)
    {
        var nft = await context.Nfts.FindAsync(id);
        if (nft == null)
            return Result<NftSummaryDto>.Failure("Nft not found.");

        nft.UpdateEntity(updatedNft);

        nft.CollectionNfts = updatedNft.Collections
            .Where(cId => context.Collections.Any(c => c.Id == cId))
            .Select(cId => new CollectionNft { NftId = id, CollectionId = cId })
            .ToList();

        await context.SaveChangesAsync();

        return Result<NftSummaryDto>.Success(nft.ToSummaryDto(), "Nft updated successfully.");
    }

    public async Task<Result<NftSummaryDto>> DeleteAsync(int id)
    {
        var nft = await context.Nfts.FindAsync(id);
        if (nft == null) return Result<NftSummaryDto>.Failure("Nft not found.");

        context.Nfts.Remove(nft);
        await context.SaveChangesAsync();

        return Result<NftSummaryDto>.Success(null!, "Nft deleted successfully.");
    }

    public async Task<IEnumerable<Nft>> GetAllByUserAsync(string userId)
    {
        return await context.NftCollectors
            .Where(nc => nc.CollectorId == userId)
            .Include(nc => nc.Nft)
                .ThenInclude(n => n.Creator)
                    .ThenInclude(u => u.Avatar)
            .Include(nc => nc.Nft)
                .ThenInclude(n => n.Auction)
                    .ThenInclude(a => a.Seller)
            .Select(nc => nc.Nft)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Nft>> GetPopularsAsync()
    {
        return await context.Nfts
            .Include(n => n.Creator)
                .ThenInclude(u => u.Avatar)
            .Include(n => n.Auction)
                .ThenInclude(a => a.Seller)
            .Include(n => n.Likes)
            .AsNoTracking()
            .OrderByDescending(n => n.Likes.Count())
            .Take(4)
            .ToListAsync();
    }

    public async Task<Result<Like>> LikeAsync(string userId, int nftId)
    {
        var existingLike = await context.Likes
            .FirstOrDefaultAsync(x => x.NftId == nftId && x.UserId == userId);

        if (existingLike == null)
        {
            var like = new Like { NftId = nftId, UserId = userId };
            await context.Likes.AddAsync(like);
            await context.SaveChangesAsync();

            return Result<Like>.Success(like, "NFT liked successfully");
        }
        else
        {
            context.Likes.Remove(existingLike);
            await context.SaveChangesAsync();

            return Result<Like>.Success(null!, "NFT unliked successfully");
        }
    }

    public async Task<int> GetLikesCountAsync(int nftId)
    {
        return await context.Likes.CountAsync(x => x.NftId == nftId);
    }

    public async Task<bool> HasUserLikedAsync(string userId, int nftId)
    {
        return await context.Likes
            .AnyAsync(x => x.NftId == nftId && x.UserId == userId);
    }
}