using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.ApplicationBootstrap.WebApi.ModelBinding;

/// <summary>
/// Model binder that uses <see cref="Microsoft.AspNetCore.Mvc.JsonOptions"/> to deserialize incoming data.
/// </summary>
public class MvcJsonOptionsModelBinder : IModelBinder
{
    /// <inheritdoc />
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var options = bindingContext.HttpContext.RequestServices.GetService<JsonOptions>();
        CancellationToken ct = bindingContext.HttpContext.RequestAborted;
        var stream = bindingContext.HttpContext.Request.Body;

        var customer = await JsonSerializer.DeserializeAsync(stream, bindingContext.ModelType, options.JsonSerializerOptions, ct);
        bindingContext.Result = ModelBindingResult.Success(customer);
    }
}