using Neal.Reddit.Application.Interfaces;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Client.Simple.Interfaces;

/// <summary>
/// Represents a wrapper for events produced for the Simple client and repository.
/// </summary>
public interface ISimpleProducerWrapper : IProducerWrapper<Link>
{
}