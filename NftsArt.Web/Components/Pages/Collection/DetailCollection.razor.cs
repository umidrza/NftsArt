﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using NftsArt.Model.Dtos.Collection;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Dtos.User;
using System.IdentityModel.Tokens.Jwt;

namespace NftsArt.Web.Components.Pages.Collection;

public partial class DetailCollection
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] IJSRuntime JS { get; set; }
    [Inject] AuthenticationStateProvider AuthStateProvider { get; set; }


    [Parameter] public int Id { get; set; }
    private CollectionDetailDto? Collection { get; set; }
    private List<NftSummaryDto>? Nfts { get; set; }

    private List<CollectorDto>? Collectors { get; set; }

    private string? UserId { get; set; }

    private bool isDataLoaded;
    private bool isScriptsInitialized;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadCollection();
        await LoadCollectionNfts();
        await LoadCollectors();
        await LoadUserId();

        if (Collection != null && UserId != null)
        {
            FollowingState = Collection.Creator.Followers.Exists(f => f.FollowerId == UserId && !f.IsDeleted);
            FollowerCount = Collection.Creator.Followers.Where(f => !f.IsDeleted).Count();
        }

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
        var res = await ApiClient.GetFromJsonAsync<List<CollectorDto>>($"api/auth/collector");

        if (res != null && res.IsSuccess && res.Data != null)
        {
            Collectors = res.Data;
        }
    }

    private async Task LoadUserId()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        UserId = authState.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
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


    private bool FollowingState = false;
    private int FollowerCount;

    private async Task HandleFollow(string userId)
    {
        var res = await ApiClient.PostAsync<FollowDto>($"api/auth/follow/{userId}", null!);

        if (res != null && res.IsSuccess && res.Data != null)
        {
            var followDto = res.Data;

            FollowingState = !followDto.IsDeleted;
            FollowerCount = !followDto.IsDeleted ? FollowerCount + 1 : FollowerCount - 1;
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
