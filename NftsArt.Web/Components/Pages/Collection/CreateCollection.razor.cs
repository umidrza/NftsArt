using Microsoft.AspNetCore.Components;
using NftsArt.Model.Dtos.Collection;
using NftsArt.Model.Dtos.Nft;

namespace NftsArt.Web.Components.Pages.Collection;

public partial class CreateCollection
{
    [Inject] private ApiClient ApiClient { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }

    [SupplyParameterFromForm]
    public CollectionCreateDto CollectionCreateDto { get; set; } = new();

    private List<NftSummaryDto>? Nfts { get; set; }


    public async Task HandleValidSubmit()
    {
        var res = await ApiClient.PostAsync<CollectionSummaryDto, CollectionCreateDto>("api/collection", CollectionCreateDto);
        if (res != null && res.IsSuccess)
        {
            NavigationManager.NavigateTo("/collection");
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadNfts();
    }

    protected async Task LoadNfts()
    {
        var res = await ApiClient.GetFromJsonAsync<List<NftSummaryDto>>($"api/nft/my-nfts");

        if (res != null && res.IsSuccess && res.Data != null)
        {
            Nfts = res.Data;
        }
    }

    private void HandleNftChange(ChangeEventArgs e, int nftId)
    {
        bool isChecked = (bool)e.Value;

        if (isChecked)
        {
            CollectionCreateDto.Nfts.Add(nftId);
        }
        else
        {
            CollectionCreateDto.Nfts.Remove(nftId);
        }
    }
}
