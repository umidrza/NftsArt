using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NftsArt.BL.Repositories;
using System.Security.Claims;
using NftsArt.Model.Mapping;
using NftsArt.Model.Helpers;
using NftsArt.Model.Dtos.Auction;

namespace NftsArt.ApiService.Controllers;

[Route("api/auction")]
[ApiController]
public class AuctionController(IAuctionRepository auctionRepo) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult<Result>> GetAuctions()
    {
        var auctions = (await auctionRepo.GetAllAsync())
                        .Select(c => c.ToSummaryDto());

        return Ok(Result.Success(auctions));
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<Result>> GetAuction([FromRoute] int id)
    {
        var auction = await auctionRepo.GetByIdAsync(id);
        if (auction == null) 
            return NotFound(Result.Failure("Auction not found"));

        return Ok(Result.Success(auction.ToDetailDto()));
    }

    [HttpPost("{nftId:int}")]
    [Authorize]
    public async Task<ActionResult<Result>> CreateAuction([FromRoute] int nftId, [FromBody] AuctionCreateDto createAuctionDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result.Failure("User not authenticated"));

        var result = await auctionRepo.CreateAsync(createAuctionDto, userId, nftId);
        if (!result.IsSuccess) 
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Result>> UpdateAuction([FromRoute] int id, [FromBody] AuctionUpdateDto updateAuctionDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var auction = await auctionRepo.GetByIdAsync(id);
        if (auction == null)
            return NotFound(Result.Failure("Auction not found"));

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result.Failure("User not authenticated"));

        if (auction.SellerId != userId)
            return BadRequest(Result.Failure("You do not have permission to update this Auction"));

        var result = await auctionRepo.UpdateAsync(id, updateAuctionDto);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Result>> DeleteAuction([FromRoute] int id)
    {
        var auction = await auctionRepo.GetByIdAsync(id);
        if (auction == null)
            return NotFound(Result.Failure("Auction not found"));

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result.Failure("User not authenticated"));

        if (auction.SellerId != userId)
            return BadRequest(Result.Failure("You do not have permission to delete this Auction"));

        var result = await auctionRepo.DeleteAsync(id);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }


    [HttpPost("purchase/{id:int}")]
    public async Task<ActionResult<Result>> PurchaseAuction([FromRoute] int id, [FromBody] int quantity)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result.Failure("User not authenticated"));

        var result = await auctionRepo.PurchaseAsync(userId, id, quantity);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }


    [HttpGet("{id:int}/bids")]
    public async Task<ActionResult<Result>> GetAuctionBids([FromRoute] int id)
    {
        var bids = (await auctionRepo.GetBidsAsync(id))
                    .Select(b => b.ToSummaryDto());

        return Ok(Result.Success(bids));
    }

}
