namespace CulinaryBlog.Domain.Common;

/// <summary>Vi phạm business rule trong Domain (ví dụ: publish recipe thiếu bước thực hiện).</summary>
public class DomainException : Exception
{
    public DomainException()
    {
    }

    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
