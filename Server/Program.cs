using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;
using Serilog.Events;
using Server.Authorization;
using Server.Injections;
using System.Net;
using System.Runtime.InteropServices;

#region Logging
// Запись warnings и errors в файлы.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.File(System.IO.Path.Combine("Logs", "log-.txt"),
        outputTemplate: "[{Level:u3}] {Timestamp:yyyy.MM.dd HH:mm:ss.fff}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 31,
        rollOnFileSizeLimit: true,
        fileSizeLimitBytes: 1024 * 16 // Для чтения файлов как страниц.
    )
    .CreateLogger();

using ILoggerFactory loggerFactory =
    LoggerFactory.Create(builder =>
        builder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        }));

ILogger<Program> logger = loggerFactory.CreateLogger<Program>();
#endregion

#region builder
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // Чтобы запускалось и как приложение, и как служба.
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
});

#region Settings
Settings settings = new Settings(Environment.GetCommandLineArgs());

builder.Services.AddSingleton(settings);

foreach (var a in settings.Args)
{
    logger.LogInformation($"{{{a.Key}: {a.Value}}}");
}
#endregion

builder.Services.AddHttpContextAccessor(); // Инъекция HTTP-контекста в GraphQL-запрос.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDataProtection();

#region Database

// Commands:
// dotnet ef migrations add AuditEvents -o Migrations/Oracle  -- --provider Oracle
// dotnet ef migrations add AuditEvents -o Migrations/Sqlite  -- --provider SQLite
// dotnet ef migrations add AuditEvents -o Migrations/Sqlite

string dbProviderName = settings.Args.FirstOrDefault(kv => kv.Key == "db").Value ?? builder.Configuration.GetValue("provider", "SQLite");
string connectionString = builder.Configuration.GetConnectionString(dbProviderName);

DbContextOrigin dbContextOrigin = new DbContextOrigin(dbProviderName, connectionString);
builder.Services.AddSingleton(dbContextOrigin);

Action<dynamic> dbProviderOptionsSetter = dbProviderOptions =>
{
    dbProviderOptions.MigrationsAssembly("Server"); // Для миграций в указанном проекте.
    dbProviderOptions.MigrationsHistoryTable("ST__MIGRATIONS");
};

switch (dbProviderName)
{
    case "Oracle":
        builder.Services.AddDbContext<AppDbContext>(options => options.UseOracle(connectionString, dbProviderOptionsSetter));
        break;

    case "sqlite":
    default:
        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString, dbProviderOptionsSetter));
        break;
    
    // TODO:
    //options.LogTo(Log.Logger.Information).EnableSensitiveDataLogging();
    //options.LogTo(Console.WriteLine).EnableSensitiveDataLogging();
}

logger.LogInformation($"DB provider: {dbProviderName}");
#endregion

#region LDAP for hosting on Linux
Ldap? ldap = builder.Configuration.GetSection("Ldap").Get<Ldap>();

if (ldap != null)
{
    logger.LogInformation("LDAP user credentials are set.");
    builder.Services.AddSingleton(ldap);
}
#endregion

#region Authorization
Descriptor authorizationDescriptor = builder.Configuration.GetSection("Authorization").Get<Descriptor>();

builder.Services.AddSingleton(authorizationDescriptor);
builder.Services.AddControllers(options => options.Filters.Add(typeof(Authorization.Filter)));
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate(options => 
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        if (ldap == null)
        {
            throw new Exception("LDAP section not found.");
        }

        options.EnableLdap(settings =>
        {
            settings.Domain = ldap.Domain;
            settings.LdapConnection = ldap.GetLdapConnection();
            
            settings.EnableLdapClaimResolution = true;
        });
    }
});
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
#endregion

#region Cors
const string corsPolicyName = "cors policy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicyName,
        builder =>
        {
            builder
                //.AllowAnyOrigin()
                .WithOrigins("http://localhost:4200") // URI клиентской части, запущенной отдельно (на localhost с другим портом).
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .AllowAnyHeader();
        });
});
#endregion

#region Files
Loading loading = builder.Configuration.GetSection("Loading").Get<Server.Injections.Loading>();

builder.Services.AddSingleton(loading);
builder.Services.Configure<KestrelServerOptions>(options =>
{
    // При загрузке данных больше options.Limits.MaxRequestBodySize
    // периодически создается ошибка 0: Response без заголовков.
    // ? Ошибка превышения options.Limits.MaxRequestBodySize опережает остальные.
    //options.Limits.MaxRequestBodySize = loading.MaxRequestBodySize;
    
    options.Limits.MaxRequestBodySize = null;
    options.Limits.MinRequestBodyDataRate = new MinDataRate(50, TimeSpan.FromSeconds(30));

    //options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(130);
    //options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
    //options.AllowSynchronousIO = true;
});
// При загрузке на сервер форма игнорируется, читается только stream из тела multipart/form-data.
// builder.Services.Configure<FormOptions>(options =>
// {
//      options.ValueLengthLimit = int.MaxValue;
//      options.MultipartBodyLengthLimit = long.MaxValue;
//      options.MultipartHeadersLengthLimit = int.MaxValue;
// });
#endregion

// GraphQL
builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddAuthorization()
    .AddAuthorizationHandler<Handler>()
    .AddFiltering()
    .AddSorting()
    .RegisterDbContext<AppDbContext>(DbContextKind.Synchronized); // Для инъекции контекста БД в методы Query.

builder.Host
    .UseSerilog() // Системное логирование в файлы.
    .UseWindowsService(); // Исполнимость в качестве службы.
#endregion

#region app
var app = builder.Build();
app.UseRouting();
app.UseHttpsRedirection();
app.UseExceptionHandler("/api/error-handler");
app.UseCors(corsPolicyName); // Для доступности другому приложению, т.е. запущенному на localhost с другим портом.

// Negotiate
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = responseContext =>
    {
        // Чтобы было невозможно получить файл из cach браузера при отказе в доступе.
        // Для SPA кэширование и не нужно.
        responseContext.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");

        if (responseContext.Context.User.Identity == null || !responseContext.Context.User.Identity.IsAuthenticated)
        {
            responseContext.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            responseContext.Context.Response.ContentLength = 0;
            responseContext.Context.Response.Body = Stream.Null;
        }
    }
});

app.MapGraphQL();
app.MapControllers();
app.MapFallbackToFile("index.html"); // Для GET-запросов через строку браузера.

// Uploading very large files.
app.MapPost("api/upload", Server.Controllers.FileController.Upload).RequireAuthorization();

logger.LogInformation("Application launched!");
logger.LogInformation("Banana Cake Pop: https://localhost:5011/graphql");
logger.LogInformation("Download GraphQL scheme: https://localhost:5011/graphql?sdl");
logger.LogInformation("Check getting info: https://localhost:5011/api/info");
logger.LogInformation("Check error 500: https://localhost:5011/api/division-by?divider=0");
logger.LogInformation("Check protector: https://localhost:5011/api/protect?value=hello");
logger.LogInformation("Check downloading: https://localhost:5011/api/download/audit?utcStart=2022.02.18&utcEnd=2022.02.21");
logger.LogInformation("Angular SPA: http://localhost:4200");

app.Run();
#endregion

// dotnet run su=desktop-cpkmc9c\diman cookie-for-dev
// dotnet run cookie-for-dev