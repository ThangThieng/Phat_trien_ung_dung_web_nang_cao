namespace CulinaryBlog.Domain.Enums;

/// <summary>SRS §7.2 – smallint: 1=Easy, 2=Medium, 3=Hard, 4=Expert.</summary>
public enum RecipeDifficulty : short
{
    Easy = 1,
    Medium = 2,
    Hard = 3,
    Expert = 4,
}
