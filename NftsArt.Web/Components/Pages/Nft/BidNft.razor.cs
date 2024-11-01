using Microsoft.AspNetCore.Components;
using NftsArt.Model.Dtos.Bid;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Helpers;
using NftsArt.Web.Services;

namespace NftsArt.Web.Components.Pages.Nft;

public partial class BidNft
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager Navigation { get; set; }
    [Inject] MessageService MessageService { get; set; }

    [Parameter] public int Id { get; set; }

    [SupplyParameterFromForm]
    private BidCreateDto BidCreateDto { get; set; } = new BidCreateDto();

    private NftDetailDto? Nft { get; set; }

    private async Task HandleSubmit()
    {
        if (Nft == null || Nft.Auction == null) return;

        var res = await ApiClient.PostAsync<BidSummaryDto, BidCreateDto>($"api/bid/{Nft.Auction.Id}", BidCreateDto);

        if (res != null && res.IsSuccess)
        {
            Navigation.NavigateTo($"/nft/{Id}");
            MessageService.ShowMessage(Message.Success(res.Message));
        }
        else
        {
            MessageService.ShowMessage(Message.Error(res?.Message ?? "Error"));
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadNft();
    }

    protected async Task LoadNft()
    {
        var res = await ApiClient.GetFromJsonAsync<NftDetailDto>($"api/nft/{Id}");

        if (res != null && res.IsSuccess)
        {
            Nft = res.Data;
        }
        else
        {
            MessageService.ShowMessage(Message.Error(res?.Message ?? "Error"));
        }
    }
}
