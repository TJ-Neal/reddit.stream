using Neal.Reddit.Core.Enums;

namespace Neal.Reddit.Core.Entities.Reddit;

public record DataContainer<T> where T : class
{
    public Kind Kind { get; set; }

    public T? Data { get; set; }
}
