using Neal.Reddit.Application.Interfaces;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Client.Faster.Interfaces;

/// <summary>
/// Represents a wrapper for events produced for the FasterKV client and repository.
/// </summary>
public interface IFasterProducerWrapper : IProducerWrapper<Link>
{
}
