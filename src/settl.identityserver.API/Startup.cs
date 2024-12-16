using Elastic.Apm.NetCoreAll;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using settl.identityserver.Domain.Shared.Enums;
using settl.identityserver.Domain.Shared.Filters;
using settl.identityserver.Domain.Shared.FIlters;
using settl.identityserver.Domain.Shared.Middlewares;
using settl.identityserver.EntityFrameworkCore.AppDbContext;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace settl.identityserver.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            var ConfigBuilder = new ConfigurationBuilder().SetBasePath(webHostEnvironment.ContentRootPath).AddJsonFile("appsettings.json");
            _config = ConfigBuilder.Build();
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
            CertificatePassword = Environment.GetEnvironmentVariable("CertificatePassword");
        }

        private readonly IConfigurationRoot _config;
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment WebHostEnvironment { get; }
        public string CertificatePassword { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            string connStr = Constants.DB_CONNECTION;

            services.AddSingleton(_config);

            services.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer(Constants.DB_CONNECTION));

            services.AddScoped<IApplicationDbContext>(provider => provider.GetService<ApplicationDbContext>());
            var rsaCertificate = new X509Certificate2(Path.Combine(WebHostEnvironment.ContentRootPath, "settlsecurecertificate.pfx"), "3*J6S=$GzN");

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddScoped<ModelValidationFilter>();
            services.AddCors(options => options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin()
                                                                                        .AllowAnyMethod()
                                                                                        .AllowAnyHeader()));
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new TrimStringConverter());
                });

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "settl.identityserver.API", Version = "v1" });
                c.IncludeXmlComments($"{WebHostEnvironment.ContentRootPath}/settl.identityserver.API.xml");
                c.EnableAnnotations();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        Array.Empty<string>()
                    }
                });
            });

            services.AddIdentityServer()
                .AddInMemoryClients(Config.GetClients())
                .AddInMemoryIdentityResources(Config.GetIdentity())
                .AddInMemoryApiScopes(Config.GetScopes())
                .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
                //.AddTestUsers()
                .AddDeveloperSigningCredential();

            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddAutoMapper(typeof(Startup));

            services.AddCertificateManager();

            services.ConfigureRepositoryServices();
            services.AddDistributedMemoryCache();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Constants.ENV_REDIS_CACHE_DB_CONN;
            });

            services.ConfigureModelBindingExceptionHandling();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || env.IsEnvironment("Test") || env.IsStaging() || env.IsEnvironment("staging"))
            {
                app.UseDeveloperExceptionPage();

                #region Configure Swagger

                app.UseSwagger();

                var provider = app.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
                app.UseSwaggerUI(
                    options =>
                    {
                        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                        // build a swagger endpoint for each discovered API version
                        foreach (var description in provider.ApiVersionDescriptions)
                            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                    });

                #endregion Configure Swagger
            }

            app.UseRequestResponseLoggingMiddleware();

            app.ConfigureExceptionHandler();

            app.UseRouting();

            app.UseIdentityServer();

            app.UseCors("AllowAll");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}