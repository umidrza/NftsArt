using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Dtos.Collection;
using NftsArt.Model.Helpers;
using NftsArt.Web.Services;

namespace NftsArt.Web.Components.Pages.Nft;

public partial class CreateNft
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] MessageService MessageService { get; set; }


    [SupplyParameterFromForm]
    private NftCreateDto NftCreateDto { get; set; } = new NftCreateDto();

    private List<CollectionSummaryDto>? Collections { get; set; }

    private bool hasImage = false;
    private string uploadedImageSrc;

    private async Task HandleValidSubmit()
    {
        var res = await ApiClient.PostAsync<NftSummaryDto, NftCreateDto>("api/nft", NftCreateDto);

        if (res != null && res.IsSuccess && res.Data != null)
        {
            var nft = res.Data;
            NavigationManager.NavigateTo($"/nft/{nft.Id}");

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
        await LoadCollections();
    }

    protected async Task LoadCollections()
    {
        var res = await ApiClient.GetFromJsonAsync<List<CollectionSummaryDto>>($"api/collection/my-collections");

        if (res != null && res.IsSuccess && res.Data != null)
        {
            Collections = res.Data;
        }
        else
        {
            MessageService.ShowMessage(Message.Error(res?.Message ?? "Error"));
        }
    }

    private async Task HandleImageChange(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file != null)
        {
            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);

            var streamContent = new StreamContent(fileStream);
            content.Add(streamContent, "file", file.Name);

            var res = await ApiClient.PostAsync<string>("api/nft/upload", content);

            if (res != null && res.IsSuccess && res.Data != null)
            {
                uploadedImageSrc = res.Data;
                NftCreateDto.ImageUrl = res.Data;
                hasImage = true;
            }
            else
            {
                hasImage = false;
                MessageService.ShowMessage(Message.Error(res?.Message ?? "Error"));
            }
        }
        else
        {
            hasImage = false;
        }
    }

    private void HandleCollectionChange(ChangeEventArgs e, int collectionId)
    {
        bool isChecked = (bool)e.Value;

        if (isChecked)
        {
            NftCreateDto.Collections.Add(collectionId);
        }
        else
        {
            NftCreateDto.Collections.Remove(collectionId);
        }
    }
}
