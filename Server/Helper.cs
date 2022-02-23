using DAL;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;
using Serilog;
using Server.Authorization;
using System.DirectoryServices.Protocols;
using Server.Injections;
using Server.Models;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

/// <summary>
/// Методы расширения и общие инструменты.
/// </summary>
public static class Helper
{
    public const string AuthorizationCookieProtectionPurpose = "auth-cookie";
    public const string JsonDateFormat = "yyyy.MM.dd HH.mm.ss.fff";
    private const string formatOfForbidden = "One of the following roles is required: {0}.";
    private delegate bool HasDomainGroup(string group);
    
    public static string GetURI(this HttpRequest request)
    {
        return string.Concat(
            request.Scheme,
            "://",
            request.Host,
            request.Path,
            request.QueryString.Value);
    }

    public static KeyValuePair<HttpStatusCode, string?> IsAvailable(this Descriptor descriptor, 
        string path,
        HttpContext httpContext,
        Settings settings, 
        IDataProtector protector,
        bool pathOfGraphql = false)
    {
        HasDomainGroup hasDomainGroup;
        string userName;

        #region Authentication with cookie
        User? userFromCookie = httpContext.Request.GetUserFromCookie(protector);

        if (descriptor.UseCookie && userFromCookie != null)
        {
            userName = userFromCookie.IdentityName;
            hasDomainGroup = userFromCookie.HasGroup;
        }
        else if (httpContext.User.Identity?.Name != null)
        {
            userName = httpContext.User.Identity.Name;
            hasDomainGroup = httpContext.User.IsInRole;
        }
        else
        {
            return new KeyValuePair<HttpStatusCode, string?>(HttpStatusCode.Unauthorized, null);
        }
        #endregion

        #region Authorization
        KeyValuePair<HttpStatusCode, string?> availableResult = new KeyValuePair<HttpStatusCode, string?>(HttpStatusCode.OK, null);

        if (settings.Args.Any(a => a.Key == "su" && string.Compare(a.Value, userName, true) == 0))
        {
            return availableResult;
        }

        Role[] requiredRoles = (pathOfGraphql
                ? descriptor.Roles.Where(r => r.GraphqlPaths.Any(p => string.Compare(p, path, true) == 0))  
                : descriptor.Roles.Where(r => r.HttpPaths.Any(p => string.Compare(p, path, true) == 0))
            ).ToArray();

        if (requiredRoles.Length == 0)
        {
            return availableResult;
        }

        string[] requiredGroups = requiredRoles
            .SelectMany(r => r.Groups)
            .Distinct()
            .ToArray();
        
        if (requiredGroups.Length == 0)
        {
            return availableResult;
        }

        for (int i = 0; i < requiredGroups.Length; i++)
        {
            if (hasDomainGroup(requiredGroups[i])) 
            {
                return availableResult;
            }
        }

        string requiredRolesString = string.Join(", ", requiredRoles.Select(r => $"'{r.Name}' ({r.Description})"));
        return new KeyValuePair<HttpStatusCode, string?>(HttpStatusCode.Forbidden, string.Format(formatOfForbidden, requiredRolesString));
        #endregion
    }

    public static async Task<KeyValuePair<HttpStatusCode, string?>> IsAvailableAsync(this Descriptor descriptor,
        string path,
        HttpContext httpContext,
        Settings settings,
        IDataProtector protector,
        bool pathOfGraphql = false)
    {
        return await Task.Run(() => descriptor.IsAvailable(path, httpContext, settings, protector, pathOfGraphql));
    }

    public static AuditEvent GetAuditRequestEvent(this AppDbContext dbContext, 
        HttpContext httpContext, 
        string? detail, 
        IMiddlewareContext? graphqlContext = null, 
        HttpStatusCode code = HttpStatusCode.OK,
        bool parseRequestForm = true)
    {
        RequestInfo i = new RequestInfo(httpContext.Request, detail, graphqlContext, parseRequestForm);

        AuditEvent e = new AuditEvent();
        e.HttpStatusCode = (int)code;
        e.Object = httpContext.Request.GetURI();
        e.Source = httpContext.User.Identity?.Name;
        e.SourceDevice = i.Device;
        e.SourceIPAddress = i.IPAddress;
        e.UtcDateTime = DateTime.UtcNow;

        if (graphqlContext != null)
        {
            e.Object += graphqlContext.Path.Print();
        }
        
        e.Description = JsonConvert.SerializeObject(i, new JsonSerializerSettings
        {
            DateFormatString = JsonDateFormat,
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        });

        return e;
    }

    public static void WriteAuditEvent(this AppDbContext dbContext, 
        HttpContext httpContext, 
        string? detail, 
        IMiddlewareContext? graphqlContext = null, 
        HttpStatusCode code = HttpStatusCode.OK,
        bool parseRequestForm = true)
    {
        dbContext.AuditEvents.Add(dbContext.GetAuditRequestEvent(httpContext, detail, graphqlContext, code, parseRequestForm));
        dbContext.SaveChanges();
    }

    public static async Task WriteAuditEventAsync(this AppDbContext dbContext, 
        HttpContext httpContext, 
        string? detail, 
        IMiddlewareContext? graphqlContext = null, 
        HttpStatusCode code = HttpStatusCode.OK,
        bool parseRequestForm = true)
    {
        await dbContext.AuditEvents.AddAsync(dbContext.GetAuditRequestEvent(httpContext, detail, graphqlContext, code, parseRequestForm));
        await dbContext.SaveChangesAsync();
    }

    public static User? GetUserFromCookie(this HttpRequest request, IDataProtector protector)
    {
        if (request.Cookies.TryGetValue("auth", out string? userJsonSecret)
            && !string.IsNullOrEmpty(userJsonSecret))
        {
            User? user = null;

            try
            {
                string userJson = protector.Unprotect(userJsonSecret);
                user = JsonConvert.DeserializeObject<User>(userJson);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"An error occurred deserializing authorization cookies. Error: {ex.Message}");
            }
            
            if (user == null)
            {
                Log.Logger.Error("Deserialization of authorization cookies returned null.");
                return null;
            }

            Log.Logger.Information("The data from the authorization cookies were received successfully.");

            return user;
        }
        else
        {
            Log.Logger.Error("The request does not contain authorization cookie.");
            return null;
        }
    }

    public static User GetUserFromDC(this IIdentity? identity, int ldapTimeout = 3, Ldap? ldap = null)
    {
        if (string.IsNullOrEmpty(identity?.Name))
        {
            throw new ArgumentNullException(nameof(identity));
        }

        string sAMAccountName = identity.GetSAMAccountName();
        
        User user = new User();
        user.IdentityName = identity.Name;

        #region Encoding
        Encoding encoding;
        if (ldap == null)
        {
            encoding = Encoding.ASCII;
        }
        else
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            encoding = Encoding.GetEncoding(ldap.CodePage);
        }
        #endregion

        try
        {
            using (LdapConnection ldapConnection = ldap == null
                ? new LdapConnection(new LdapDirectoryIdentifier(string.Empty))
                : ldap.GetLdapConnection())
            {
                
                TimeSpan timeout = TimeSpan.FromSeconds(ldapTimeout);

                // ldapConnection.Timeout = timeout;
                // ldapConnection.SessionOptions.AutoReconnect = false;
                // ldapConnection.SessionOptions.LocatorFlag = LocatorFlags.OnlyLdapNeeded;
                // ldapConnection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;
                // ldapConnection.SessionOptions.SendTimeout = timeout;

                ldapConnection.Bind();
                
                SearchRequest searchRequest;
                SearchResponse searchResponse;
                
                // 1. Определение base из КД, определившего групповую политику для запускающего пользователя.
                searchRequest = new SearchRequest(null, 
                    "(objectClass=*)", 
                    SearchScope.Base, 
                    new string[] { "defaultNamingContext" });     
                searchRequest.SizeLimit = 1;
                searchResponse = (SearchResponse)ldapConnection.SendRequest(searchRequest, timeout);
                user.Base = searchResponse.Entries[0].Attributes["defaultNamingContext"][0].ToString(); // Всегда на латинице.

                // 2. Определение ФИО и групп.
                searchRequest = new SearchRequest(user.Base, 
                    $"(&(objectCategory=person)(objectClass=user)(sAMAccountName={sAMAccountName}))", 
                    SearchScope.Subtree,
                    new string[] { "CN", "memberOf" });              
                searchRequest.SizeLimit = 1;
                searchResponse = (SearchResponse)ldapConnection.SendRequest(searchRequest, timeout);

                user.DisplayName = encoding.GetString((byte[])(searchResponse.Entries[0].Attributes["CN"][0]));

                var groupsAttributes = searchResponse.Entries[0].Attributes["memberOf"];

                if (groupsAttributes != null && groupsAttributes.Count > 0)
                {
                    foreach (byte[] entry in groupsAttributes)
                    {
                        string entryString = encoding.GetString(entry);
                        int fromIndex = 3 + entryString.IndexOf("CN=", StringComparison.OrdinalIgnoreCase);
                        int toIndex = -3 + entryString.IndexOf(',', StringComparison.OrdinalIgnoreCase);
                        string group = entryString.Substring(fromIndex, toIndex);

                        user.Groups.Add(group);
                    }
                }

                return user;
            }
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, ex.Message);

            user.LdapErrorResponseMessage = ex.Message + $" (the values are interpreted using the {encoding.CodePage} code page)";

            return user;
        }
    }

    public static string GetSAMAccountName(this IIdentity identity)
    {
        if (string.IsNullOrEmpty(identity?.Name))
        {
            throw new ArgumentNullException(nameof(identity));
        }

        int separatorIndex;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            separatorIndex = identity.Name.IndexOf('\\');
            return identity.Name.Substring(separatorIndex);
        } 
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            separatorIndex = identity.Name.LastIndexOf('@');
            return identity.Name.Substring(0, separatorIndex);
        }
        else
        {
            throw new Exception("Unsupported operating system.");
        }
    }

    /// <summary>
    /// Создает строку CSV-файла с разделителем pipe '|' (вертикальная черта).
    /// </summary>
    /// <param name="cells">Ячейки строки. null заменяется на "?".</param>
    /// <returns>Строка CSV-файла с разделителем pipe '|' (вертикальной чертой).</returns>
    public static string CreateCsvRow(params string?[] cells)
    {
        return string.Join('|', cells.Select(c => string.IsNullOrEmpty(c) ? "?" : c).ToArray());
    }
}