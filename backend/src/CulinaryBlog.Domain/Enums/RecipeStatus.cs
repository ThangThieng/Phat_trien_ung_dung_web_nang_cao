namespace CulinaryBlog.Domain.Enums;

/// <summary>SRS §7.2 – smallint: 0=Draft, 1=Published, 2=Archived.</summary>
public enum RecipeStatus : short
{
    Draft = 0,
    Published = 1,
    Archived = 2,
}
