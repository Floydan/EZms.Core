using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EZms.Core.Helpers;
using EZms.Core.Repositories;
using Microsoft.AspNetCore.Routing;

namespace EZms.Core.Routing
{
    public class CachedRoute<TPrimaryKey> : IRouter
    {
        private string _controller;
        private readonly string _action;
        private readonly ICachedRouteDataProvider _dataProvider;
        private readonly IRouter _target;
        private readonly ICachedContentTypeControllerMappings _typeMappings;

        public CachedRoute(IRouter target)
        {
            _dataProvider = ServiceLocator.Current.GetInstance<ICachedRouteDataProvider>();
            _action = "Index";
            _target = target;
            _typeMappings = ServiceLocator.Current.GetInstance<ICachedContentTypeControllerMappings>();
        }

        public async Task RouteAsync(RouteContext context)
        {
            var requestPath = context.HttpContext.Request.Path.Value;

            if (!string.IsNullOrEmpty(requestPath) && requestPath[0] == '/')
            {
                // Trim the leading slash
                requestPath = requestPath.Trim(' ', '/');
            }

            // Get the page id that matches.

            //If this returns false, that means the URI did not match
            if (!GetPageList().TryGetValue(requestPath, out var id))
            {
                return;
            }

            //Invoke MVC controller/action
            var routeData = context.RouteData;

            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
            var content = await contentRepository.GetContent(SafeConvert<int>(id));

            var contentType = content.ModelType;
            
            var controllerType = _typeMappings.Get(contentType.ToString());
            if (controllerType != null)
            {
                _controller = controllerType.Name.Replace("Controller", "");
            }

            routeData.Values["controller"] = _controller;
            routeData.Values["action"] = _action;

            // This will be the primary key of the database row.
            // It might be an integer or a GUID.
            routeData.Values["contentid"] = id;

            await _target.RouteAsync(context);
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            VirtualPathData result;

            if (TryFindMatch(GetPageList(), context.Values, out var virtualPath))
            {
                result = new VirtualPathData(this, virtualPath);
            }
            else
            {
                result = _target.GetVirtualPath(context) ?? new VirtualPathData(this, CreateDefaultVirtualPath(context.Values));
            }

            return result;
        }

        private static string CreateDefaultVirtualPath(IDictionary<string, object> values)
        {
            values.TryGetValue("area", out var area);
            values.TryGetValue("controller", out var controller);
            values.TryGetValue("action", out var action);
            values.TryGetValue("id", out var id);

            var extras = new List<KeyValuePair<string,object>>();

            foreach (var valuesKey in values.Keys)
            {
                if (valuesKey != "area" && valuesKey != "controller" && valuesKey != "action" && valuesKey != "id")
                {
                    if (values.TryGetValue(valuesKey, out var extra))
                    {
                        extras.Add(new KeyValuePair<string, object>(valuesKey, extra));
                    }
                }
            }

            var qs = "";
            if (extras.Any())
            {
                qs = $"?{string.Join("&", extras.Select(w => $"{w.Key}={w.Value}"))}";
            }

            if ((string)controller == "Home" && (string)action == "Index")
            {
                controller = "";
                action = "";
            }

            return $"{area}/{controller}/{action}/{id}{qs}".Trim().Trim('/');
        }

        private bool TryFindMatch(IDictionary<string, int> pages, IDictionary<string, object> values, out string virtualPath)
        {
            virtualPath = string.Empty;

            if (!values.TryGetValue("contentid", out var contentId))
            {
                return false;
            }

            var id = SafeConvert<TPrimaryKey>(contentId);
            values.TryGetValue("controller", out var controller);
            values.TryGetValue("action", out var action);

            // The logic here should be the inverse of the logic in 
            // RouteAsync(). So, we match the same controller, action, and id.
            // If we had additional route values there, we would take them all 
            // into consideration during this step.
            if (controller != null && (action != null && (action.Equals(_action) && controller.Equals(_controller))))
            {
                // The 'OrDefault' case returns the default value of the type you're 
                // iterating over. For value types, it will be a new instance of that type. 
                // Since KeyValuePair<TKey, TValue> is a value type (i.e. a struct), 
                // the 'OrDefault' case will not result in a null-reference exception. 
                // Since TKey here is string, the .Key of that new instance will be null.
                virtualPath = pages.FirstOrDefault(x => x.Value.Equals(id)).Key;
                if (!string.IsNullOrEmpty(virtualPath))
                {
                    return true;
                }
            }
            return false;
        }

        private IDictionary<string, int> GetPageList()
        {
            return _dataProvider.GetPageList();
        }

        private static T SafeConvert<T>(object obj)
        {
            if (typeof(T) == typeof(Guid))
            {
                if (obj is string)
                {
                    return (T)(object)new Guid(obj.ToString());
                }
                return (T)(object)Guid.Empty;
            }
            return (T)Convert.ChangeType(obj, typeof(T));
        }
    }
}
