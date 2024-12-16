using Hangfire;
using Hangfire.SqlServer;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using settl.identityserver.Appication.Contracts.RepositoryInterfaces;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Application.Contracts.RepositoryInterfaces;
using settl.identityserver.Application.Services;
using settl.identityserver.Dapper;
using settl.identityserver.Domain.Models;
using settl.identityserver.Domain.Shared;
using settl.identityserver.Domain.Shared.Caching;
using settl.identityserver.Domain.Shared.Enums;
using settl.identityserver.Domain.Shared.Helpers;
using settl.identityserver.Domain.Shared.Helpers.Authentication;
using settl.identityserver.EntityFrameworkCore.RepositoryImplementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace settl.identityserver.API
{
    public static class ServiceExtensions
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(error =>
            {
                error.Run(async context =>
                {
                    var responses = CustomApiResponse.Get();
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = 400;
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature is not null)
                    {
                        Log.Error($"Something went wrong in {contextFeature.Error}");

                        await context.Response.WriteAsync(new Error
                        {
                            Code = responses[CustomApiResponse.Status.INTERNAL_ERROR],
                            Message = $"An unexpected error occured. Try Again Later",
                            Errors = new List<string> { contextFeature.Error.Message }
                        }.ToString());
                    }
                });
            });
        }

        public static void ConfigureRepositoryServices(this IServiceCollection services)
        {
            services.AddScoped<IDapper, DapperService>();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddScoped(typeof(IGenericWithoutBaseEntityRepository<>), typeof(GenericWithoutBaseEntityRepository<>));

            services.AddScoped<IAgencyService, AgencyService>();

            services.AddScoped<IOTPService, OTPService>();

            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IEmailService, EmailService>();

            services.AddScoped<IUserService, UserService>();

            services.AddScoped<IConsumerService, ConsumerService>();

            services.AddScoped<ISecurityAnswerService, SecurityAnswerService>();

            services.AddScoped<ISecurityQuestionService, SecurityQuestionService>();

            services.AddScoped<ISmsService, SmsService>();
            services.AddScoped<IJWT, JWT>();

            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IVerificationService, VerificationService>();
            services.AddSingleton<ICacheService, CacheService>();
        }

        public static void ConfigureModelBindingExceptionHandling(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    ValidationProblemDetails error = actionContext.ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .Select(e => new ValidationProblemDetails(actionContext.ModelState)).FirstOrDefault();

                    Log.Error($"{{@RequestPath}} received invalid message format: {{@Exception}}",
                      actionContext.HttpContext.Request.Path.Value,
                      error.Errors.Values);

                    var responses = CustomApiResponse.Get();
                    IActionResult dataResult = null;

                    var r = new Response<object>
                    {
                        Code = responses[CustomApiResponse.Status.INVALID_PARAMETERS],
                        Message = "Some parameters are either missing or invalid.",
                        Data = null,
                        Errors = error.Errors.Values
                    };

                    return dataResult;
                };
            });
        }

        public static void ConfigureHangfireServices(this IServiceCollection services)
        {
            services.AddHangfire(configuration => configuration
              .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
              .UseSimpleAssemblyNameTypeSerializer()
              .UseRecommendedSerializerSettings()
              .UseSqlServerStorage(Constants.HANGFIRE_DB, new SqlServerStorageOptions
              {
                  CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                  SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                  QueuePollInterval = TimeSpan.Zero,
                  UseRecommendedIsolationLevel = true,
                  UsePageLocksOnDequeue = true,
                  DisableGlobalLocks = true,
                  SchemaName = "SettlIdentityServerHangFireDb"
              }));
        }
    }

    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (context.UserName == "settl" && context.Password == "hackathon")
            {
                context.Result = new GrantValidationResult(context.UserName, GrantType.ResourceOwnerPassword);
            }

            return Task.CompletedTask;
        }
    }
}