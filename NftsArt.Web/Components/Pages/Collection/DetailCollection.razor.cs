using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NftsArt.Model.Dtos.Collection;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Dtos.User;
using NftsArt.Model.Entities;

namespace NftsArt.Web.Components.Pages.Collection;

public partial class DetailCollection
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] IJSRuntime JS { get; set; }


    [Parameter]
    public int Id { get; set; }
    private CollectionDetailDto? Collection { get; set; }
    private List<NftSummaryDto>? Nfts { get; set; }

    private List<UserDetailDto>? Collectors { get; set; }

    private bool isDataLoaded;
    private bool isScriptsInitialized;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadCollection();
        await LoadCollectionNfts();
        await LoadCollectors();

        isDataLoaded = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!isScriptsInitialized && isDataLoaded)
        {
            if (Collection != null && Nfts != null)
            {
                await JS.InvokeVoidAsync("CollectionScript");
                await JS.InvokeVoidAsync("DropdownScript");
            }

            if (Collectors != null)
            {
                await JS.InvokeVoidAsync("AutoScrollScript");
            }

            isScriptsInitialized = true;
        }
    }

    protected async Task LoadCollection()
    {
        var res = await ApiClient.GetFromJsonAsync<CollectionDetailDto>($"api/collection/{Id}");

        if (res != null && res.IsSuccess && res.Data != null)
        {
            Collection = res.Data;
        }
    }

    protected async Task LoadCollectionNfts()
    {
        var res = await ApiClient.GetFromJsonAsync<List<NftSummaryDto>>(
            $"api/collection/{Id}/nfts" +
            $"?SearchTerm={QueryModel.SearchTerm}" +
            $"&Statuses={string.Join(",", SelectedStatuses)}" +
            $"&CurrencyName={QueryModel.CurrencyName}" +
            $"&Quantity={QueryModel.Quantity}" +
            $"&MinPrice={QueryModel.MinPrice}" +
            $"&MaxPrice={QueryModel.MaxPrice}" +
            $"&SortBy={QueryModel.SortBy}" +
            $"&PageNumber={QueryModel.PageNumber}" +
            $"&PageSize={QueryModel.PageSize}");

        if (res != null && res.IsSuccess && res.Data != null)
        {
            Nfts = res.Data;
        }
    }

    private async Task LoadCollectors()
    {
        var res = await ApiClient.GetFromJsonAsync<List<UserDetailDto>>($"api/auth/collector");

        if (res != null && res.IsSuccess && res.Data != null)
        {
            Collectors = res.Data;
        }
    }

    private string CalculateCountdown(DateTime auctionEndTime)
    {
        var timeDifference = auctionEndTime - DateTime.Now;

        if (timeDifference.TotalMilliseconds > 0)
        {
            var days = timeDifference.Days;
            var hours = timeDifference.Hours;
            var minutes = timeDifference.Minutes;

            var countdownDisplay = $"{days}d : {hours:D2}h : {minutes:D2}m";

            return countdownDisplay;
        }
        else
        {
            return "Expired";
        }
    }


    //Filters

    private NftQueryDto QueryModel = new();

    private HashSet<string> SelectedStatuses = [];

    protected async Task ApplyFilters()
    {
        await LoadCollectionNfts();
    }

    private async Task HandleStatusChange(ChangeEventArgs e, string status)
    {
        if ((bool)e.Value!)
        {
            SelectedStatuses.Add(status);
        }
        else
        {
            SelectedStatuses.Remove(status);
        }
        await ApplyFilters();
    }

    private async Task HandleCurrencyChange(string currency)
    {
        QueryModel.CurrencyName = currency;
        await ApplyFilters();
    }

    private async Task HandleQuantityChange(string quantity)
    {
        QueryModel.Quantity = quantity;
        await ApplyFilters();
    }

    private async Task HandleSortChange(ChangeEventArgs e)
    {
        QueryModel.SortBy = e.Value?.ToString();
        await ApplyFilters();
    }

    private async Task HandleMinValueChange(ChangeEventArgs e)
    {
        QueryModel.MinPrice = e.Value?.ToString();
        await ApplyFilters();
    }

    private async Task HandleMaxValueChange(ChangeEventArgs e)
    {
        QueryModel.MaxPrice = e.Value?.ToString();
        await ApplyFilters();
    }

    private async Task HandleSearchChange(ChangeEventArgs e)
    {
        QueryModel.SearchTerm = e.Value?.ToString();
        await ApplyFilters();
    }

    private async Task ClearSearch()
    {
        QueryModel.SearchTerm = string.Empty;
        await ApplyFilters();
    }


    //Pagination
    [Parameter] public int CurrentPage { get; set; }
    [Parameter] public int TotalPages { get; set; }
    [Parameter] public EventCallback<int> OnPageChanged { get; set; }

    private bool DisabledPrevious => CurrentPage == 1;
    private bool DisabledNext => CurrentPage == TotalPages;

    private async Task GoToPage(int pageNumber)
    {
        if (pageNumber != CurrentPage)
        {
            CurrentPage = pageNumber;
            await OnPageChanged.InvokeAsync(CurrentPage);
        }
    }

    private async Task PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await OnPageChanged.InvokeAsync(CurrentPage);
        }
    }

    private async Task NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await OnPageChanged.InvokeAsync(CurrentPage);
        }
    }
}
