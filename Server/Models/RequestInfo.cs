using HotChocolate.Resolvers;
using System.Web;
using UAParser;

namespace Server.Models;

public class RequestInfo
{
    private readonly string[] formIgnoredKeys = new string[]
    {
        "password",
        "X-Requested-Width",
        
        // Telerik:
        // "sort",
        // "group",
        // "filter",
        // "page",
        // "pageSize"
    };

    public readonly Dictionary<string, string>? HttpFormParameters;
    public readonly Dictionary<string, string>? HttpQueryParameters;
    public readonly Dictionary<string, string>? GraphqlQueryParameters;

    /// <summary>
    /// Метод запроса (GET, POST).
    /// </summary>
    public readonly string Method;
    
    /// <summary>
    /// Отдельно, чтобы не перемешивать с HTTP path, который уже в URI.
    /// </summary>
    public readonly string? GraphqlPath;

    public readonly string URI;
    public readonly string IPAddress;
    public readonly string Device;
    public readonly string? Detail;

    /// <param name="parseForm">Должен быть false при загрузке на сервер гигантских файлов (&gt; 2GB).</param>
    public RequestInfo(HttpRequest request, string? detail = null, IMiddlewareContext? graphqlContext = null, bool parseForm = true)
    {
        Detail = detail;

        if (parseForm && request.HasFormContentType)
        {
            HttpFormParameters = request.Form.Keys
                .Where(k => !formIgnoredKeys.Any(ik => string.Compare(ik, k, true) == 0))
                .ToDictionary(k => k, k => request.Form[k].ToString());
        }

        if (request.QueryString.HasValue)
        {
            var queryValueCollection = HttpUtility.ParseQueryString(request.QueryString.Value!);
            HttpQueryParameters = queryValueCollection.AllKeys.ToDictionary(k => k!, k => queryValueCollection[k]!);
        }

        Method = request.Method;
        URI = request.GetURI();
        IPAddress = request.HttpContext.Connection.RemoteIpAddress!.ToString();

        Parser uaParser = Parser.GetDefault();
        ClientInfo ci = uaParser.Parse(request.Headers["User-Agent"].ToString());
        Device = ci.UA.Family;

        if (graphqlContext != null)
        {
            GraphqlPath = graphqlContext.Path.Print();
            GraphqlQueryParameters = graphqlContext.Variables.ToDictionary(k => k.Name.ToString(), v => v.Value.ToString());
        }
    }
}
