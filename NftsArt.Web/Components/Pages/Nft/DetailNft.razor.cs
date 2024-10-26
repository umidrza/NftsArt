using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NftsArt.Model.Dtos.Auction;
using NftsArt.Model.Dtos.Bid;
using NftsArt.Model.Dtos.Collection;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Entities;
using NftsArt.Model.Enums;
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

    private bool isUserLiked = false;
    private int likeCount = 0;

    private bool isTermsAccepted = true;

    private readonly decimal EthToUsdRate = 2637.91M;
    private readonly decimal BtcToUsdRate = 68156.61M;
    private decimal UsdConversionRate;

    private Timer timer;
    private string CountdownDisplay { get; set; } = "Calculating...";

    protected override async Task OnParametersSetAsync()
    {
        if (CollectionId.HasValue)
        {
            await LoadCollection();
        }

        await LoadNft();

        if (Nft != null)
        {
            await LoadLikeStatus();
            Auction = Nft.Auction;
            NftStatus = Nft.GetAuctionStatus();
        }

        if (Auction != null)
        {
            await LoadBids();
            UsdConversionRate = Auction.Currency == Currency.BTC.ToString() ? BtcToUsdRate : EthToUsdRate;
            StartTimer();

            await JS.InvokeVoidAsync("DropdownScript");
        }

        StateHasChanged();
    }

    private async Task LoadNft()
    {
        var res = await ApiClient.GetFromJsonAsync<NftDetailDto>($"api/nft/{Id}");

        if (res.IsSuccess && res.Data != null)
        {
            Nft = res.Data;
        }
    }

    private async Task LoadCollection()
    {
        var res = await ApiClient.GetFromJsonAsync<CollectionDetailDto>($"api/collection/{CollectionId!.Value}");

        if (res.IsSuccess && res.Data != null)
        {
            Collection = res.Data;
        }
    }

    private async Task LoadBids()
    {
        if (Nft != null && Nft.Auction != null)
        {
            var res = await ApiClient.GetFromJsonAsync<List<BidSummaryDto>>($"api/auction/{Nft.Auction.Id}/bids");

            if (res.IsSuccess && res.Data != null)
            {
                Bids = res.Data;
            }
        }
    }

    private async Task LoadLikeStatus()
    {
        var countRes = await ApiClient.GetFromJsonAsync<int>($"api/nft/{Id}/likes");
        if (countRes.IsSuccess)
        {
            likeCount = countRes.Data;
        }

        var isLikedRes = await ApiClient.GetFromJsonAsync<bool>($"api/nft/{Id}/is-liked");
        if (isLikedRes.IsSuccess)
        {
            isUserLiked = isLikedRes.Data;
        }
    }

    private async Task LikeNft()
    {
        var res = await ApiClient.PostAsync<Like, object>($"api/nft/{Id}/like", null!);

        if (res.IsSuccess && res.Data != null)
        {
            likeCount++;
            isUserLiked = true;
        }
        else if (res.IsSuccess && res.Data == null)
        {
            likeCount--;
            isUserLiked = false;
        }
    }

    private void StartTimer()
    {
        timer = new Timer(UpdateCountdown, null, 0, 60000);
    }

    private void UpdateCountdown(object state)
    {
        var timeDifference = Auction.EndTime - DateTime.Now;

        if (timeDifference.TotalMilliseconds > 0)
        {
            var days = timeDifference.Days;
            var hours = timeDifference.Hours;
            var minutes = timeDifference.Minutes;

            CountdownDisplay = Auction.EndTime.Date == DateTime.Today
                ? $"{hours.ToString("D2")}h : {minutes.ToString("D2")}m"
                : $"{days}d : {hours.ToString("D2")}h : {minutes.ToString("D2")}m";
        }
        else
        {
            CountdownDisplay = "Expired";
            timer.Dispose();
        }

        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        timer?.Dispose();
    }


    private string CalculateDifference(decimal nftPrice, decimal bidAmount, int quantity)
    {
        if (nftPrice > 0 && bidAmount > 0 && quantity > 0)
        {
            decimal percentage = ((bidAmount - (nftPrice * quantity)) / nftPrice) * 100;
            return percentage > 0
                ? $"{percentage.ToString("0")}% above"
                : $"{Math.Abs(percentage).ToString("0")}% below";
        }
        return "N/A";
    }
}
