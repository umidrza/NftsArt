using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NftsArt.Model.Dtos.Auction;
using NftsArt.Model.Dtos.Bid;
using NftsArt.Model.Dtos.Collection;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Entities;
using NftsArt.Model.Enums;

namespace NftsArt.Web.Components.Pages.Nft;

public partial class DetailNft
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] IJSRuntime JS { get; set; }

    [Parameter] public int Id { get; set; }
    [Parameter] public int? CollectionId { get; set; }

    private NftDetailDto? Nft { get; set; }
    private CollectionDetailDto? Collection { get; set; }
    private AuctionDetailDto? Auction { get; set; }
    private List<BidDetailDto>? Bids { get; set; }

    private bool isUserLiked = false;
    private int likeCount = 0;

    private bool isTermsAccepted = true;

    private decimal usdConversionRate;

    private Timer timer;
    private string CountdownDisplay { get; set; } = "Calculating...";

    private bool isDataLoaded;
    private bool isScriptsInitialized;

    protected override async Task OnParametersSetAsync()
    {
        await LoadNft();

        if (CollectionId.HasValue)
        {
            await LoadCollection();
        }

        if (Nft != null)
        {
            await LoadLikeStatus();
            await LoadAuction();
        }

        if (Auction != null)
        {
            await LoadBids();

            GetConversionRate();
            StartTimer();
        }

        StateHasChanged();
        isDataLoaded = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!isScriptsInitialized && isDataLoaded)
        {
            if (Bids != null)
            {
                await JS.InvokeVoidAsync("DropdownScript");
            }

            isScriptsInitialized = true;
        }
    }

    private async Task LoadNft()
    {
        var res = await ApiClient.GetFromJsonAsync<NftDetailDto>($"api/nft/{Id}");

        if (res != null && res.IsSuccess && res.Data != null)
        {
            Nft = res.Data;
        }
    }

    private async Task LoadAuction()
    {
        if (Nft != null && Nft.NftStatus != NftStatus.Not_On_Sale)
        {
            var res = await ApiClient.GetFromJsonAsync<AuctionDetailDto>($"api/auction/{Nft.Auction!.Id}");

            if (res != null && res.IsSuccess && res.Data != null)
            {
                Auction = res.Data;
            }
        }
    }


    private async Task LoadCollection()
    {
        var res = await ApiClient.GetFromJsonAsync<CollectionDetailDto>($"api/collection/{CollectionId!.Value}");

        if (res != null && res.IsSuccess && res.Data != null)
        {
            Collection = res.Data;
        }
    }

    private async Task LoadBids()
    {
        if (Nft != null && Nft.Auction != null)
        {
            var res = await ApiClient.GetFromJsonAsync<List<BidDetailDto>>($"api/auction/{Nft.Auction.Id}/bids");

            if (res != null && res.IsSuccess && res.Data != null)
            {
                Bids = res.Data;
            }
        }
    }

    private async Task LoadLikeStatus()
    {
        var countRes = await ApiClient.GetFromJsonAsync<int>($"api/nft/{Id}/likes");
        if (countRes != null && countRes.IsSuccess)
        {
            likeCount = countRes.Data;
        }

        var isLikedRes = await ApiClient.GetFromJsonAsync<bool>($"api/nft/{Id}/is-liked");
        if (isLikedRes != null && isLikedRes.IsSuccess)
        {
            isUserLiked = isLikedRes.Data;
        }
    }

    private async Task LikeNft()
    {
        var res = await ApiClient.PostAsync<Like, object>($"api/nft/{Id}/like", null!);

        if (res != null && res.IsSuccess)
        {
            if (res.Data != null)
            {
                likeCount++;
                isUserLiked = true;
            }
            else
            {
                likeCount--;
                isUserLiked = false;
            }
        }
    }

    private void GetConversionRate()
    {
        decimal EthToUsdRate = 2637.91M;
        decimal BtcToUsdRate = 68156.61M;

        usdConversionRate = Auction!.Currency.ToString() == Currency.BTC.ToString() ? BtcToUsdRate : EthToUsdRate;
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


    private bool IsTruncated { get; set; } = true;
    private string GetExtraContent(string text, int maxLength)
    {
        return IsTruncated && text.Length > maxLength
            ? text.Substring(0, maxLength) + "..."
            : text;
    }
    
    private string ButtonText => IsTruncated ? "Show more" : "Show less";
}
