using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using NftsArt.Model.Dtos.Auction;
using NftsArt.Model.Dtos.Bid;
using NftsArt.Model.Dtos.Collection;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Entities;
using NftsArt.Model.Enums;
using NftsArt.Model.Helpers;
using NftsArt.Web.Services;
using System.IdentityModel.Tokens.Jwt;

namespace NftsArt.Web.Components.Pages.Nft;

public partial class DetailNft
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] AuthenticationStateProvider AuthStateProvider { get; set; }
    [Inject] IJSRuntime JS { get; set; }
    [Inject] NavigationManager Navigation { get; set; }
    [Inject] MessageService MessageService { get; set; }


    [Parameter] public int Id { get; set; }
    [Parameter] public int? CollectionId { get; set; }

    private NftDetailDto? Nft { get; set; }
    private CollectionDetailDto? Collection { get; set; }
    private AuctionDetailDto? Auction { get; set; }
    private List<BidDetailDto>? Bids { get; set; }
    private string? UserId { get; set; }

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
        await LoadUserId();

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

        if (Bids != null && Bids.Count > 2)
        {
            FilterBidChart("4");
            UpdateBidChart();
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
        else
        {
            MessageService.ShowMessage(Message.Error(res?.Message ?? "Error"));
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
        else
        {
            MessageService.ShowMessage(Message.Error(countRes?.Message ?? "Error"));
        }

        if (UserId != null)
        {
            var isLikedRes = await ApiClient.GetFromJsonAsync<bool>($"api/nft/{Id}/is-liked");
            if (isLikedRes != null && isLikedRes.IsSuccess)
            {
                isUserLiked = isLikedRes.Data;
            }
            else
            {
                MessageService.ShowMessage(Message.Error(isLikedRes?.Message ?? "Error"));
            }
        }
    }

    private async Task LoadUserId()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        if (authState == null) return;

        UserId = authState.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
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
        else
        {
            MessageService.ShowMessage(Message.Error(res?.Message ?? "Error"));
        }
    }

    private async Task DeleteAuction()
    {
        if (!isTermsAccepted) return;
        if (Nft == null || Nft.Auction == null) return;
        if (Bids != null && Bids.Count > 0)
        {
            MessageService.ShowMessage(Message.Error("You can't delete live auction"));
        }

        var res = await ApiClient.DeleteAsync<AuctionSummaryDto>($"api/auction/{Nft.Auction.Id}");

        if (res != null && res.IsSuccess)
        {
            Navigation.NavigateTo($"/nft/{Id}", true);
            MessageService.ShowMessage(Message.Success("Auction deleted successfully!"));
        }
        else
        {
            MessageService.ShowMessage(Message.Error(res?.Message ?? "Error"));
        }
    }

    private async Task HandlePurchase()
    {
        if (!isTermsAccepted) return;
        if (Nft == null || Nft.Auction == null) return;

        var res = await ApiClient.PostAsync<AuctionSummaryDto, int>($"api/auction/{Nft.Auction.Id}/purchase", 1);

        if (res != null && res.IsSuccess)
        {
            Navigation.NavigateTo($"/nft/{Id}", true);
            MessageService.ShowMessage(Message.Success("Nft purchased successfully!"));
        }
        else
        {
            MessageService.ShowMessage(Message.Error(res?.Message ?? "Error"));
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

    private string GetCountdownDays(DateTime endDate)
    {
        var timeDifference = endDate - DateTime.Now;

        if (timeDifference.TotalMilliseconds > 0)
        {
            var days = timeDifference.Days;
            return $"{days} days";
        }
        else
        {
            return "Expired";
        }
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


    private string ButtonText => IsTruncated ? "Show more" : "Show less";
    private bool IsTruncated { get; set; } = true;
    private string GetExtraContent(string text, int maxLength)
    {
        return IsTruncated && text.Length > maxLength
            ? text.Substring(0, maxLength) + "..."
            : text;
    }
    


    // Bids chart 
    private List<decimal> FilteredBidAmounts { get; set; } = new();
    private List<DateTime> FilteredBidTimestamps { get; set; } = new();

    private List<string> DisplayedPrices { get; set; } = new();
    private List<DateTime> DisplayedDates { get; set; } = new();

    private string PathData1 { get; set; } = string.Empty;
    private string PathData2 { get; set; } = string.Empty;

    private void OnRangeChanged(ChangeEventArgs e)
    {
        string selectedRange = e.Value?.ToString() ?? "4";
        FilterBidChart(selectedRange);
        UpdateBidChart();
    }

    private void FilterBidChart(string range)
    {
        DateTime now = DateTime.Now;
        DateTime threshold = range switch
        {
            "1" => now.AddDays(-7),
            "2" => now.AddMonths(-1),
            "3" => now.AddYears(-1),
            _ => DateTime.MinValue
        };

        var filteredBids = Bids!.Where(bid => bid.StartTime >= threshold).ToList();

        FilteredBidAmounts = filteredBids.Select(bid => bid.Amount).ToList();
        FilteredBidTimestamps = filteredBids.Select(bid => bid.StartTime).ToList();
    }

    private void UpdateBidChart()
    {
        if (FilteredBidAmounts.Count == 0) return;

        decimal maxBidAmount = FilteredBidAmounts.Max();
        decimal minBidAmount = FilteredBidAmounts.Min();

        PathData1 = "M";
        PathData2 = "M";

        for (int i = 0; i < FilteredBidAmounts.Count; i++)
        {
            decimal amount = FilteredBidAmounts[i];
            float x = i * (650f / (FilteredBidAmounts.Count - 1));
            float y = 200f - ((float)((amount - minBidAmount) / (maxBidAmount - minBidAmount)) * 200f);

            if (i == 0)
            {
                PathData1 += $"{x} {y}";
                PathData2 += $"{x} {y}";
            }
            else
            {
                PathData1 += $" L{x} {y}";
                PathData2 += $" L{x} {y}";
            }
        }

        DisplayedPrices = Enumerable.Range(0, 6)
            .Select(i => minBidAmount + i * (maxBidAmount - minBidAmount) / 5)
            .Select(p => p.ToString("F1"))
            .Reverse()
            .ToList();

        DisplayedDates = FilteredBidTimestamps
            .Where((_, i) => i % Math.Max(1, FilteredBidTimestamps.Count / 6) == 0)
            .ToList();
    }
}
