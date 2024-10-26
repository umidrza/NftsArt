using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NftsArt.BL.Repositories;
using System.Security.Claims;
using NftsArt.Model.Mapping;
using NftsArt.Model.Helpers;
using NftsArt.Model.Dtos.Auction;
using NftsArt.Model.Dtos.Bid;

namespace NftsArt.ApiService.Controllers;

[Route("api/auction")]
[ApiController]
public class AuctionController(IAuctionRepository auctionRepo) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult<Result<List<AuctionSummaryDto>>>> GetAuctions()
    {
        var auctions = (await auctionRepo.GetAllAsync())
                    .Select(a => a.ToSummaryDto()).ToList();

        return Ok(Result<List<AuctionSummaryDto>>.Success(auctions));
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<Result<AuctionDetailDto>>> GetAuction([FromRoute] int id)
    {
        var auction = await auctionRepo.GetByIdAsync(id);
        if (auction == null) 
            return Ok(Result<AuctionDetailDto>.Failure("Auction not found"));

        return Ok(Result<AuctionDetailDto>.Success(auction.ToDetailDto()));
    }

    [HttpGet("popular")]
    public async Task<ActionResult<Result<AuctionDetailDto>>> GetPopularAuction()
    {
        var auction = await auctionRepo.GetPopularAsync();
        if (auction == null)
            return Ok(Result<AuctionDetailDto>.Failure("Auction not found"));

        return Ok(Result<AuctionDetailDto>.Success(auction.ToDetailDto()));
    }

    [HttpPost("{nftId:int}")]
    [Authorize]
    public async Task<ActionResult<Result<AuctionDetailDto>>> CreateAuction([FromRoute] int nftId, [FromBody] AuctionCreateDto createAuctionDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result<AuctionDetailDto>.Failure("User not authenticated"));

        var result = await auctionRepo.CreateAsync(createAuctionDto, userId, nftId);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Result<AuctionDetailDto>>> UpdateAuction([FromRoute] int id, [FromBody] AuctionUpdateDto updateAuctionDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var auction = await auctionRepo.GetByIdAsync(id);
        if (auction == null)
            return NotFound(Result<AuctionDetailDto>.Failure("Auction not found"));

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result<AuctionDetailDto>.Failure("User not authenticated"));

        if (auction.SellerId != userId)
            return BadRequest(Result<AuctionDetailDto>.Failure("You do not have permission to update this Auction"));

        var result = await auctionRepo.UpdateAsync(id, updateAuctionDto);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Result<AuctionDetailDto>>> DeleteAuction([FromRoute] int id)
    {
        var auction = await auctionRepo.GetByIdAsync(id);
        if (auction == null)
            return NotFound(Result<AuctionDetailDto>.Failure("Auction not found"));

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result<AuctionDetailDto>.Failure("User not authenticated"));

        if (auction.SellerId != userId)
            return BadRequest(Result<AuctionDetailDto>.Failure("You do not have permission to delete this Auction"));

        var result = await auctionRepo.DeleteAsync(id);
        return Ok(result);
    }


    [HttpPost("purchase/{id:int}")]
    public async Task<ActionResult<Result<AuctionDetailDto>>> PurchaseAuction([FromRoute] int id, [FromBody] int quantity)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result<AuctionDetailDto>.Failure("User not authenticated"));

        var result = await auctionRepo.PurchaseAsync(userId, id, quantity);
        return Ok(result);
    }


    [HttpGet("{id:int}/bids")]
    public async Task<ActionResult<List<BidSummaryDto>>> GetAuctionBids([FromRoute] int id)
    {
        var bids = (await auctionRepo.GetBidsAsync(id))
                    .Select(b => b.ToSummaryDto()).ToList();

        return Ok(Result<List<BidSummaryDto>>.Success(bids));
    }
}
