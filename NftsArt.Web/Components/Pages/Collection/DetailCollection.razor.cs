using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using NftsArt.Model.Dtos.Collection;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Dtos.User;
using NftsArt.Model.Helpers;

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

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadCollection();
        await LoadCollectionNfts();
        await LoadCollectors();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("CollectionScript");
            await JS.InvokeVoidAsync("InitScript");
        }
    }

    private string GetNftUrl(int nftId)
    {
        return $"/collection/{Id}/nft/{nftId}";
    }

    protected async Task LoadCollection()
    {
        var res = await ApiClient.GetFromJsonAsync<Result>($"api/collection/{Id}");

        if (res.IsSuccess && res.Data != null)
        {
            Collection = JsonConvert.DeserializeObject<CollectionDetailDto>(res.Data.ToString());
        }
    }

    protected async Task LoadCollectionNfts()
    {
        var res = await ApiClient.GetFromJsonAsync<Result>(
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

        if (res.IsSuccess && res.Data != null)
        {
            Nfts = JsonConvert.DeserializeObject<List<NftSummaryDto>>(res.Data.ToString());
        }
    }

    private async Task LoadCollectors()
    {
        var res = await ApiClient.GetFromJsonAsync<Result>($"api/auth/collector");

        if (res.IsSuccess && res.Data != null)
        {
            Collectors = JsonConvert.DeserializeObject<List<UserDetailDto>>(res.Data.ToString());
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
