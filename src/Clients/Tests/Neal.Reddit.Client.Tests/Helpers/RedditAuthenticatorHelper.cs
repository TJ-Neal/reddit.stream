using Neal.Reddit.Client.Models;
using RestSharp;

namespace Neal.Reddit.Client.Tests.Helpers;

internal class RedditAuthenticatorHelper : RedditAuthenticator
{
    public RedditAuthenticatorHelper(
        Credentials credentials) : base(credentials)
    {
    }

    internal async ValueTask<Parameter> GetAuthenticationParameter() =>
        await this.GetAuthenticationParameter(string.Empty);
}