using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NftsArt.BL.Repositories;
using System.Security.Claims;
using NftsArt.Model.Mapping;
using NftsArt.Model.Dtos.Bid;
using NftsArt.Model.Helpers;

namespace NftsArt.ApiService.Controllers;

[Route("api/bid")]
[ApiController]
public class BidController(IBidRepository bidRepo) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult<Result>> GetBids()
    {
        var bids = (await bidRepo.GetAllAsync())
                    .Select(c => c.ToSummaryDto());

        return Ok(Result.Success(bids));
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<Result>> GetBid([FromRoute] int id)
    {
        var bid = await bidRepo.GetByIdAsync(id);
        if (bid == null) 
            return NotFound(Result.Failure("Bid not found"));

        return Ok(Result.Success(bid.ToDetailDto()));
    }

    [HttpPost("{auctionId:int}")]
    [Authorize]
    public async Task<ActionResult<Result>> PlaceBid([FromRoute] int auctionId, [FromBody] BidCreateDto createBidDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result.Failure("User not authenticated"));

        var result = await bidRepo.CreateAsync(createBidDto, userId, auctionId);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}
