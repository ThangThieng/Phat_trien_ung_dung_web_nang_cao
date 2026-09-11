using CulinaryBlog.Domain.Common;

namespace CulinaryBlog.Domain.Entities;

/// <summary>SRS §7.3 – bước thực hiện, sắp theo StepNumber (unique trong cùng Recipe).</summary>
public class RecipeStep : BaseEntity
{
    private RecipeStep()
    {
    }

    public Guid RecipeId { get; private set; }

    public int StepNumber { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public int? TimerMinutes { get; private set; }

    public string? ImageUrl { get; private set; }

    public static RecipeStep Create(Guid recipeId, int stepNumber, string title, string description, int? timerMinutes = null, string? imageUrl = null)
    {
        if (stepNumber <= 0)
        {
            throw new DomainException("StepNumber phải lớn hơn 0.");
        }

        return new RecipeStep
        {
            RecipeId = recipeId,
            StepNumber = stepNumber,
            Title = title,
            Description = description,
            TimerMinutes = timerMinutes,
            ImageUrl = imageUrl,
        };
    }
}
