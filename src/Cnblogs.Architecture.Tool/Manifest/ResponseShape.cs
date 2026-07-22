namespace Cnblogs.Architecture.Tool.Manifest;

/// <summary>The shape of the payload returned by a CQRS endpoint.</summary>
internal enum ResponseShape
{
    Item,
    List,
    PagedList,
    None
}
