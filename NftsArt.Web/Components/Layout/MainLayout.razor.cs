using Microsoft.AspNetCore.Components;
using NftsArt.Model.Helpers;
using NftsArt.Web.Services;

namespace NftsArt.Web.Components.Layout;

public partial class MainLayout : IDisposable
{
    [Inject] MessageService MessageService { get; set; }

    protected List<Message> Messages { get; set; } = [];

    protected void ShowMessage(Message message)
    {
        Messages.Add(message);
        StateHasChanged();

        _ = Task.Delay(message.Duration).ContinueWith(task =>
        {
            InvokeAsync(() =>
            {
                message.IsDeactivated = true;
                StateHasChanged();

                _ = Task.Delay(500).ContinueWith(_ =>
                {
                    InvokeAsync(() => ClearMessage(message));
                });
            });
        });

    }

    protected void ClearMessage(Message message)
    {
        Messages.Remove(message);
        StateHasChanged();
    }


    protected override void OnInitialized()
    {
        MessageService.OnMessage += message => ShowMessage(message);
    }

    public void Dispose()
    {
        MessageService.OnMessage -= ShowMessage;
    }
}
