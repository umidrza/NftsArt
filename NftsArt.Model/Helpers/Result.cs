namespace NftsArt.Model.Helpers;

public class Result
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }

    public Result() { }

    private Result(bool isSuccess, string message, object data)
    {
        IsSuccess = isSuccess;
        Message = message;
        Data = data;
    }

    public static Result Success(object data, string message = "Operation succeeded")
    {
        return new Result(true, message, data);
    }

    public static Result Failure(string message)
    {
        return new Result(false, message, null!);
    }
}
