using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NftsArt.BL.Repositories;
using NftsArt.Model.Dtos.Provider;
using NftsArt.Model.Helpers;
using NftsArt.Model.Mapping;

namespace NftsArt.ApiService.Controllers
{
    [Route("api/provider")]
    [ApiController]
    public class ProviderController(IProviderRepository providerRepo) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<Result<List<ProviderSummaryDto>>>> GetProviders()
        {
            var providers = (await providerRepo.GetAllAsync())
                        .Select(c => c.ToSummaryDto()).ToList();

            return Ok(Result<List<ProviderSummaryDto>>.Success(providers));
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<Result<ProviderSummaryDto>>> GetProvider([FromRoute] int id)
        {
            var provider = await providerRepo.GetByIdAsync(id);
            if (provider == null)
                return Ok(Result<ProviderSummaryDto>.Failure("Provider not found"));

            return Ok(Result<ProviderSummaryDto>.Success(provider.ToSummaryDto()));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result<ProviderSummaryDto>>> CreateProvider([FromBody] ProviderCreateDto createProviderDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await providerRepo.CreateAsync(createProviderDto);
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result<ProviderSummaryDto>>> DeleteProvider([FromRoute] int id)
        {
            var provider = await providerRepo.GetByIdAsync(id);
            if (provider == null)
                return NotFound(Result<ProviderSummaryDto>.Failure("Provider not found"));

            var result = await providerRepo.DeleteAsync(id);
            return Ok(result);
        }
    }
}
