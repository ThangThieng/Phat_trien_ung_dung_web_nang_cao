using CulinaryBlog.Domain.Exceptions;

namespace CulinaryBlog.API.Middleware.ExceptionMapping;

/// <summary>Ánh xạ HTTP cho các domain exception dùng chung, không thuộc module nào (Buổi 3 — Dev 4).</summary>
public sealed class CommonExceptionMappings : IExceptionStatusMapping
{
    public void Configure(ExceptionStatusMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        map.Map<BusinessRuleViolationException>(StatusCodes.Status400BadRequest)
            .Map<ConcurrencyConflictException>(StatusCodes.Status409Conflict);
    }
}
