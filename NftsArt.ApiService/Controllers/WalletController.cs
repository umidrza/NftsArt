using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NftsArt.BL.Repositories;
using NftsArt.Model.Helpers;
using NftsArt.Model.Mapping;
using System.Security.Claims;
using NftsArt.Model.Dtos.Wallet;

namespace NftsArt.ApiService.Controllers;

[Route("api/wallet")]
[ApiController]
public class WalletController(IWalletRepository walletRepo) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult<Result>> GetWallets()
    {
        var wallets = (await walletRepo.GetAllAsync())
                    .Select(c => c.ToSummaryDto());

        return Ok(Result.Success(wallets));
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<Result>> GetWallet([FromRoute] int id)
    {
        var wallet = await walletRepo.GetByIdAsync(id);
        if (wallet == null) 
            return NotFound(Result.Failure("Wallet not found"));

        return Ok(Result.Success(wallet.ToDetailDto()));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Result>> CreateWallet([FromBody] WalletCreateDto createWalletDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result.Failure("User not authenticated"));

        var result = await walletRepo.CreateAsync(createWalletDto, userId);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Result>> UpdateWallet([FromRoute] int id, [FromBody] WalletUpdateDto updateWalletDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var wallet = await walletRepo.GetByIdAsync(id);
        if (wallet == null)
            return NotFound(Result.Failure("Wallet not found"));

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result.Failure("User not authenticated"));

        if (wallet.UserId != userId)
            return BadRequest(Result.Failure("You do not have permission to update this Wallet"));

        var result = await walletRepo.UpdateAsync(id, updateWalletDto);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Result>> DeleteWallet([FromRoute] int id)
    {
        var wallet = await walletRepo.GetByIdAsync(id);
        if (wallet == null)
            return NotFound(Result.Failure("Wallet not found"));

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result.Failure("User not authenticated"));

        if (wallet.UserId != userId)
            return BadRequest(Result.Failure("You do not have permission to delete this Wallet"));

        var result = await walletRepo.DeleteAsync(id);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }
}
