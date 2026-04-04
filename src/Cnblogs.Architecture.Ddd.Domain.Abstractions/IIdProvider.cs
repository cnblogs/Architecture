namespace Cnblogs.Architecture.Ddd.Domain.Abstractions;

/// <summary>
/// ID generator
/// </summary>
public interface IIdProvider
{
    /// <summary>
    /// Get next id in human-readable style (yyyyMMddHHmmssXXXYYY). Make sure your concurrent for the same eigen is less than 100/s. Otherwise, consider using <see cref="NextNumeric"/> instead
    /// </summary>
    /// <returns></returns>
    long NextReadable();

    /// <summary>
    /// Get next id in human-readable style (yyyyMMddHHmmssXXXYYY). Make sure your concurrent for the same eigen is less than 100/s. Otherwise, consider using <see cref="NextNumeric"/> instead
    /// </summary>
    /// <param name="eigen">Slice ID. Can be machineId, userId, podId, threadId, etc. Only using the last 3 digits.</param>
    /// <returns></returns>
    long NextReadable(int eigen);

    /// <summary>
    /// Get next numeric id using snowflake-like algorithm.
    /// </summary>
    /// <returns></returns>
    long NextNumeric();
}
