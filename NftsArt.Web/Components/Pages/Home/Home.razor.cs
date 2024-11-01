using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NftsArt.Model.Dtos.Auction;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Dtos.User;

namespace NftsArt.Web.Components.Pages.Home;

public partial class Home : IDisposable
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] IJSRuntime JS { get; set; }

    private List<NftSummaryDto>? PopularNfts { get; set; }

    private AuctionDetailDto? Auction { get; set; }
    private bool isTermsAccepted = true;

    private List<CollectorDto>? Collectors { get; set; }

    private bool isDataLoaded;
    private bool isScriptsInitialized;

    private Timer timer;
    private string CountdownDisplay { get; set; } = "Calculating...";

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadPopularNfts();
        await LoadAuction();
        await LoadCollectors();

        if (Auction != null)
        {
            StartTimer();
        }

        isDataLoaded = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("DropdownScript");
        }

        if (!isScriptsInitialized && isDataLoaded)
        {
            if (Collectors != null)
            {
                await JS.InvokeVoidAsync("AutoScrollScript");
            }

            isScriptsInitialized = true;
        }
    }

    private async Task LoadCollectors()
    {
        var res = await ApiClient.GetFromJsonAsync<List<CollectorDto>>($"api/auth/collector");

        if (res != null && res.IsSuccess && res.Data != null)
        {
            Collectors = res.Data;
        }
    }

    private async Task LoadPopularNfts()
    {
        var res = await ApiClient.GetFromJsonAsync<List<NftSummaryDto>>($"api/nft/popular");

        if (res != null && res.IsSuccess && res.Data != null)
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
}
