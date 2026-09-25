namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>Application Error Codes – SRS Phụ lục B (SCREAMING_SNAKE_CASE, dùng trong trường "type" của RFC 7807).</summary>
public static class ErrorCodes
{
    public const string AuthEmailExists = "AUTH_EMAIL_EXISTS";
    public const string AuthUserNameExists = "AUTH_USERNAME_EXISTS";
    public const string AuthInvalidCredentials = "AUTH_INVALID_CREDENTIALS";
    public const string AuthAccountLocked = "AUTH_ACCOUNT_LOCKED";
    public const string AuthAccountDisabled = "AUTH_ACCOUNT_DISABLED";
    public const string AuthTokenExpired = "AUTH_TOKEN_EXPIRED";
    public const string AuthTokenInvalid = "AUTH_TOKEN_INVALID";

    public const string RecipeNotFound = "RECIPE_NOT_FOUND";
    public const string RecipeForbidden = "RECIPE_FORBIDDEN";

    public const string CategoryNotFound = "CATEGORY_NOT_FOUND";
    public const string CategoryNameExists = "CATEGORY_NAME_EXISTS";
    public const string CategoryDeleteHasRecipes = "CATEGORY_DELETE_HAS_RECIPES";

    public const string FileSizeExceeded = "FILE_SIZE_EXCEEDED";
    public const string FileMimeInvalid = "FILE_MIME_INVALID";
    public const string FileForbidden = "FILE_FORBIDDEN";
    public const string FileStorageUnavailable = "FILE_STORAGE_UNAVAILABLE";

    public const string ValidationError = "VALIDATION_ERROR";
    public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
    public const string InternalError = "INTERNAL_ERROR";
}
