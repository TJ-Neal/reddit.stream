namespace Neal.Reddit.Client.Models;

public record DataContainer<T> where T : class
{
    public Kind Kind { get; set; }

    public T? Data { get; set; }
}
