namespace CulinaryBlog.Domain.Exceptions;

/// <summary>
/// Application Error Codes — SRS v1.2.2 Phụ lục B, đủ 29 mã (SCREAMING_SNAKE_CASE, dùng trong trường "type" của RFC 7807).
/// Nằm ở Domain (chuỗi thuần, chỉ BCL) để domain exception và mọi tầng ngoài dùng CHUNG một nguồn — không có hai
/// danh sách mã song song. Đưa đủ 29 mã vào một lần (commit nền Buổi 3) để dev module không phải sửa file dùng chung này.
/// Test kiến trúc ErrorCodesTests giữ danh sách này khớp đúng Phụ lục B.
/// </summary>
public static class ErrorCodes
{
    // ---- Auth ----
    public const string AuthEmailExists = "AUTH_EMAIL_EXISTS";
    public const string AuthInvalidCredentials = "AUTH_INVALID_CREDENTIALS";
    public const string AuthTokenExpired = "AUTH_TOKEN_EXPIRED";
    public const string AuthTokenInvalid = "AUTH_TOKEN_INVALID";
    public const string AuthRefreshTokenExpired = "AUTH_REFRESH_TOKEN_EXPIRED";
    public const string AuthRefreshTokenRevoked = "AUTH_REFRESH_TOKEN_REVOKED";
    public const string AuthGoogleTokenInvalid = "AUTH_GOOGLE_TOKEN_INVALID";
    public const string AuthGoogleUnavailable = "AUTH_GOOGLE_UNAVAILABLE";
    public const string AuthAccountDisabled = "AUTH_ACCOUNT_DISABLED";
    public const string AuthAccountLocked = "AUTH_ACCOUNT_LOCKED";
    public const string AuthUserNotFound = "AUTH_USER_NOT_FOUND";

    /// <summary>
    /// ⚠️ Tạm thời — KHÔNG có trong Phụ lục B (MT-46). Còn được RegisterUserCommand dùng cho tới khi Dev 1 làm xong
    /// retrofit D-1 (BE tự sinh UserName) trong Buổi 3; D-1 xong thì xóa hằng số này và dòng miễn trừ trong ErrorCodesTests.
    /// </summary>
    public const string AuthUserNameExists = "AUTH_USERNAME_EXISTS";

    // ---- Recipe ----
    public const string RecipeNotFound = "RECIPE_NOT_FOUND";
    public const string RecipeSlugExists = "RECIPE_SLUG_EXISTS";
    public const string RecipePublishIncomplete = "RECIPE_PUBLISH_INCOMPLETE";
    public const string RecipeForbidden = "RECIPE_FORBIDDEN";
    public const string RecipeConcurrencyConflict = "RECIPE_CONCURRENCY_CONFLICT";
    public const string RecipeInvalidStateTransition = "RECIPE_INVALID_STATE_TRANSITION";
    public const string IngredientQuantityRequired = "INGREDIENT_QUANTITY_REQUIRED";

    // ---- Category ----
    public const string CategoryNotFound = "CATEGORY_NOT_FOUND";
    public const string CategoryNameExists = "CATEGORY_NAME_EXISTS";
    public const string CategoryDeleteHasRecipes = "CATEGORY_DELETE_HAS_RECIPES";

    // ---- File ----
    public const string FileSizeExceeded = "FILE_SIZE_EXCEEDED";
    public const string FileMimeInvalid = "FILE_MIME_INVALID";
    public const string FileForbidden = "FILE_FORBIDDEN";
    public const string FileStorageUnavailable = "FILE_STORAGE_UNAVAILABLE";

    // ---- Common ----
    public const string ValidationError = "VALIDATION_ERROR";
    public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
    public const string InternalError = "INTERNAL_ERROR";
    public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";
}
