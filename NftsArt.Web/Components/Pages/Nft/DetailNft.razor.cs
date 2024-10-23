using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using NftsArt.Model.Dtos.Auction;
using NftsArt.Model.Dtos.Bid;
using NftsArt.Model.Dtos.Collection;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Enums;
using NftsArt.Model.Helpers;
using NftsArt.Model.Mapping;

namespace NftsArt.Web.Components.Pages.Nft;

public partial class DetailNft
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] IJSRuntime JS { get; set; }

    [Parameter] public int Id { get; set; }
    [Parameter] public int? CollectionId { get; set; }

    private NftDetailDto? Nft { get; set; }
    private CollectionDetailDto? Collection { get; set; }
    private AuctionSummaryDto? Auction { get; set; }
    private List<BidSummaryDto>? Bids { get; set; }
    private NftStatus NftStatus { get; set; }
    private NftLikeDto LikeStatus { get; set; } = new NftLikeDto();


    private bool isTermsAccepted = true;

    private readonly decimal EthToUsdRate = 2637.91M;
    private readonly decimal BtcToUsdRate = 68156.61M;

    private decimal UsdConversionRate; 

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadNft();
        await LoadLikeStatus();
        
        if (Nft != null)
        {
            Auction = Nft.Auction;
            NftStatus = Nft.GetAuctionStatus();
        }

        if (Auction != null)
        {
            await LoadBids();
            UsdConversionRate = Auction.Currency == Currency.BTC.ToString() ? BtcToUsdRate : EthToUsdRate;
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (CollectionId.HasValue)
        {
            await LoadCollection();
        }
    }

    private async Task LoadNft()
    {
        var res = await ApiClient.GetFromJsonAsync<Result>($"api/nft/{Id}");

        if (res.IsSuccess && res.Data != null)
        {
            Nft = JsonConvert.DeserializeObject<NftDetailDto>(res.Data.ToString());
        }
    }

    private async Task LoadCollection()
    {
        var res = await ApiClient.GetFromJsonAsync<Result>($"api/collection/{CollectionId!.Value}");

        if (res.IsSuccess && res.Data != null)
        {
            Collection = JsonConvert.DeserializeObject<CollectionDetailDto>(res.Data.ToString());
        }
    }

    private async Task LoadBids()
    {
        if (Nft != null && Nft.Auction != null)
        {
            var res = await ApiClient.GetFromJsonAsync<Result>($"api/auction/{Nft.Auction.Id}/bids");

            if (res.IsSuccess && res.Data != null)
            {
                Bids = JsonConvert.DeserializeObject<List<BidSummaryDto>>(res.Data.ToString());
            }
        }
    }

    private async Task LoadLikeStatus()
    {
        var countRes = await ApiClient.GetFromJsonAsync<Result>($"api/nft/{Id}/likes");
        if (countRes.IsSuccess && countRes.Data != null)
        {
            var NftLikeDto = JsonConvert.DeserializeObject<NftLikeDto>(countRes.Data.ToString());
            LikeStatus.LikeCount = NftLikeDto.LikeCount;
        }

        var isLikedRes = await ApiClient.GetFromJsonAsync<Result>($"api/nft/{Id}/is-liked");
        if (isLikedRes.IsSuccess && isLikedRes.Data != null)
        {
            var NftLikeDto = JsonConvert.DeserializeObject<NftLikeDto>(isLikedRes.Data.ToString());
            LikeStatus.HasUserLiked = NftLikeDto.HasUserLiked;
        }
    }

    private async Task LikeNft()
    {
        var res = await ApiClient.PostAsync<Result>($"api/nft/{Id}/like");

        if (res.IsSuccess && res.Data != null)
        {
            LikeStatus.LikeCount++;
            LikeStatus.HasUserLiked = true;
        }
        else if (res.IsSuccess && res.Data == null)
        {
            LikeStatus.LikeCount--;
            LikeStatus.HasUserLiked = false;
        }
    }


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("InitScript");
        }
    }

    private string GetNftSellUrl()
    {
        return $"/nft/{Id}/sell";
    }

    private string GetCollectionUrl()
    {
        return $"/collection/{Collection?.Id}";
    }
}
