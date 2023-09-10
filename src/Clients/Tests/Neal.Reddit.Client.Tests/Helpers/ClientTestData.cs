using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Enums;
using System.Collections;

namespace Neal.Reddit.Client.Tests.Helpers;

public class ClientTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { new SubredditConfiguration() };
        yield return new object[] { new SubredditConfiguration("all") };
        yield return new object[] 
        { 
            new SubredditConfiguration("all", MonitorTypes.AfterStartOnly)
        };
        yield return new object[] 
        { 
            new SubredditConfiguration()
            {
                Name = "all",
                MonitorType = MonitorTypes.None,
                Sort = Sorts.New,
            } 
        };
        yield return new object[] 
        { 
            new SubredditConfiguration()
            {
                Name = "all",
                Sort = Sorts.New,
                PerRequestLimit = 50,
            }
        };
        yield return new object[] 
        { 
            new SubredditConfiguration()
            {
                Name = "all",
                Sort = Sorts.New
            }
        };
        yield return new object[] 
        { 
            new SubredditConfiguration()
            {
                Name = "all",
                PerRequestLimit = 50,
            } 
        };
        yield return new object[] 
        { 
            new SubredditConfiguration()
            {
                Name = "all",
                MonitorType = MonitorTypes.None,
                Sort = Sorts.New,
                PerRequestLimit = 50,
            } 
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}
