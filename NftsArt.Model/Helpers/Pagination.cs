namespace NftsArt.Model.Helpers;

public class Pagination<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public int Count { get; set; }
}
