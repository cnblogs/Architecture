using Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 用于注入 Model Binder 的扩展方法。
/// </summary>
public static class ControllerOptionInjector
{
    /// <summary>
    /// 添加 CQRS 相关的 Model Binder Provider
    /// </summary>
    /// <param name="options"><see cref="MvcOptions"/></param>
    public static void AddCqrsModelBinderProvider(this MvcOptions options)
    {
        options.ModelBinderProviders.Insert(0, new CqrsModelBinderProvider());
    }

    /// <summary>
    /// 添加 CQRS 相关的 Model Binder Provider
    /// </summary>
    /// <param name="builder"><see cref="IMvcBuilder"/></param>
    public static void AddCqrsModelBinderProvider(this IMvcBuilder builder)
    {
        builder.AddMvcOptions(options => options.AddCqrsModelBinderProvider());
    }
}