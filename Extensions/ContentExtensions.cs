using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using AutoMapper;
using EZms.Core.Cache;
using EZms.Core.Helpers;
using EZms.Core.Models;
using EZms.Core.Routing;
using EZms.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace EZms.Core.Extensions
{
    public static class ContentExtensions
    {
        public static string GetContentFullUrlSlug<T>(this T content, INavigationService navigationService = null) where T : IContent
        {
            if (content == null) return null;

            var urlSlug = GetContentFullUrlSlug((int?)content.Id);
            if (!string.IsNullOrWhiteSpace(urlSlug)) return urlSlug;

            navigationService = navigationService ?? ServiceLocator.Current.GetInstance<INavigationService>();
            var ancestorPath = navigationService.GetContentAncestorsPath(content.ParentId ?? -1, true).GetAwaiter().GetResult();
            urlSlug = $"{ancestorPath}/{content.UrlSlug}".Trim('/');

            //if (!urlSlug.StartsWith('/')) urlSlug = $"/{urlSlug}";
            return urlSlug;
        }

        public static string GetContentFullUrlSlug(this int? contentId)
        {
            if (!contentId.HasValue) return null;
            var routeDataCache = ServiceLocator.Current.GetInstance<IRouteDataCache>();
            var routeData = routeDataCache.Get();
            if (routeData != null)
            {
                var (key, _) = routeData.FirstOrDefault(w => w.Value == contentId);
                if (key != null) return key;
            }

            return null;
        }

        public static T ToType<T>(this Content content)
        {
            if (content == null) return default(T);

            var mapper = ServiceLocator.Current.GetInstance<IMapper>();
            var mapped = mapper.Map<T>(content);

            var modelProperty = mapped.GetType().GetProperty("Model");

            if (modelProperty == null)
            {
                var mappedType = mapped.GetType();
                var modelType = mappedType.GetProperty("ModelType")?.GetValue(mapped) as Type;
                var modelAsJson = mappedType.GetProperty("ModelAsJson")?.GetValue(mapped) as string;

                var m = JsonConvert.DeserializeObject(modelAsJson, modelType);

                return mapper.Map(mapped, (T)m);
            }

            var model = modelProperty.GetValue(mapped);

            if (model == null) return default(T);

            var final = mapper.Map(mapped, (T)model);

            return final;
        }

        public static T FilterForVisitor<T>(this T content, ClaimsPrincipal principal = null, bool includeUnPublished = false) where T : IContent
        {
            return FilterForVisitor<T>(new[] { content }, principal, includeUnPublished).FirstOrDefault();
        }

        public static IEnumerable<T> FilterForVisitor<T>(this IEnumerable<T> contents, ClaimsPrincipal principal = null, bool includeUnPublished = false) where T : IContent
        {
            if (contents == null) return Enumerable.Empty<T>();

            if (!includeUnPublished)
            {
                var publishedStateAccessor = ServiceLocator.Current.GetInstance<IPublishedStateAccessor>();
                contents = contents.Where(publishedStateAccessor.IsPublished);
            }

            if (principal == null)
            {
                var httpContext = ServiceLocator.Current.GetInstance<IHttpContextAccessor>()?.HttpContext;
                if (httpContext != null)
                {
                    principal = httpContext.User;
                }
            }


            var isAuthenticated = principal?.Identity?.IsAuthenticated ?? false;
            var userRoleIds = new List<string>();
            if (isAuthenticated)
            {
                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var cacheKey = $"EZms:principal:userroles:{userId}";

                if (!EZmsInMemoryCache.TryGetValue(cacheKey, out userRoleIds))
                {
                    var userManager = ServiceLocator.Current.GetInstance<UserManager<IdentityUser>>();
                    var user = userManager.FindByIdAsync(userId).GetAwaiter().GetResult();
                    if (user != null)
                    {
                        var userRoles = userManager.GetRolesAsync(user).GetAwaiter().GetResult().ToList();

                        if (!userRoles.IsNullOrEmpty())
                        {
                            var roleManager = ServiceLocator.Current.GetInstance<RoleManager<IdentityRole>>();
                            userRoleIds = roleManager.Roles
                                .Where(w => userRoles.Contains(w.Name, StringComparer.OrdinalIgnoreCase))
                                .Select(w => w.Id)
                                .ToListAsync().GetAwaiter().GetResult();
                        }
                    }

                    EZmsInMemoryCache.Set(
                        cacheKey,
                        userRoleIds,
                        new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) });
                }
            }

            var filtered = contents.Where(w => w.AllowedGroups.IsNullOrEmpty() || (!w.AllowedGroups.IsNullOrEmpty() && w.AllowedGroups.Intersect(userRoleIds).Any()));

            return filtered;
        }
    }
}
