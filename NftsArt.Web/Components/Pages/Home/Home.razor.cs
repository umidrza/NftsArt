using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NftsArt.Model.Dtos.Auction;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Dtos.User;
using NftsArt.Model.Enums;
using NftsArt.Model.Mapping;

namespace NftsArt.Web.Components.Pages.Home;

public partial class Home
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] IJSRuntime JS { get; set; }

    private List<NftSummaryDto>? PopularNfts { get; set; }

    private AuctionDetailDto? Auction { get; set; }
    private NftStatus AuctionStatus { get; set; }
    private bool isTermsAccepted = true;

    private List<UserDetailDto>? Collectors { get; set; }



    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadPopularNfts();
        await LoadAuction();
        await LoadCollectors();

        if (Auction != null)
        {
            AuctionStatus = Auction.Nft.GetAuctionStatus();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("InitScript");
        }
    }

    private async Task LoadCollectors()
    {
        var res = await ApiClient.GetFromJsonAsync<List<UserDetailDto>>($"api/auth/collector");

        if (res.IsSuccess && res.Data != null)
        {
            Collectors = res.Data;
        }
    }

    private async Task LoadPopularNfts()
    {
        var res = await ApiClient.GetFromJsonAsync<List<NftSummaryDto>>($"api/nft/popular");

        if (res.IsSuccess && res.Data != null)
        {
            PopularNfts = res.Data;
        }
    }

    private async Task LoadAuction()
    {
        var res = await ApiClient.GetFromJsonAsync<AuctionDetailDto>($"api/auction/popular");

        if (res != null && res.IsSuccess)
        {
            Auction = res.Data;
        }
    }
}
