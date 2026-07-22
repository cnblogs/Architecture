namespace Cnblogs.Architecture.Tool.Manifest;

/// <summary>Where a parameter of a CQRS endpoint is bound from on the wire.</summary>
internal enum ParameterSource
{
    Route = 0,
    Query = 1,
    Body = 2
}
