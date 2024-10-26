using NftsArt.Model.Dtos.Provider;
using NftsArt.Model.Entities;

namespace NftsArt.Model.Mapping;

public static class ProviderMapping
{
    public static Provider ToEntity(this ProviderCreateDto providerCreateDto)
    {
        return new Provider
        {
            Name = providerCreateDto.Name,
            Image = providerCreateDto.ImageUrl,
        };
    }

    public static ProviderSummaryDto ToSummaryDto(this Provider provider)
    {
        return new ProviderSummaryDto
            (
                provider.Id,
                provider.Name,
                provider.Image
            );
    }
}

