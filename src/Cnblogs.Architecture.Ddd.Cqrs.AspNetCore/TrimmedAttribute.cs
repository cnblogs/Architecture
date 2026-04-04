using System.Text.Json.Serialization;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
/// Auto trim string when deserialized from JSON
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class TrimmedAttribute() : JsonConverterAttribute(typeof(TrimmedStringConverter));
