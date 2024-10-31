using Microsoft.AspNetCore.Components;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Dtos.Auction;
using Microsoft.JSInterop;
using System.Globalization;
using NftsArt.Model.Mapping;

namespace NftsArt.Web.Components.Pages.Nft;

public partial class UpdateNft
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] IJSRuntime JS { get; set; }
    [Inject] NavigationManager Navigation { get; set; }

    [Parameter] public int Id { get; set; }

    [SupplyParameterFromForm]
    private AuctionUpdateDto AuctionUpdateDto { get; set; } = new AuctionUpdateDto();

    private NftDetailDto? Nft { get; set; }

    private async Task HandleValidSubmit()
    {
        var res = await ApiClient.PutAsync<AuctionSummaryDto, AuctionUpdateDto>($"api/auction/{Nft!.Auction!.Id}", AuctionUpdateDto);

        if (res != null && res.IsSuccess)
        {
            Navigation.NavigateTo($"/nft/{Id}");
        }
        
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadNft();

        if (Nft != null && Nft.Auction != null)
        {
            AuctionUpdateDto = Nft.Auction.ToUpdateDto();
        }
    }

    protected async Task LoadNft()
    {
        var res = await ApiClient.GetFromJsonAsync<NftDetailDto>($"api/nft/{Id}");

        if (res != null && res.IsSuccess)
        {
            Nft = res.Data;
        }
    }


    private string _selectedSchedule = "6-month";
    public string SelectedSchedule
    {
        get => _selectedSchedule;
        set
        {
            _selectedSchedule = value;
            UpdateEndDate();
        }
    }
    private DateTime MinEndDate => AuctionUpdateDto.StartTime.AddDays(1);
    private void UpdateEndDate()
    {
        var scheduleParts = SelectedSchedule.Split('-');
        int duration = int.Parse(scheduleParts[0], CultureInfo.InvariantCulture);
        string unit = scheduleParts[1];

        AuctionUpdateDto.EndTime = unit switch
        {
            "month" => AuctionUpdateDto.StartTime.AddMonths(duration),
            "year" => AuctionUpdateDto.StartTime.AddYears(duration),
            _ => AuctionUpdateDto.EndTime
        };
    }
}
