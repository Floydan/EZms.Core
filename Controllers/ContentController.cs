using System.Net;
using EZms.Core.Authorization;
using EZms.Core.Extensions;
using EZms.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EZms.Core.Controllers
{
    [EZmsAllowedGroupsAuthorize]
    public abstract class ContentController<T> : Controller where T : IContent
    {
        public abstract IActionResult Index(T page);

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.HttpContext.Items["ezms-content"] is IContent content))
            {
                base.OnActionExecuting(context);
                return;
            }

            var includeUnPublished = false;
            if(context.HttpContext.Items["cms-preview"] is int versionId)
            {
                includeUnPublished = versionId > 0;
            }

            var result = content.FilterForVisitor(includeUnPublished: includeUnPublished);

            if (result == null) context.Result = new StatusCodeResult((int)HttpStatusCode.Unauthorized);

            base.OnActionExecuting(context);
        }
    }
}
