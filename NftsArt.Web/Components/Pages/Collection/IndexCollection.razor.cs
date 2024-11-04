using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using NftsArt.Model.Dtos.Collection;
using NftsArt.Model.Dtos.User;
using NftsArt.Model.Helpers;
using NftsArt.Web.Services;
using System.IdentityModel.Tokens.Jwt;

namespace NftsArt.Web.Components.Pages.Collection;

public partial class IndexCollection
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] AuthenticationStateProvider AuthStateProvider { get; set; }
    [Inject] IJSRuntime JS { get; set; }
    [Inject] MessageService MessageService { get; set; }


    private IEnumerable<CollectionDetailDto>? Collections { get; set; }
    private List<CollectorDto>? Collectors { get; set; }

    private string? UserId { get; set; }


    private bool isDataLoaded;
    private bool isScriptsInitialized;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadCollections();
        await LoadCollectors();
        await LoadUserId();

        CurrentPage = QueryModel.PageNumber;

        if (Collections != null)
        {
            foreach (var collection in Collections)
            {
                string id = collection.Creator.Id;

                FollowerCounts[id] = collection.Creator.Followers.Where(f => !f.IsDeleted).Count();

                if (UserId != null)
                {
                    FollowingStates[id] = collection.Creator.Followers.Exists(f => f.FollowerId == UserId && !f.IsDeleted);
                }
            }

        }

        isDataLoaded = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!isScriptsInitialized && isDataLoaded)
        {
            if (Collections != null)
            {
                await JS.InvokeVoidAsync("CollectionScript");
            }

            if (Collectors != null)
            {
                await JS.InvokeVoidAsync("AutoScrollScript");
            }

            isScriptsInitialized = true;
        }
    }

    private async Task LoadCollections()
    {
        var res = await ApiClient.GetFromJsonAsync<Pagination<CollectionDetailDto>>(
                $"api/collection" +
                $"?SearchTerm={QueryModel.SearchTerm}" +
                $"&Categories={string.Join(",", SelectedCategories)}" +
                $"&BlockchainName={QueryModel.BlockchainName}" +
                $"&SortBy={QueryModel.SortBy}" +
                $"&PageNumber={QueryModel.PageNumber}" +
                $"&PageSize={QueryModel.PageSize}");

        if (res != null && res.IsSuccess && res.Data != null)
        {
            Collections = res.Data.Data;
            TotalPages = (int)Math.Ceiling(res.Data.Count / (double)QueryModel.PageSize);
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

    protected async Task LoadUserId()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        if (authState == null) return;
        
        UserId = authState.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
    }


    private Dictionary<string, bool> FollowingStates = [];
    private Dictionary<string, int> FollowerCounts = [];

    private async Task HandleFollow(string userId)
    {
        if (Collections == null || UserId == null) return;

        var res = await ApiClient.PostAsync<FollowDto>($"api/auth/follow/{userId}", null!);

        if (res != null && res.IsSuccess && res.Data != null)
        {
            var followDto = res.Data;

            FollowingStates[userId] = !followDto.IsDeleted;
            FollowerCounts[userId] = !followDto.IsDeleted ? FollowerCounts[userId] + 1 : FollowerCounts[userId] - 1;
        }
        else
        {
            MessageService.ShowMessage(Message.Error(res?.Message ?? "Error"));
        }
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
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }

    private bool DisabledPrevious => CurrentPage == 1;
    private bool DisabledNext => CurrentPage == TotalPages;

    private async Task LoadPage(int pageNumber)
    {
        CurrentPage = pageNumber;
        QueryModel.PageNumber = pageNumber;
        await LoadCollections();
    }

    private async Task GoToPage(int pageNumber)
    {
        Console.WriteLine(pageNumber);
        if (pageNumber != CurrentPage)
        {
            await LoadPage(pageNumber);
        }
    }

    private async Task PreviousPage()
    {
        if (CurrentPage > 1)
        {
            await LoadPage(CurrentPage - 1);
        }
    }

    private async Task NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            await LoadPage(CurrentPage + 1);
        }
    }
}