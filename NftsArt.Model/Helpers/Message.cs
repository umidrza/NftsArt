namespace NftsArt.Model.Helpers;

public class Message
{
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = "alert-info";
    public int Duration { get; set; } = 3000;
    public bool IsDeactivated { get; set; } = false;

    public Message() { }

    private Message(string value, string type, int duration)
    {
        Value = value;
        Type = type;
        Duration = duration;
    }

    public static Message Success(string value, int duration = 3000)
    {
        return new Message(value, "alert-success", duration);
    }

    public static Message Info(string value, int duration = 3000)
    {
        return new Message(value, "alert-info", duration);
    }

    public static Message Error(string value, int duration = 3000)
    {
        return new Message(value, "alert-error", duration);
    }
}
