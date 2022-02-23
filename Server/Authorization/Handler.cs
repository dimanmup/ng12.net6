using DAL;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.DataProtection;
using Server.Injections;
using System.Net;

namespace Server.Authorization;

public class Handler : IAuthorizationHandler
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly Descriptor descriptor;
    private readonly Settings settings;
    private readonly DbContextOrigin dbContextOrigin;
    private readonly IDataProtector protector;

    public Handler(IHttpContextAccessor httpContextAccessor, Descriptor descriptor, Settings settings, DbContextOrigin dbContextOrigin, IDataProtectionProvider provider)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.descriptor = descriptor;
        this.settings = settings;
        this.dbContextOrigin = dbContextOrigin;
        this.protector = provider.CreateProtector(Helper.AuthorizationCookieProtectionPurpose);
    }

    public ValueTask<AuthorizeResult> AuthorizeAsync(IMiddlewareContext graphqlContext, AuthorizeDirective directive)
    {
        AuthorizeResult result = IsAllowed(graphqlContext) ? AuthorizeResult.Allowed : AuthorizeResult.NotAllowed;

        return new ValueTask<AuthorizeResult>(result);
    }

    private bool IsAllowed(IMiddlewareContext graphqlContext)
    {
        var authorizationResult = descriptor.IsAvailable(graphqlContext.Path.Print(), httpContextAccessor.HttpContext!, settings, protector, true);

        using AppDbContext dbContext = dbContextOrigin.GetDbContext();
        dbContext.WriteAuditEvent(httpContextAccessor.HttpContext!, 
            graphqlContext: graphqlContext, 
            code: authorizationResult.Key, 
            detail: authorizationResult.Value);

        return authorizationResult.Key == HttpStatusCode.OK;
    }
}
