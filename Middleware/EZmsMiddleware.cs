using System;
using System.Linq;
using AutoMapper;
using EZms.Core.Attributes;
using EZms.Core.Authorization;
using EZms.Core.AzureBlobFileProvider;
using EZms.Core.Extensions;
using EZms.Core.Helpers;
using EZms.Core.Loaders;
using EZms.Core.Middleware.Models;
using EZms.Core.Models;
using EZms.Core.Repositories;
using EZms.Core.Routing;
using EZms.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EZms.Core.Middleware
{
    public static class EZmsMiddleware
    {
        public static IServiceCollection AddEZms(this IServiceCollection services, Action<EZmsConfiguration> configAction = null)
        {
            //services.ConfigureOptions(typeof(EZmsCoreConfigureOptions));

            var implementationInstance = new EZmsConfiguration();
            configAction?.Invoke(implementationInstance);

            if (implementationInstance.RouteDataCache != null)
                services.AddSingleton(implementationInstance.RouteDataCache);
            else
                services.AddSingleton<IRouteDataCache, RouteDataCache>();

            if (implementationInstance.ContentVersionRepository != null)
                services.AddScoped(w => implementationInstance.ContentVersionRepository);
            else
                services.AddScoped<IContentVersionRepository, ContentVersionRepository>();

            if (implementationInstance.ContentRepository != null)
                services.AddScoped(w => implementationInstance.ContentRepository);
            else
                services.AddScoped<IContentRepository, ContentRepository>();

            if (implementationInstance.NavigationService != null)
                services.AddScoped(w => implementationInstance.NavigationService);
            else
                services.AddScoped<INavigationService, NavigationService>();

            if (implementationInstance.CachedRouteDataProvider != null)
                services.AddScoped(w => implementationInstance.CachedRouteDataProvider);
            else
                services.AddScoped<ICachedRouteDataProvider, CachedPageRouteDataProvider>();

            if (implementationInstance.CachedRouteDataProvider != null)
                services.AddScoped(w => implementationInstance.ContentLoader);
            else
                services.AddScoped<IContentLoader, DefaultContentLoader>();


            if (implementationInstance.CachedPageTypeControllerMappings != null)
                services.AddScoped(w => implementationInstance.CachedPageTypeControllerMappings);
            else
                services.AddSingleton<ICachedContentTypeControllerMappings, CachedContentTypeControllerMappings>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IPublishedStateAccessor, PublishedStateAccessor>();

            services.AddScoped(x =>
            {
                var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
                var factory = x.GetRequiredService<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            });

            var azureBlobFileProvider = new AzureBlobFileProvider.AzureBlobFileProvider(implementationInstance.AzureBlobOptions);
            services.AddSingleton(azureBlobFileProvider);

            var provider = new FileExtensionContentTypeProvider();
            services.AddSingleton<IMimeMappingService>(w => new MimeMappingService(provider));
            services.AddTransient<IBlobContainerFactory>(w => new DefaultBlobContainerFactory(implementationInstance.AzureBlobOptions));
            services.AddTransient<IImageUploadService, ImageUploadService>();

            return services;
        }

        public static IServiceCollection SetupEZmsServiceLocator(this IServiceCollection services)
        {
            ServiceLocator.SetLocatorProvider(services.BuildServiceProvider());
            return services;
        }

        public static IServiceCollection SetupEZmsAutomapper(this IServiceCollection services, params Type[] assemblyTypes)
        {
            AutoMapperConfiguration.Init(assemblyTypes);
            services.AddSingleton(AutoMapperConfiguration.Mapper);
            return services;
        }

        public static IApplicationBuilder UseEZms(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<EZmsContext>();
                if (!context.AllMigrationsApplied())
                {
                    context.Database.Migrate();
                }
            }

            return app;
        }

        public static IApplicationBuilder UseEZmsAzureFilesProvider(this IApplicationBuilder app, IHostingEnvironment env)
        {
            var blobFileProvider = app.ApplicationServices.GetRequiredService<AzureBlobFileProvider.AzureBlobFileProvider>();
            if (env.IsDevelopment())
            {
                app.UseDirectoryBrowser(new DirectoryBrowserOptions {
                    FileProvider = blobFileProvider,
                    RequestPath = "/files"
                });
            }

            app.UseStaticFiles(new StaticFileOptions() {
                FileProvider = blobFileProvider,
                RequestPath = "/files"
            });

            return app;
        }

        public static class AutoMapperConfiguration
        {
            public static void Init(params Type[] assemblyTypes)
            {
                MapperConfiguration = new MapperConfiguration(cfg =>
                {
                    if(assemblyTypes != null && assemblyTypes.Length != 0)
                        cfg.AddMaps(assemblyTypes);

                    cfg.CreateMissingTypeMaps = true;

                    var appTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).ToList();
                    var types = appTypes
                        .Where(w => w.IsGenericType && (w.GetGenericTypeDefinition() == typeof(PageContent<>) || w.GetGenericTypeDefinition() == typeof(ProductContent<>)))
                        .Select(w => w);


                    cfg.CreateMap(typeof(ContentVersion), typeof(Content), MemberList.None);
                    cfg.CreateMap(typeof(Content), typeof(ContentVersion), MemberList.None);

                    foreach (var type in types)
                    {
                        cfg.CreateMap(typeof(Content), type, MemberList.None);
                        cfg.CreateMap(type, typeof(Content), MemberList.None);
                        cfg.CreateMap(typeof(ContentVersion), type, MemberList.None);
                        cfg.CreateMap(type, typeof(ContentVersion), MemberList.None);
                    }

                    var pageTypes = appTypes.Where(w => w.IsClass && w.GetCustomAttributes(typeof(PageDataAttribute), false).Any()).Reverse();
                    foreach (var pageType in pageTypes)
                    {
                        cfg.CreateMap(typeof(IContent), pageType, MemberList.None);
                    }

                });

                Mapper = MapperConfiguration.CreateMapper();
            }

            public static IMapper Mapper { get; private set; }

            public static MapperConfiguration MapperConfiguration { get; private set; }
        }
    }
}
