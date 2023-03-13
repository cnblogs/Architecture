using Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Extensions to inject custom model binder for CQRS.
/// </summary>
public static class ControllerOptionInjector
{
    /// <summary>
    ///     Add custom model binder used for CQRS, like model binder for <see cref="PagingParams"/>.
    /// </summary>
    /// <param name="options"><see cref="MvcOptions"/></param>
    public static void AddCqrsModelBinderProvider(this MvcOptions options)
    {
        options.ModelBinderProviders.Insert(0, new CqrsModelBinderProvider());
    }

    /// <summary>
    ///     Add custom model binder used for CQRS, like model binder for <see cref="PagingParams"/>.
    /// </summary>
    /// <param name="builder"><see cref="IMvcBuilder"/></param>
    public static void AddCqrsModelBinderProvider(this IMvcBuilder builder)
    {
        builder.AddMvcOptions(options => options.AddCqrsModelBinderProvider());
    }
}
