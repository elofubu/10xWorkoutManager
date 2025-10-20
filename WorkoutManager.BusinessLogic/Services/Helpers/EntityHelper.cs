using WorkoutManager.BusinessLogic.Exceptions;

namespace WorkoutManager.BusinessLogic.Services.Helpers;

/// <summary>
/// Helper class for common entity operations like null checking.
/// Centralizes error handling and reduces code duplication across services.
/// </summary>
public static class EntityHelper
{
    /// <summary>
    /// Fetches an entity and throws NotFoundException if it's null.
    /// This eliminates repeated null-check patterns across services.
    /// </summary>
    /// <typeparam name="T">The entity type to fetch</typeparam>
    /// <param name="fetchFunc">Async function that fetches the entity</param>
    /// <param name="entityName">Name of the entity type (e.g., "Session", "Exercise")</param>
    /// <param name="id">Optional ID of the entity for error message</param>
    /// <returns>The fetched entity (guaranteed non-null)</returns>
    /// <exception cref="NotFoundException">Thrown if the entity is not found</exception>
    public static async Task<T> ThrowIfNotFoundAsync<T>(
        Func<Task<T?>> fetchFunc,
        string entityName,
        object? id = null) where T : class
    {
        if (fetchFunc == null)
            throw new ArgumentNullException(nameof(fetchFunc));

        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentException("Entity name cannot be null or empty", nameof(entityName));

        var entity = await fetchFunc();

        if (entity == null)
        {
            var idPart = id != null ? $" with ID {id}" : string.Empty;
            var message = $"{entityName}{idPart} not found";
            throw new NotFoundException(message);
        }

        return entity;
    }

    /// <summary>
    /// Checks if an entity exists (returns boolean instead of throwing).
    /// Useful when you want to conditionally handle missing entities.
    /// </summary>
    /// <typeparam name="T">The entity type to check</typeparam>
    /// <param name="fetchFunc">Async function that fetches the entity</param>
    /// <returns>True if entity exists, false otherwise</returns>
    public static async Task<bool> ExistsAsync<T>(Func<Task<T?>> fetchFunc) where T : class
    {
        if (fetchFunc == null)
            throw new ArgumentNullException(nameof(fetchFunc));

        var entity = await fetchFunc();
        return entity != null;
    }
}
