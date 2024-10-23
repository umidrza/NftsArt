using Microsoft.EntityFrameworkCore;
using NftsArt.Database.Data;
using NftsArt.Model.Dtos.Collection;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Entities;
using NftsArt.Model.Enums;
using NftsArt.Model.Helpers;
using NftsArt.Model.Mapping;

namespace NftsArt.BL.Repositories;


public interface ICollectionRepository
{
    Task<IEnumerable<Collection>> GetAllAsync(CollectionQueryDto query);
    Task<Collection?> GetByIdAsync(int id);
    Task<Result> CreateAsync(CollectionCreateDto newCollection, string userId);
    Task<Result> UpdateAsync(int id, CollectionUpdateDto updatedCollection);
    Task<Result> DeleteAsync(int id);
    Task<IEnumerable<Nft>> GetNftsAsync(int id, NftQueryDto query);
    Task<IEnumerable<Collection>> GetAllByUserAsync(string userId);
}

public class CollectionRepository(AppDbContext context) : ICollectionRepository
{
    public async Task<IEnumerable<Collection>> GetAllAsync(CollectionQueryDto query)
    {
        var collections = context.Collections.AsQueryable();


        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            collections = collections.Where(c => c.Name.ToLower().Contains(query.SearchTerm.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(query.BlockchainName))
        {
            if (Enum.TryParse<Blockchain>(query.BlockchainName, true, out var blockchainEnum))
            {
                collections = collections.Where(c => c.Blockchain == blockchainEnum);
            }
        }

        if (!string.IsNullOrEmpty(query.Categories))
        {
            var categoryList = query.Categories.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var validCategories = new List<Category>();

            foreach (var category in categoryList)
            {
                if (Enum.TryParse<Category>(category, true, out var categoryEnum))
                {
                    validCategories.Add(categoryEnum);
                }
            }

            if (validCategories.Count > 0)
            {
                collections = collections.Where(c => validCategories.Contains(c.Category));
            }
        }

        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            collections = query.SortBy switch
            {
                "name-asc" => collections.OrderBy(c => c.Name),
                "name-desc" => collections.OrderByDescending(c => c.Name),
                _ => collections
            };
        }

        return await collections
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Include(c => c.Creator)
                .ThenInclude(u => u.Avatar)
            .Include(c => c.CollectionNfts)
                .ThenInclude(cn => cn.Nft)
                    .ThenInclude(n => n.Auction)
                        .ThenInclude(a => a.Seller)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Collection?> GetByIdAsync(int id)
    {
        var collection = await context.Collections
            .Include(c => c.Creator)
                .ThenInclude(u => u.Avatar)
            .Include(c => c.CollectionNfts)
                .ThenInclude(cn => cn.Nft)
                    .ThenInclude(n => n.Auction)
                        .ThenInclude(a => a.Seller)
            .FirstOrDefaultAsync(c => c.Id == id);

        return collection;
    }

    public async Task<Result> CreateAsync(CollectionCreateDto collectionCreateDto, string userId)
    {
        var collection = collectionCreateDto.ToEntity(userId);

        collection.CollectionNfts = collectionCreateDto.Nfts
            .Where(id => context.Nfts.Any(nft => nft.Id == id))
            .Select(id => new CollectionNft { NftId = id, CollectionId = collection.Id })
            .ToList();

        await context.Collections.AddAsync(collection);
        await context.SaveChangesAsync();

        return Result.Success(collection, "Collection created successfully");
    }

    public async Task<Result> UpdateAsync(int id, CollectionUpdateDto updatedCollection)
    {
        var collection = await context.Collections.FindAsync(id);
        if (collection == null)
            return Result.Failure("Collection not found.");

        collection.UpdateEntity(updatedCollection);

        collection.CollectionNfts = updatedCollection.Nfts
            .Where(nftId => context.Nfts.Any(nft => nft.Id == nftId))
            .Select(nftId => new CollectionNft { NftId = nftId, CollectionId = id })
            .ToList();

        await context.SaveChangesAsync();
        return Result.Success(collection, "Collection updated successfully.");
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var collection = await context.Collections.FindAsync(id);
        if (collection == null) return Result.Failure("Collection not found.");

        context.Collections.Remove(collection);
        await context.SaveChangesAsync();

        return Result.Success(null!, "Collection deleted successfully.");
    }

    public async Task<IEnumerable<Nft>> GetNftsAsync(int id, NftQueryDto query)
    {
        var collection = await context.Collections
            .Include(c => c.CollectionNfts)
                .ThenInclude(cn => cn.Nft)
                    .ThenInclude(n => n.Auction)
                        .ThenInclude(a => a.Seller)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (collection == null) return null!;


        var nfts = collection.CollectionNfts.Select(cn => cn.Nft);

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

        return  nfts
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize);
    }

    public async Task<IEnumerable<Collection>> GetAllByUserAsync(string userId)
    {
        return await context.Collections
            .Where(c => c.CreatorId == userId)
            .Include(c => c.Creator)
                .ThenInclude(u => u.Avatar)
            .Include(c => c.CollectionNfts)
                .ThenInclude(cn => cn.Nft)
                    .ThenInclude(n => n.Auction)
                        .ThenInclude(a => a.Seller)
            .AsNoTracking()
            .ToListAsync();
    }
}
