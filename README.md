# EZms.Core
This is the Core part of the EZms CMS platform, for more information on using EZms as a whole, please to to [EZms](https://github.com/Floydan/EZms) or read the [EZms wiki](https://github.com/Floydan/EZms/wiki)

To enable the UI you need to follow the instructions for the [EZms.UI](https://github.com/Floydan/EZms.UI/) project

The following needs to be added to the .NET Core startup to enable EZms.Core:

```csharp
public Startup(IConfiguration configuration, IHostingEnvironment environment)
{
    Configuration = configuration;
    HostingEnvironment = environment;
}

public IConfiguration Configuration { get; }
public IHostingEnvironment HostingEnvironment { get; }

public void ConfigureServices(IServiceCollection services) {
    
    services.AddMemoryCache();

    var databaseConnectionString = Configuration.GetConnectionString("ConnectionStringName");

    services.AddDbContextPool<EZmsContext>(options =>
    {
        options.UseSqlServer(databaseConnectionString,
            sqlServerOptionsAction: sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(EZmsContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
    });

    services.AddEZms(config =>
    {
        config.AzureBlobOptions = new AzureBlobOptions {    
            ConnectionString = blobStorageConnectionString,
            DocumentContainer = "files"
        };
    });

    
    services.AddSingleton(HostingEnvironment);

    services.SetupEZmsAutomapper()
            //If needed this is where you add the EZms.UI middleware 
            .SetupEZmsServiceLocator();
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
    
    // EZms.Core middlware
    app.UseEZms();

    //Seed if needed
    using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
    {
        var ezContext = serviceScope.ServiceProvider.GetRequiredService<EZmsContext>();
        ezContext.EnsureSeeded(HostingEnvironment.ContentRootPath);
    }
}

```