using DAL;
using HotChocolate.AspNetCore.Authorization;

public class Query
{
    [Authorize]
    [UseOffsetPaging(DefaultPageSize = 15, IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public IQueryable<AuditEvent> GetAuditEvents(AppDbContext context) => context.AuditEvents;
}
