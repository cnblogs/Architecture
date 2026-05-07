using System.Reflection;

namespace Cnblogs.Architecture.Ddd.Cqrs.Abstractions.Internals;

internal record EnricherDescriptor(Type ImplType, MethodInfo EnrichMethod, MethodInfo BulkEnrichMethod);