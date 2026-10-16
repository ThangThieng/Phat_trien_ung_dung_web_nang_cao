using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Exceptions;

namespace CulinaryBlog.Domain.Entities;

/// <summary>
/// SRS §7.3 – bước thực hiện, sắp theo StepNumber (unique trong cùng Recipe, constraint DEFERRABLE — D-16).
/// StepNumber do SERVER toàn quyền gán (MT-03): client không bao giờ gửi, muốn đổi thứ tự thì dùng reorder.
/// </summary>
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
        EnsurePositive(stepNumber);

        var step = new RecipeStep { RecipeId = recipeId, StepNumber = stepNumber };
        step.Update(title, description, timerMinutes, imageUrl);
        return step;
    }

    /// <summary>FR-RCP-010: sửa NỘI DUNG bước (không có StepNumber — thứ tự chỉ đổi qua Recipe.ReorderSteps).</summary>
    internal void Update(string title, string description, int? timerMinutes, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
        {
            throw new BusinessRuleViolationException(ErrorCodes.ValidationError, "Tiêu đề và mô tả của bước không được để trống.");
        }

        if (timerMinutes is < 0)
        {
            throw new BusinessRuleViolationException(ErrorCodes.ValidationError, "Thời gian của bước phải >= 0.");
        }

        Title = title.Trim();
        Description = description.Trim();
        TimerMinutes = timerMinutes;
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
    }

    internal void Renumber(int stepNumber)
    {
        EnsurePositive(stepNumber);
        StepNumber = stepNumber;
    }

    private static void EnsurePositive(int stepNumber)
    {
        if (stepNumber <= 0)
        {
            throw new BusinessRuleViolationException(ErrorCodes.ValidationError, "StepNumber phải lớn hơn 0.");
        }
    }
}
