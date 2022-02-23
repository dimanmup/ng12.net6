using DAL;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Server.Authorization;
using Server.Injections;

namespace Authorization;

public class Filter : IAsyncAuthorizationFilter
{
    private readonly Descriptor descriptor;
    private readonly Settings settings;
    private readonly AppDbContext dbContext;
    private readonly IDataProtector protector;

    public Filter(Descriptor descriptor, Settings settings, AppDbContext dbContext, IDataProtectionProvider provider)
    {
        this.descriptor = descriptor;
        this.settings = settings;
        this.dbContext = dbContext;
        this.protector = provider.CreateProtector(Helper.AuthorizationCookieProtectionPurpose);
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var authorizationResult = descriptor.IsAvailable(context.HttpContext.Request.Path, context.HttpContext, settings, protector);
        await dbContext.WriteAuditEventAsync(context.HttpContext, code: authorizationResult.Key, detail: authorizationResult.Value);

        if (authorizationResult.Key != System.Net.HttpStatusCode.OK) 
        {
            context.Result = new ObjectResult(authorizationResult.Value) { StatusCode = (int)authorizationResult.Key };
        }
    }
}