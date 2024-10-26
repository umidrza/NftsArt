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
    public async Task<ActionResult<Result<List<BidSummaryDto>>>> GetBids()
    {
        var bids = (await bidRepo.GetAllAsync())
                    .Select(c => c.ToSummaryDto()).ToList();

        return Ok(Result<List<BidSummaryDto>>.Success(bids));
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<Result<BidDetailDto>>> GetBid([FromRoute] int id)
    {
        var bid = await bidRepo.GetByIdAsync(id);
        if (bid == null) 
            return Ok(Result<BidDetailDto>.Failure("Bid not found"));

        return Ok(Result<BidDetailDto>.Success(bid.ToDetailDto()));
    }

    [HttpPost("{auctionId:int}")]
    [Authorize]
    public async Task<ActionResult<Result<BidSummaryDto>>> PlaceBid([FromRoute] int auctionId, [FromBody] BidCreateDto createBidDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result<BidSummaryDto>.Failure("User not authenticated"));

        var result = await bidRepo.CreateAsync(createBidDto, userId, auctionId);
        return Ok(result);
    }
}
