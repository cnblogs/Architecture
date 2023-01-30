using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
/// Model Binder Provider for custom types
/// </summary>
public class CqrsModelBinderProvider : IModelBinderProvider
{
    /// <inheritdoc />
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(PagingParams))
        {
            return new BinderTypeModelBinder(typeof(PagingParamsModelBinder));
        }

        return null;
    }
}