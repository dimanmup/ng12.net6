using DAL;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Server.Models;
using Server.Authorization;
using Server.Injections;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

[ApiController]
public class InfoController : Controller
{
    private readonly AppDbContext context;
    private readonly Settings settings;
    private readonly Descriptor descriptor;
    private readonly Ldap? ldap;
    private readonly IDataProtector protector;

    public InfoController(AppDbContext context, Settings settings, Descriptor descriptor, IDataProtectionProvider provider, Ldap? ldap) 
    {
        this.context = context;
        this.settings = settings;
        this.descriptor = descriptor;
        this.ldap = ldap;
        protector = provider.CreateProtector(Helper.AuthorizationCookieProtectionPurpose);
    }

    /// <summary>
    /// Дает справку.<br>
    /// Если в авторизации используется кука, отправляет ее вместе с ответом на этот запрос.
    /// </summary>
    /// <param name="group">Проверяемая доменная группа.</param>
    /// <returns>
    /// Справка об<br/>
    /// 1) авторизации,<br/>
    /// 2) базе данных,<br/>
    /// 3) ниличии доменной группы,<br/>
    /// 4) доменной учетке (ФИО и группы).
    /// </returns>
    [Route("api/info")]
    public JsonResult GetInfo(string? group)
    {
        #region Required
        Dictionary<string, object?> identityInfo = new Dictionary<string, object?>();
        identityInfo.Add("is authenticated", User.Identity?.IsAuthenticated);
        identityInfo.Add("authentication type", User.Identity?.AuthenticationType);
        identityInfo.Add("identity name", User.Identity?.Name);

        Dictionary<string, object?> databaseInfo = new Dictionary<string, object?>();
        databaseInfo.Add("can connect", context.Database.CanConnect());
        databaseInfo.Add("provider", context.Database.ProviderName);
        databaseInfo.Add("data source", context.Database.GetDbConnection().DataSource);
        databaseInfo.Add("connection string", context.Database.GetDbConnection().ConnectionString);
        databaseInfo.Add("user", context.Database.GetConnectionString()?
            .Split(';')
            .FirstOrDefault(cs => cs.StartsWith("user id", StringComparison.OrdinalIgnoreCase))?
            .Split("=")[1]);
        
        Dictionary<string, object> info = new Dictionary<string, object>();
        info.Add("identity", identityInfo);
        info.Add("database", databaseInfo);

        if (!string.IsNullOrEmpty(group))
        {
            Dictionary<string, object?> groupsInfo = new Dictionary<string, object?>();
            groupsInfo.Add(group, User.IsInRole(group));
            info.Add("has group", groupsInfo);
        }

        JsonResult result = Json(info, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            WriteIndented = true
        });

        if (string.IsNullOrEmpty(User.Identity?.Name))
        {
            return result;
        }
        #endregion

        #region Domain
        User user = User.Identity.GetUserFromDC(ldap: ldap);
        
        Dictionary<string, object?> domainInfo = new Dictionary<string, object?>();
        domainInfo.Add("display name", user.DisplayName);
        domainInfo.Add("LDAP base", user.Base);
        domainInfo.Add("LDAP response error", user.LdapErrorResponseMessage);

        for (int i = 0; i < user.Groups.Count; i++)
        {
            domainInfo.Add($"groups[{i + 1}]", user.Groups[i]);
        }

        info.Add("domain info", domainInfo);

        if (!descriptor.UseCookie)
        {
            return result;
        }
        #endregion

        #region Authorization cookie
        string userJson = JsonConvert.SerializeObject(user);
        string userJsonSecret = protector.Protect(userJson);

        CookieOptions co = new CookieOptions();

        // Чтобы принимались на другом сервере,
        // т.е. были доступны для Client на localhost с другим портом.
        if (settings.Args.Any(a => a.Key == "cookie-for-dev"))
        {
            co.Secure = true;
            co.SameSite = SameSiteMode.None;
        }

        Response.Cookies.Append("auth", userJsonSecret, co);
        #endregion

        return result;
    }

    [Route("api/test-db")]
    public int TestDb() 
    {
        AuditEvent e = new AuditEvent();
        e.Description = "Test database.";
        e.HttpStatusCode = 200;
        e.Object = HttpContext.Request.Path;
        e.Source = HttpContext.User.Identity?.Name;
        e.UtcDateTime = DateTime.UtcNow;
        
        context.AuditEvents.Add(e);

        return context.SaveChanges();
    }

    /// <summary>
    /// Для проверки инъекции шифрователя.
    /// </summary>
    [Route("api/protect")]
    public string Protect(string value) => protector.Protect(value);
    
    /// <summary>
    /// Для проверки перехвата ошибки при делении на 0.
    /// </summary>
    [Route("api/division-by")]
    public int DivisionBy(int divider = 1) => 1 / divider;

    /// <summary>
    /// Для проверки перехвата ошибки со всеми потомками любого типа.
    /// </summary>
    [Route("api/error")]
    public IActionResult Error()
    {
        throw new AggregateException(
            new IndexOutOfRangeException("ex 1"),
            new ApplicationException("ex 2"),
            new ArgumentNullException("ex 3",
                new AggregateException(
                    new Exception("ex 4",
                        new ArgumentException("ex 5")
                        )
                    )
                )
            );
    }

    /// <summary>
    /// Перехватыват ошибки времени исполнения.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/error-handler")]
    public JsonResult HandleError()
    {
        IExceptionHandlerFeature exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;
        List<string> errors = new List<string>();
        List<Exception> innerExceptions = GetInnerExceptions(exceptionHandlerFeature.Error);

        for (int i = 0; i < innerExceptions.Count; i++)
        {
            errors.Add(innerExceptions[i].Message);

            // Запись в файлы логов, в таблицу событий аудита БД, отправка на почту...
        }

        HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        context.WriteAuditEvent(HttpContext, detail: string.Join("; ", errors), code: HttpStatusCode.InternalServerError);

        return Json(errors);
    }

    /// <summary>
    /// Находит все внутренние ошибки с учетом AggregateException.
    /// </summary>
    private List<Exception> GetInnerExceptions(Exception e)
    {
        List<Exception> eList = new List<Exception>();
        
        if (e is AggregateException)
        {
            eList.AddRange((e as AggregateException)!.InnerExceptions);
        }
        else
        {
            eList.Add(e);
        }

        List<Exception> ieList = eList
            .Where(i => i.InnerException != null)
            .SelectMany(i => GetInnerExceptions(i.InnerException!)) // Перед фильтр на nonnullable.
            .ToList();

        eList.AddRange(ieList);

        return eList;
    }
}