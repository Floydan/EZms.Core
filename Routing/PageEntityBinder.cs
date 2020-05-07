using System;
using System.Threading.Tasks;
using AutoMapper;
using EZms.Core.Loaders;
using EZms.Core.Models;
using EZms.Core.Repositories;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Rentals.Infrastructure.Routing
{
    public class PageEntityBinder : IModelBinder
    {
        private readonly IContentRepository _contentRepository;
        private readonly IContentLoader _contentLoader;
        private readonly IMapper _mapper;

        public PageEntityBinder(IContentRepository contentRepository, IContentLoader contentLoader, IMapper mapper)
        {
            _contentRepository = contentRepository;
            _contentLoader = contentLoader;
            _mapper = mapper;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = "contentid"; //string.IsNullOrEmpty(bindingContext.ModelName) ? "contentid" : bindingContext.ModelName;

            // Try to fetch the value of the argument by name
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            // Check if the argument value is null or empty
            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }

            if (!int.TryParse(value, out var id))
            {
                // Non-integer arguments result in model state errors
                bindingContext.ModelState.TryAddModelError(
                    modelName,
                    "Content Id must be an integer.");
                return Task.CompletedTask;
            }
            
            var version = 0;
            if (bindingContext.HttpContext.Request.Query.ContainsKey("cms-preview"))
            {
                int.TryParse(bindingContext.HttpContext.Request.Query["cms-preview"], out version);
            }

            var content = GetContent(id, version == 0 ? null : (int?)version, bindingContext.ModelType).GetAwaiter().GetResult();

            bindingContext.Result = content == null ? ModelBindingResult.Failed() : ModelBindingResult.Success(content);

            if (content != null)
            {
                bindingContext.HttpContext.Items["contentid"] = content.GetType().GetProperty("Id")?.GetValue(content);
                bindingContext.HttpContext.Items["ezms-content"] = (IContent) content;
                bindingContext.HttpContext.Items["cms-preview"] = version;
            }

            return Task.CompletedTask;
        }

        private async Task<object> GetContent(int? id, int? version, Type type)
        {
            var method = _contentLoader.GetType().GetMethod(nameof(IContentLoader.Get));
            var generic = method.MakeGenericMethod(type);
            var task = (Task)generic.Invoke(_contentLoader, new object[] { id, version });

            await task.ConfigureAwait(false);

            var result = task.GetType().GetProperty("Result");
            return result.GetValue(task);
        }
    }
}
