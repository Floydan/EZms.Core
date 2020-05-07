using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using EZms.Core.Extensions;
using EZms.Core.Helpers;
using EZms.Core.Loaders;
using EZms.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EZms.Core.Authorization
{
    public class EZmsAllowedGroupsAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter, IAllowAnonymous
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            var content = (IContent)context.HttpContext.Items["ezms-content"];
            int.TryParse(context.RouteData.Values["contentid"]?.ToString(), out var contentId);
            if (content == null && contentId > 0)
            {
                var version = 0;
                if (context.HttpContext.Request.Query.ContainsKey("cms-preview"))
                {
                    int.TryParse(context.HttpContext.Request.Query["cms-preview"], out version);
                }

                var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
                content = contentLoader.GetContent(contentId, version == 0 ? (int?)null : version).GetAwaiter().GetResult();
            }

            if (content?.AllowedGroups.IsNullOrEmpty() ?? true)
            {
                return;
            }

            var isAuthenticated = user.Identity?.IsAuthenticated ?? false;

            if (!isAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userRolesIds = Enumerable.Empty<string>();
            var userManager = ServiceLocator.Current.GetInstance<UserManager<IdentityUser>>();
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var identityUser = userManager.FindByIdAsync(userId).GetAwaiter().GetResult();
            if (identityUser != null)
            {
                var userRoles = userManager.GetRolesAsync(identityUser).GetAwaiter().GetResult();
                if (!userRoles.IsNullOrEmpty())
                {
                    var roleManager = ServiceLocator.Current.GetInstance<RoleManager<IdentityRole>>();
                    userRolesIds = roleManager.Roles.Where(w => userRoles.Contains(w.Name, StringComparer.OrdinalIgnoreCase)).Select(w => w.Id);
                }
            }

            if (!content.AllowedGroups.Intersect(userRolesIds).Any())
                context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
        }
    }
}
