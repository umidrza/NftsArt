using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using NftsArt.Model.Dtos.Collection;
using NftsArt.Model.Dtos.User;
using System.IdentityModel.Tokens.Jwt;

namespace NftsArt.Web.Components.Pages.Collection;

public partial class IndexCollection
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] AuthenticationStateProvider AuthStateProvider { get; set; }
    [Inject] IJSRuntime JS { get; set; }


    private List<CollectionDetailDto>? Collections { get; set; }
    private List<UserDetailDto>? Collectors { get; set; }

    private string? UserId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadCollections();
        await LoadCollectors();

        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        UserId = authState.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
    }

    private async Task LoadCollections()
    {
        var res = await ApiClient.GetFromJsonAsync<List<CollectionDetailDto>>(
                $"api/collection" +
                $"?SearchTerm={QueryModel.SearchTerm}" +
                $"&Categories={string.Join(",", SelectedCategories)}" +
                $"&BlockchainName={QueryModel.BlockchainName}" +
                $"&SortBy={QueryModel.SortBy}" +
                $"&PageNumber={QueryModel.PageNumber}" +
                $"&PageSize={QueryModel.PageSize}");

        if (res.IsSuccess && res.Data != null)
        {
            Collections = res.Data;
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


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("CollectionScript");
            await JS.InvokeVoidAsync("InitScript");
        }
    }

    private string GetCollectionUrl(int collectionId)
    {
        return $"/collection/{collectionId}";
    }

    private string GetCollectionUpdateUrl(int collectionId)
    {
        return $"/collection/{collectionId}/update";
    }



    //Filters

    private CollectionQueryDto QueryModel = new();

    private HashSet<string> SelectedCategories = [];

    protected async Task ApplyFilters()
    {
        await LoadCollections();
    }

    private async Task HandleCategoryChange(ChangeEventArgs e, string category)
    {
        if ((bool)e.Value!)
        {
            SelectedCategories.Add(category);
        }
        else
        {
            SelectedCategories.Remove(category);
        }
        await ApplyFilters();
    }
   
    private async Task HandleBlockchainChange(ChangeEventArgs e)
    {
        QueryModel.BlockchainName = e.Value?.ToString();
        await ApplyFilters();
    }

    private async Task HandleSortChange(ChangeEventArgs e)
    {
        QueryModel.SortBy = e.Value?.ToString();
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
