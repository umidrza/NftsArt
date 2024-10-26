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
    public async Task<ActionResult<Result<List<WalletSummaryDto>>>> GetWallets()
    {
        var wallets = (await walletRepo.GetAllAsync())
                    .Select(c => c.ToSummaryDto()).ToList();

        return Ok(Result<List<WalletSummaryDto>>.Success(wallets));
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<Result<WalletDetailDto>>> GetWallet([FromRoute] int id)
    {
        var wallet = await walletRepo.GetByIdAsync(id);
        if (wallet == null) 
            return Ok(Result<WalletDetailDto>.Failure("Wallet not found"));

        return Ok(Result<WalletDetailDto>.Success(wallet.ToDetailDto()));
    }


    [HttpGet("my-wallet")]
    [Authorize]
    public async Task<ActionResult<Result<WalletDetailDto>>> GetUserWallet([FromQuery] WalletQueryDto query)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result<WalletDetailDto>.Failure("User not authenticated"));

        var wallet = await walletRepo.GetByQueryAsync(query, userId);
        if (wallet == null)
            return Ok(Result<WalletDetailDto>.Failure("Wallet not found"));

        return Ok(Result<WalletDetailDto>.Success(wallet.ToDetailDto()));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Result<WalletSummaryDto>>> CreateWallet([FromBody] WalletCreateDto createWalletDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result<WalletSummaryDto>.Failure("User not authenticated"));

        var result = await walletRepo.CreateAsync(createWalletDto, userId);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Result<WalletSummaryDto>>> UpdateWallet([FromRoute] int id, [FromBody] WalletUpdateDto updateWalletDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var wallet = await walletRepo.GetByIdAsync(id);
        if (wallet == null)
            return NotFound(Result<WalletSummaryDto>.Failure("Wallet not found"));

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result<WalletSummaryDto>.Failure("User not authenticated"));

        if (wallet.UserId != userId)
            return BadRequest(Result<WalletSummaryDto>.Failure("You do not have permission to update this Wallet"));

        var result = await walletRepo.UpdateAsync(id, updateWalletDto);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Result<WalletSummaryDto>>> DeleteWallet([FromRoute] int id)
    {
        var wallet = await walletRepo.GetByIdAsync(id);
        if (wallet == null)
            return NotFound(Result<WalletSummaryDto>.Failure("Wallet not found"));

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result<WalletSummaryDto>.Failure("User not authenticated"));

        if (wallet.UserId != userId)
            return BadRequest(Result<WalletSummaryDto>.Failure("You do not have permission to delete this Wallet"));

        var result = await walletRepo.DeleteAsync(id);
        return Ok(result);
    }
}
