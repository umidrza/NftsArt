using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NftsArt.Model.Dtos.Auction;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Dtos.User;
using NftsArt.Model.Enums;

namespace NftsArt.Web.Components.Pages.Home;

public partial class Home
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] IJSRuntime JS { get; set; }

    private List<NftSummaryDto>? PopularNfts { get; set; }

    private AuctionDetailDto? Auction { get; set; }
    private NftStatus AuctionStatus { get; set; }

    private readonly decimal EthToUsdRate = 2637.91M;
    private readonly decimal BtcToUsdRate = 68156.61M;

    private decimal UsdConversionRate;

    private List<UserDetailDto>? Collectors { get; set; }

    private bool isTermsAccepted = true;


    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadCollectors();
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

    private string GetNftUrl(int collectionId, int nftId)
    {
        return $"/collection/{collectionId}/nft/{nftId}";
    }

    private string GetNftUrl(int nftId)
    {
        return $"/nft/{nftId}";
    }

    private string GetCollectionUrl(int collectionId)
    {
        return $"/collection/{collectionId}";
    }
}
