using NftsArt.Model.Helpers;

namespace NftsArt.BL.Services;

public class MessageService
{
    public event Action<Message> OnMessage;

    public void ShowMessage(Message message)
    {
        OnMessage?.Invoke(message);
    }
}