using DAL;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Server.Authorization;
using Server.Injections;
using Server.Models;
using System.Globalization;
using System.Net;
using System.Web;

namespace Server.Controllers;

[ApiController]
public class FileController : Controller
{
    private readonly AppDbContext context;

    public FileController(AppDbContext context)
    {
        this.context = context;
    }
    
    private static async Task WriteBadRequestAuditEventAsync(string message, DbContextOrigin dbContextOrigin, HttpRequest request)
    {
        using (AppDbContext dbContext = dbContextOrigin.GetDbContext())
        {
            await dbContext.WriteAuditEventAsync(request.HttpContext,
                code: HttpStatusCode.BadRequest,
                detail: message,
                parseRequestForm: false);
        }
    }

    public static async Task<IResult> Upload(
        HttpRequest request, 
        Loading loading, 
        Descriptor descriptor, 
        Settings settings, 
        DbContextOrigin dbContextOrigin,
        IDataProtectionProvider provider
        )
    {
        IDataProtector protector = provider.CreateProtector(Helper.AuthorizationCookieProtectionPurpose);

        #region Authorization
        var authorizationResult = await descriptor.IsAvailableAsync(request.Path, request.HttpContext, settings, protector);

        if (authorizationResult.Key != HttpStatusCode.OK)
        {
            using (AppDbContext dbContext = dbContextOrigin.GetDbContext())
            {
                await dbContext.WriteAuditEventAsync(request.HttpContext,
                    code: authorizationResult.Key,
                    detail: authorizationResult.Value,
                    parseRequestForm: false);
            }

            return Results.Unauthorized();
        }
        #endregion
        
        string? message = null;

        #region Validation
        if (request.Headers.TryGetValue("Content-Length", out StringValues sv)
            && sv.Count == 1
            && long.TryParse(sv[0], out long length))
        {
            if (length >= loading.MaxRequestBodySize)
            {
                message = $"Size {length} bytes exceeds {loading.MaxRequestBodySize} bytes.";
            }
        }
        else
        {
            message = "The request does not contain the 'Content-Length' header.";
        }

        if (message != null)
        {
            await WriteBadRequestAuditEventAsync(message, dbContextOrigin, request);
            return Results.BadRequest(message);
        }
        #endregion

        #region Parsing the request form
        if (!request.HasFormContentType
            || !MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader)
            || string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
        {
            message = "Not a form data for uploading.";

            await WriteBadRequestAuditEventAsync(message, dbContextOrigin, request);

            return Results.BadRequest(message);
        }
        #endregion

        MultipartReader reader = new MultipartReader(mediaTypeHeader.Boundary.Value, request.Body);
        MultipartSection? section = await reader.ReadNextSectionAsync();

        while (section != null)
        {
            bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(
                section.ContentDisposition,
                out ContentDispositionHeaderValue? contentDisposition
                );

            if (hasContentDispositionHeader
                && contentDisposition != null
                && contentDisposition.DispositionType.Equals("form-data")
                && !string.IsNullOrEmpty(contentDisposition.FileName.Value))
            {
                if (!Directory.Exists(loading.UploadingFolder))
                {
                    Directory.CreateDirectory(loading.UploadingFolder);
                }

                #region Name validation
                if (!loading.NameAllowed(contentDisposition.FileName.Value))
                {
                    message = "This name is not allowed.";
                    await WriteBadRequestAuditEventAsync(message, dbContextOrigin, request);
                    return Results.BadRequest(message);
                }
                #endregion

                string path = System.IO.Path.Combine(loading.UploadingFolder, contentDisposition.FileName.Value);

                #region Path
                if (System.IO.File.Exists(path))
                {
                    string name = contentDisposition.FileName.Value;
                    string extension = "";
                    int dotLastIndex = name.LastIndexOf('.');

                    if (dotLastIndex > -1)
                    {
                        name = contentDisposition.FileName.Value.Substring(0, dotLastIndex);
                        extension = contentDisposition.FileName.Value.Substring(dotLastIndex);
                    }
                    
                    int counter = 1;
                    do
                    {
                        path = System.IO.Path.Combine(loading.UploadingFolder, $"{name}({counter++}){extension}");
                    }
                    while (System.IO.File.Exists(path));
                }
                #endregion

                using (var targetStream = System.IO.File.Create(path))
                {
                    await section.Body.CopyToAsync(targetStream);
                }

                FileDescription fd = new FileDescription(path);
                
                using (AppDbContext dbContext = dbContextOrigin.GetDbContext())
                {
                    await dbContext.WriteAuditEventAsync(request.HttpContext,
                        detail: fd.GetJson(),
                        parseRequestForm: false);
                }

                return Results.Ok(message);
            }
            
            section = await reader.ReadNextSectionAsync();
        }

        message = "No files data in the request.";

        await WriteBadRequestAuditEventAsync(message, dbContextOrigin, request);

        return Results.BadRequest(message);
    }

    [Route("api/download/audit")]
    public async Task DownloadAuditLog(string utcStart, string utcEnd, string? dateFormat = "yyyy.MM.dd")
    {
        if (string.IsNullOrEmpty(utcStart) 
            || string.IsNullOrEmpty(utcEnd))
        {
            return;
        }

        DateTime utcStartDate;
        DateTime utcEndDate;
        
        if (!DateTime.TryParseExact(utcStart, dateFormat, null, DateTimeStyles.None, out utcStartDate) 
            || !DateTime.TryParseExact(utcEnd, dateFormat, null, DateTimeStyles.None, out utcEndDate))
        {
            return;
        }
        
        Response.ContentType = "text/csv";
        Response.Headers.Add("Content-Disposition", string.Format("attachment; filename=\"{0}\"; filename*=UTF-8''{0}", HttpUtility.UrlPathEncode("audit.csv")));
        Response.Headers.Add("X-Content-Type-Options", "nosniff");

        StreamWriter sw;
        await using ((sw = new StreamWriter(Response.Body)))
        {
            await sw.WriteLineAsync(Helper.CreateCsvRow(
                "Id", 
                "UTC datetime", 
                "Object", 
                "Source", 
                "IP Address", 
                "Client agent",
                "HTTP status code",
                "Detail"));

            await foreach (AuditEvent e in context.AuditEvents
                .AsNoTracking()
                .AsQueryable()
                .Where(e => e.UtcDateTime >= utcStartDate && e.UtcDateTime < utcEndDate)
                .OrderByDescending(e => e.Id)
                .AsAsyncEnumerable())
            {
                await sw.WriteLineAsync(Helper.CreateCsvRow(
                    e.Id.ToString(), 
                    e.UtcDateTime.ToString("yyyy.MM.dd HH:ss:mm"),
                    e.Object,
                    e.Source,
                    e.SourceIPAddress,
                    e.SourceDevice,
                    e.HttpStatusCode.ToString(),
                    e.Description?.Replace("\r\n",""))
                    )
                    .ConfigureAwait(false);
            }

            await sw.FlushAsync().ConfigureAwait(false);
        }
    }
}
