using System.Text.RegularExpressions;

namespace DataDogServiceCatalog.Application.Common.Extensions.HttpResponseMessageExtensions;

public static class HttpResponseMessageExtensions
{
    public static Dictionary<string, string> ParseLinksHeader(
        this HttpResponseMessage response)
    {
        var links = new Dictionary<string, string>();

        response.Headers.TryGetValues("link", out var headers);
        if (headers == null) return links;

        var matches = Regex.Matches(
            headers.First(),
            @"<(?<url>[^>]*)>;\s+rel=""(?<link>\w+)\""");
    
        foreach(Match m in matches)
            links.Add(m.Groups["link"].Value, m.Groups["url"].Value);

        return links;
    }
}