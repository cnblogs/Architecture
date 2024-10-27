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
    public static IMvcBuilder AddCqrsModelBinderProvider(this IMvcBuilder builder)
    {
        return builder.AddMvcOptions(options => options.AddCqrsModelBinderProvider());
    }

    /// <summary>
    ///     Add long to string json converter.
    /// </summary>
    /// <param name="builder"><see cref="IMvcBuilder"/>.</param>
    public static IMvcBuilder AddLongToStringJsonConverter(this IMvcBuilder builder)
    {
        return builder.AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new LongToStringConverter()));
    }
}
