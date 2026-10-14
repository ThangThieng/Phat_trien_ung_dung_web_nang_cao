namespace CulinaryBlog.API.Extensions;

public sealed class SecurityRateLimitingOptions
{
    public bool Enabled { get; set; } = true;

    public RateQuota Auth { get; set; } = new() { PermitLimit = 10 };

    public RateQuota General { get; set; } = new() { PermitLimit = 100 };

    public RateQuota Upload { get; set; } = new() { PermitLimit = 5 };
}

public sealed class RateQuota
{
    public int PermitLimit { get; set; }

    public int WindowSeconds { get; set; } = 60;
}
