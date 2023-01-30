using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
/// Model Binder for <see cref="PagingParams"/>
/// </summary>
public class PagingParamsModelBinder : IModelBinder
{
    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var modelName = bindingContext.ModelName;
        var pageIndex = bindingContext.ValueProvider.GetValue(nameof(PagingParams.PageIndex));
        var pageSize = bindingContext.ValueProvider.GetValue(nameof(PagingParams.PageSize));
        if (pageIndex == ValueProviderResult.None || pageSize == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        var pageIndexString = pageIndex.FirstValue;
        var pageSizeString = pageSize.FirstValue;
        if (pageIndexString == null || pageSizeString == null)
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        var pageIndexSuccess = int.TryParse(pageIndexString, out var pageIndexNumber);
        if (pageIndexSuccess == false || pageIndexNumber <= 0)
        {
            bindingContext.ModelState.TryAddModelError(modelName, "PageIndex must be a positive number");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        var pageSizeSuccess = int.TryParse(pageSizeString, out var pageSizeNumber);
        if (pageSizeSuccess == false || pageSizeNumber < 0)
        {
            bindingContext.ModelState.TryAddModelError(modelName, "PageIndex must be a positive number or 0");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        var pagingParams = new PagingParams(pageIndexNumber, pageSizeNumber);
        bindingContext.Result = ModelBindingResult.Success(pagingParams);
        return Task.CompletedTask;
    }
}