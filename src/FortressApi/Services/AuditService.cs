using FortressApi.Data;
using FortressApi.Models;

namespace FortressApi.Services;

public sealed class AuditService(AppDbContext db)
{
    public async Task WriteAsync(HttpContext ctx, Guid? userId, string action, string resource)
    {
        var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua = ctx.Request.Headers.UserAgent.ToString();

        db.AuditEvents.Add(new AuditEvent
        {
            UserId = userId,
            Action = action,
            Resource = resource,
            Ip = ip,
            UserAgent = ua
        });

        await db.SaveChangesAsync();
    }
}
