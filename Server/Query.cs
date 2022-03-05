using DAL;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

public class Query
{
    [Authorize]
    [UseOffsetPaging(DefaultPageSize = 15, IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public IQueryable<AuditEvent> GetAuditEvents(AppDbContext context) => context.AuditEvents;
    
    [Authorize]
    [UseOffsetPaging(DefaultPageSize = 15, IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public IQueryable<AuditEvent> GetAuditUploadingsEvents(AppDbContext context) 
        => context.AuditEvents.Where(e => EF.Functions.Like(e.Object, "%/api/upload%"));
}
