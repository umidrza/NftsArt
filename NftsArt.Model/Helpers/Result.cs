namespace NftsArt.Model.Helpers;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }

    public Result() { }

    private Result(bool isSuccess, string message, T data)
    {
        IsSuccess = isSuccess;
        Message = message;
        Data = data;
    }

    public static Result<T> Success(T data = default!, string message = "Operation succeeded")
    {
        return new Result<T>(true, message, data);
    }

    public static Result<T> Failure(string message)
    {
        return new Result<T>(false, message, default!);
    }
}
