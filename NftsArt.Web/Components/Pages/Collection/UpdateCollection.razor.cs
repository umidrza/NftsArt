using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using NftsArt.Model.Dtos.Collection;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Helpers;
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
        var res = await ApiClient.PutAsync<Result, CollectionUpdateDto>($"/api/collection/{Id}", CollectionUpdateDto);
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
        var res = await ApiClient.GetFromJsonAsync<Result>($"/api/collection/{Id}");
        if (res != null && res.IsSuccess)
        {
            var collectionDetailDto = JsonConvert.DeserializeObject<CollectionDetailDto>(res.Data.ToString());

            if (collectionDetailDto != null)
            {
                CollectionUpdateDto = collectionDetailDto.ToUpdateDto();
            }
        }
    }

    protected async Task LoadNfts()
    {
        var res = await ApiClient.GetFromJsonAsync<Result>($"api/nft/my-nfts");

        if (res.IsSuccess && res.Data != null)
        {
            Nfts = JsonConvert.DeserializeObject<List<NftSummaryDto>>(res.Data.ToString());
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
        var res = await ApiClient.DeleteAsync<Result>($"api/collection/{Id}");

        if (res.IsSuccess)
        {
            NavigationManager.NavigateTo("/collection");
        }
    }
}
