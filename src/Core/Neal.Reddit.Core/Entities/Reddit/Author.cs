namespace Neal.Reddit.Core.Entities.Reddit;

public class Author
{
    public string Name { get; set; } = string.Empty;

    public int PostCount { get => this.Posts.Count(); }

    public IEnumerable<Link> Posts { get; set; } = Enumerable.Empty<Link>();
}
