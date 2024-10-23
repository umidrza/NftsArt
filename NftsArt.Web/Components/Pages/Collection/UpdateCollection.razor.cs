using Microsoft.AspNetCore.Components;
using NftsArt.Model.Dtos.Collection;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Mapping;

namespace NftsArt.Web.Components.Pages.Collection;

public partial class UpdateCollection : ComponentBase
{
    [Inject] private ApiClient ApiClient { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }

    [Parameter]
    public int Id { get; set; }

    [SupplyParameterFromForm]
    public CollectionUpdateDto CollectionUpdateDto { get; set; } = new();

    private List<NftSummaryDto>? Nfts { get; set; }


    public async Task HandleValidSubmit()
    {
        var res = await ApiClient.PutAsync<CollectionSummaryDto, CollectionUpdateDto>($"/api/collection/{Id}", CollectionUpdateDto);
        if (res != null && res.IsSuccess)
        {
            NavigationManager.NavigateTo($"/collection/{Id}");
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadCollection();
        await LoadNfts();
    }

    protected async Task LoadCollection()
    {
        var res = await ApiClient.GetFromJsonAsync<CollectionDetailDto>($"/api/collection/{Id}");
        if (res != null && res.IsSuccess && res.Data != null)
        {
            CollectionUpdateDto = res.Data.ToUpdateDto();
        }
    }

    protected async Task LoadNfts()
    {
        var res = await ApiClient.GetFromJsonAsync<List<NftSummaryDto>>($"api/nft/my-nfts");

        if (res.IsSuccess && res.Data != null)
        {
            Nfts = res.Data;
        }
    }

    private void HandleNftChange(ChangeEventArgs e, int nftId)
    {
        bool isChecked = (bool)e.Value;

        if (isChecked)
        {
            CollectionUpdateDto.Nfts.Add(nftId);
        }
        else
        {
            CollectionUpdateDto.Nfts.Remove(nftId);
        }
    }

    protected async Task HandleDelete()
    {
        var res = await ApiClient.DeleteAsync<CollectionSummaryDto>($"api/collection/{Id}");

        if (res.IsSuccess)
        {
            NavigationManager.NavigateTo("/collection");
        }
    }
}
