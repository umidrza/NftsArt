using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using NftsArt.Model.Dtos.Auction;
using NftsArt.Model.Dtos.Collection;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Helpers;

namespace NftsArt.Web.Components.Pages.Nft;

public partial class SellNft
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] IJSRuntime JS {  get; set; }

    [Parameter] public int Id { get; set; }

    [SupplyParameterFromForm]
    private AuctionCreateDto AuctionCreateDto { get; set; } = new AuctionCreateDto();

    private NftDetailDto? Nft { get; set; }

    private async Task HandleValidSubmit()
    {
        var res = await ApiClient.PostAsync<Result, AuctionCreateDto>($"api/auction/{Id}", AuctionCreateDto);

        if (res.IsSuccess)
        {
            NavigationManager.NavigateTo($"/nft/{Id}");
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadNft();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("NftSellScript");
        }
    }

    protected async Task LoadNft()
    {
        var res = await ApiClient.GetFromJsonAsync<Result>($"api/nft/{Id}");

        if (res.IsSuccess && res.Data != null)
        {
            Nft = JsonConvert.DeserializeObject<NftDetailDto>(res.Data.ToString());
        }
    }
}
